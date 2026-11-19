using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maison.BLL.DTOs;
using Maison.BLL.Interfaces;
using Maison.BLL.Mappings;
using Maison.DAL.Context;
using Maison.Domain.Entities;
using Maison.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Maison.BLL.Services
{
    public class AuditService : IAuditService
    {
        private readonly MaisonDbContext _context;

        public AuditService(MaisonDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(Guid? actorId, string actorEmail, string actorRole, string action, string module, string endpoint, string details)
        {
            _context.SystemAuditLogs.Add(new AuditLog
            {
                ActorId = actorId,
                ActorEmail = actorEmail,
                ActorRole = actorRole,
                Action = action,
                Module = module,
                Endpoint = endpoint,
                Details = details,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLogDto>> GetLogsAsync(int limit = 100)
        {
            return await _context.SystemAuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .Select(l => l.ToDto())
                .ToListAsync();
        }

        public async Task<FinancialSummaryReportDto> GetFinancialExecutiveReportAsync()
        {
            var validOrders = await _context.Orders.Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded).ToListAsync();
            var totalRevenue = validOrders.Sum(o => o.TotalAmount);

            var pendingAudit = await _context.Products.CountAsync(p => p.PublishingStatus == ApprovalStatus.PendingAuditVerification)
                             + await _context.RefundRequests.CountAsync(r => r.Status == ApprovalStatus.PendingAuditVerification);

            var pendingAdmin = await _context.Products.CountAsync(p => p.PublishingStatus == ApprovalStatus.PendingAdminApproval)
                             + await _context.RefundRequests.CountAsync(r => r.Status == ApprovalStatus.PendingAdminApproval);

            var stagedProducts = await _context.Products.CountAsync(p => p.PublishingStatus != ApprovalStatus.PublishedLive);

            // Forward looking margin calculation (simulated 18% net forecast based on active catalog)
            decimal forecast = Math.Round(totalRevenue * 1.18m, 2);

            return new FinancialSummaryReportDto(
                totalRevenue,
                forecast,
                validOrders.Count,
                stagedProducts,
                pendingAudit,
                pendingAdmin
            );
        }
    }
}