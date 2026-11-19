using System;
using Maison.Domain.Enums;

namespace Maison.BLL.DTOs
{
    public record WorkflowGateDecisionDto(bool Approved, string Reason);
    public record WorkflowStatusResponseDto(
        Guid ItemId,
        string ItemType,
        RiskTier RiskTier,
        ApprovalStatus Status,
        string InitiatedBy,
        string Details,
        DateTime CreatedAt
    );
}