namespace Maison.Domain.Enums
{
    public enum ApprovalStatus
    {
        Draft,
        AutoApproved,
        PendingAuditVerification, // Tier 3 Gate 1
        PendingAdminApproval,     // Tier 2 or Tier 3 Gate 2
        AuditFlaggedDeclined,
        AdminDeclined,
        PublishedLive
    }
}