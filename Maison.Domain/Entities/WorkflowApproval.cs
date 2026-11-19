using System;

namespace Maison.Domain.Entities
{
    public class WorkflowApproval
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RequestType { get; set; } = string.Empty;
        public Guid RequestedById { get; set; }
        public User? RequestedBy { get; set; }
        public string PayloadJson { get; set; } = "{}";
        public decimal? ThresholdValue { get; set; }
        public string ApprovalStatus { get; set; } = "Pending"; // AutoApproved, Pending, Approved, Declined
        public Guid? AdminReviewerId { get; set; }
        public User? AdminReviewer { get; set; }
        public string? AdminReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }
}