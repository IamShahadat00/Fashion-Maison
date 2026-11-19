using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maison.BLL.DTOs;

namespace Maison.BLL.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(Guid? actorId, string actorEmail, string actorRole, string action, string module, string endpoint, string details);
        Task<IEnumerable<AuditLogDto>> GetLogsAsync(int limit = 100);
        Task<FinancialSummaryReportDto> GetFinancialExecutiveReportAsync();
    }
}