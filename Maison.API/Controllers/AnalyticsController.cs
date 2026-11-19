using System.Threading.Tasks;
using Maison.BLL.Interfaces;
using Maison.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maison.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{UserRoleType.SystemAdmin},{UserRoleType.ProductManager},{UserRoleType.MarketingSales},{UserRoleType.OperationsManager},{UserRoleType.AuditOfficer}")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AnalyticsController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("financial-summary")]
        public async Task<IActionResult> GetFinancialSummary() =>
            Ok(await _auditService.GetFinancialExecutiveReportAsync());
    }
}