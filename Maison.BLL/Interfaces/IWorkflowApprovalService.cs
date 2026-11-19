using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maison.BLL.DTOs;

namespace Maison.BLL.Interfaces
{
    public interface IWorkflowApprovalService
    {
        Task<Guid> ProcessRequestAsync(string requestType, Guid requesterId, decimal amount, string payload);
        Task<bool> ResolveEscalationAsync(Guid id, Guid adminId, bool approve, string reason);
        Task<IEnumerable<WorkflowStatusResponseDto>> GetPendingAuditQueueAsync();
        Task<IEnumerable<WorkflowStatusResponseDto>> GetPendingAdminQueueAsync();
    }
}