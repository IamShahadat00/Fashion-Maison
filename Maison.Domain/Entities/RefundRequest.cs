using System;
using Maison.Domain.Enums;

namespace Maison.Domain.Entities
{
    public class RefundRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public Guid CustomerId { get; set; }
        public User Customer { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public RiskTier Tier { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.PendingAdminApproval;
        
        // Gate 1: Audit Officer Sign-off (if Tier 3)
        public Guid? AuditOfficerId { get; set; }
        public User? AuditOfficer { get; set; }
        public string? AuditNotes { get; set; }
        public DateTime? AuditVerifiedAt { get; set; }

        // Gate 2: System Admin Sign-off
        public Guid? AdminId { get; set; }
        public User? Admin { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? AdminDecidedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}