using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maison.BLL.DTOs;
using Maison.BLL.Interfaces;
using Maison.DAL.Context;
using Maison.Domain.Entities;
using Maison.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Maison.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly MaisonDbContext _context;
        private readonly IAuditService _auditService;

        public OrderService(MaisonDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<Order> PlaceOrderAsync(Guid customerId, CreateOrderDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("Order must contain at least one valid item.");

            var order = new Order
            {
                CustomerId = customerId,
                ShippingAddress = dto.ShippingAddress,
                Status = OrderStatus.Confirmed,
                PaymentReference = $"PAY-MDL-{Guid.NewGuid().ToString()[..8].ToUpper()}"
            };

            decimal total = 0;
            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId)
                    ?? throw new KeyNotFoundException($"Product {item.ProductId} not found.");

                if (product.StockQuantity < item.Quantity)
                    throw new InvalidOperationException($"Insufficient inventory for '{product.Title}'.");

                product.StockQuantity -= item.Quantity;
                total += product.Price * item.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                });
            }

            order.TotalAmount = total;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(customerId, "customer", UserRoleType.Customer, "ORDER_PLACED", "Checkout", "/api/orders", $"Order {order.Id} placed for ${order.TotalAmount}");

            return order;
        }

        public async Task<IEnumerable<Order>> GetOrdersForRoleAsync(Guid userId, string role)
        {
            var query = _context.Orders.Include(o => o.Items).ThenInclude(i => i.Product).OrderByDescending(o => o.CreatedAt).AsQueryable();

            if (role == UserRoleType.Customer)
                query = query.Where(o => o.CustomerId == userId);

            return await query.ToListAsync();
        }

        public async Task<Order> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto, Guid actorId)
        {
            var order = await _context.Orders.FindAsync(orderId)
                ?? throw new KeyNotFoundException("Order not found.");

            order.Status = dto.Status;
            if (!string.IsNullOrEmpty(dto.TrackingNumber))
                order.TrackingNumber = dto.TrackingNumber;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogActionAsync(actorId, "ops", UserRoleType.OperationsManager, "ORDER_STATUS_CHANGED", "Operations", $"/api/orders/{orderId}/status", $"Status updated to {dto.Status}");

            return order;
        }

        // Multi-Tier Refund Governance Engine
        public async Task<RefundRequest> RequestRefundAsync(RefundSubmissionDto dto, Guid requesterId, string role)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId)
                ?? throw new KeyNotFoundException("Order not found.");

            var refund = new RefundRequest
            {
                OrderId = dto.OrderId,
                CustomerId = order.CustomerId,
                Amount = dto.Amount,
                Reason = dto.Reason
            };

            // Evaluate Refund Governance Thresholds
            if (dto.Amount <= 250m)
            {
                // Tier 1: Auto-Approved
                refund.Tier = RiskTier.Tier1_LowRisk_AutoPublish;
                refund.Status = ApprovalStatus.AutoApproved;
                order.Status = OrderStatus.Refunded;
            }
            else if (dto.Amount <= 1000m)
            {
                // Tier 2: Admin Only
                refund.Tier = RiskTier.Tier2_MediumRisk_AdminOnly;
                refund.Status = ApprovalStatus.PendingAdminApproval;
            }
            else
            {
                // Tier 3: Dual Approval (Audit Officer -> Admin)
                refund.Tier = RiskTier.Tier3_HighRisk_DualApproval;
                refund.Status = ApprovalStatus.PendingAuditVerification;
            }

            _context.RefundRequests.Add(refund);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(requesterId, role, role, "REFUND_REQUESTED", "Returns & Refunds", "/api/orders/refunds", $"Refund ${dto.Amount} requested. Status: {refund.Status}");

            return refund;
        }

        public async Task<RefundRequest> AuditVerifyRefundAsync(Guid refundId, Guid auditorId, WorkflowGateDecisionDto decision)
        {
            var refund = await _context.RefundRequests.FindAsync(refundId)
                ?? throw new KeyNotFoundException("Refund request not found.");

            if (refund.Status != ApprovalStatus.PendingAuditVerification)
                throw new InvalidOperationException("Refund is not in Pending Audit status.");

            refund.AuditOfficerId = auditorId;
            refund.AuditNotes = decision.Reason;
            refund.AuditVerifiedAt = DateTime.UtcNow;

            if (decision.Approved)
            {
                refund.Status = ApprovalStatus.PendingAdminApproval; // Passed Gate 1 -> Routed to Admin
            }
            else
            {
                refund.Status = ApprovalStatus.AuditFlaggedDeclined;
            }

            await _context.SaveChangesAsync();
            return refund;
        }

        public async Task<RefundRequest> AdminDecideRefundAsync(Guid refundId, Guid adminId, WorkflowGateDecisionDto decision)
        {
            var refund = await _context.RefundRequests.Include(r => r.Order).FirstOrDefaultAsync(r => r.Id == refundId)
                ?? throw new KeyNotFoundException("Refund request not found.");

            if (refund.Status != ApprovalStatus.PendingAdminApproval)
                throw new InvalidOperationException("Refund is not in Pending Admin Approval status.");

            refund.AdminId = adminId;
            refund.AdminNotes = decision.Reason;
            refund.AdminDecidedAt = DateTime.UtcNow;

            if (decision.Approved)
            {
                refund.Status = ApprovalStatus.PublishedLive;
                refund.Order.Status = OrderStatus.Refunded;
            }
            else
            {
                refund.Status = ApprovalStatus.AdminDeclined;
            }

            await _context.SaveChangesAsync();
            return refund;
        }
    }
}