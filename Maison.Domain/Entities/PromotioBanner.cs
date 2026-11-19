using System;
using Maison.Domain.Enums;

namespace Maison.Domain.Entities
{
    public class PromotionBanner
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string PromoCode { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public RiskTier Tier { get; set; }
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Draft;
        public bool IsPopupActive { get; set; } = false;
        
        public Guid? CreatedById { get; set; }
        public Guid? AuditOfficerId { get; set; }
        public string? AuditNotes { get; set; }
        public DateTime? AuditVerifiedAt { get; set; }

        public Guid? AdminReviewerId { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? AdminDecidedAt { get; set; }

        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
        public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(30);
    }
}