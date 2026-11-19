using System;

namespace Maison.BLL.DTOs
{
    public record AuditLogDto(
        long Id,
        Guid? ActorId,
        string ActorEmail,
        string ActorRole,
        string Action,
        string Module,
        string Endpoint,
        string Details,
        DateTime Timestamp
    );

    public record FinancialSummaryReportDto(
        decimal TotalRevenue,
        decimal NetMarginForecast,
        int ActiveOrdersCount,
        int StagedPendingApprovals,
        int Tier3PendingAuditCount,
        int Tier2PendingAdminCount
    );
}