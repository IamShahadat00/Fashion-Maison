using System.Threading.Tasks;
using Maison.BLL.Interfaces;
using Maison.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maison.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{UserRoleType.SystemAdmin},{UserRoleType.AuditOfficer}")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditLogsController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] int limit = 100) =>
            Ok(await _auditService.GetLogsAsync(limit));
    }
}