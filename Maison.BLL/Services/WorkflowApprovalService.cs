using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maison.BLL.DTOs;
using Maison.BLL.Interfaces;
using Maison.DAL.Context;
using Maison.Domain.Enums;
using Maison.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maison.BLL.Services
{
    public class WorkflowApprovalService : IWorkflowApprovalService
    {
        private const string PendingStatus = "Pending";

        private readonly MaisonDbContext _context;

        public WorkflowApprovalService(MaisonDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> ProcessRequestAsync(string requestType, Guid requesterId, decimal amount, string payload)
        {
            var request = new WorkflowApproval
            {
                RequestType = requestType,
                RequestedById = requesterId,
                ThresholdValue = amount,
                PayloadJson = payload,
                ApprovalStatus = PendingStatus
            };

            _context.WorkflowApprovals.Add(request);
            await _context.SaveChangesAsync();

            return request.Id;
        }

        public async Task<bool> ResolveEscalationAsync(Guid id, Guid adminId, bool approve, string reason)
        {
            var request = await _context.WorkflowApprovals
                .SingleOrDefaultAsync(item => item.Id == id && item.ApprovalStatus == PendingStatus);

            if (request is null)
            {
                return false;
            }

            request.AdminReviewerId = adminId;
            request.AdminReason = reason;
            request.ReviewedAt = DateTime.UtcNow;
            request.ApprovalStatus = approve ? "Approved" : "Declined";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<WorkflowStatusResponseDto>> GetPendingAuditQueueAsync()
        {
            var list = new List<WorkflowStatusResponseDto>();

            var products = await _context.Products.Where(p => p.PublishingStatus == ApprovalStatus.PendingAuditVerification).ToListAsync();
            list.AddRange(products.Select(p => new WorkflowStatusResponseDto(
                p.Id,
                "Product Price Delta",
                RiskTier.Tier3_HighRisk_DualApproval,
                p.PublishingStatus,
                "Product Manager",
                $"SKU: {p.Sku}, Current: ${p.Price}, Proposed: ${p.StagedPrice}, COGS: ${p.CostOfGoodsSold}",
                p.UpdatedAt
            )));

            var refunds = await _context.RefundRequests.Where(r => r.Status == ApprovalStatus.PendingAuditVerification).ToListAsync();
            list.AddRange(refunds.Select(r => new WorkflowStatusResponseDto(
                r.Id,
                "High-Value Customer Refund",
                RiskTier.Tier3_HighRisk_DualApproval,
                r.Status,
                "Customer Service",
                $"Order: {r.OrderId}, Amount: ${r.Amount}, Reason: {r.Reason}",
                r.CreatedAt
            )));

            return list;
        }

        public async Task<IEnumerable<WorkflowStatusResponseDto>> GetPendingAdminQueueAsync()
        {
            var list = new List<WorkflowStatusResponseDto>();

            var products = await _context.Products.Where(p => p.PublishingStatus == ApprovalStatus.PendingAdminApproval).ToListAsync();
            list.AddRange(products.Select(p => new WorkflowStatusResponseDto(
                p.Id,
                "Commercial Product Staging",
                RiskTier.Tier2_MediumRisk_AdminOnly,
                p.PublishingStatus,
                "Product Manager / Audit Officer Cleared",
                $"SKU: {p.Sku}, Staged Price: ${p.StagedPrice ?? p.Price}",
                p.UpdatedAt
            )));

            var refunds = await _context.RefundRequests.Where(r => r.Status == ApprovalStatus.PendingAdminApproval).ToListAsync();
            list.AddRange(refunds.Select(r => new WorkflowStatusResponseDto(
                r.Id,
                "Order Refund Release",
                RiskTier.Tier2_MediumRisk_AdminOnly,
                r.Status,
                "CS / Audit Verified",
                $"Order: {r.OrderId}, Amount: ${r.Amount}",
                r.CreatedAt
            )));

            var escalations = await _context.WorkflowApprovals
                .Where(request => request.ApprovalStatus == PendingStatus)
                .ToListAsync();
            list.AddRange(escalations.Select(request => new WorkflowStatusResponseDto(
                request.Id,
                request.RequestType,
                RiskTier.Tier2_MediumRisk_AdminOnly,
                ApprovalStatus.PendingAdminApproval,
                request.RequestedById.ToString(),
                $"Amount: {request.ThresholdValue}, Payload: {request.PayloadJson}",
                request.CreatedAt
            )));

            return list;
        }
    }
}