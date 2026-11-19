using System.Threading.Tasks;
using Maison.BLL.Interfaces;
using Maison.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maison.API.Controllers
{
    [ApiController]
    [Route("api/WorkflowApprovals")]
    [Authorize]
    public class WorkflowApprovalQueuesController : ControllerBase
    {
        private readonly IWorkflowApprovalService _workflowService;

        public WorkflowApprovalQueuesController(IWorkflowApprovalService workflowService)
        {
            _workflowService = workflowService;
        }

        // Gate 1 Escalation Queue for Audit Officer
        [Authorize(Roles = $"{UserRoleType.SystemAdmin},{UserRoleType.AuditOfficer}")]
        [HttpGet("audit-queue")]
        public async Task<IActionResult> GetAuditQueue() =>
            Ok(await _workflowService.GetPendingAuditQueueAsync());

        // Gate 2 Escalation Queue for System Admin
        [Authorize(Roles = UserRoleType.SystemAdmin)]
        [HttpGet("admin-queue")]
        public async Task<IActionResult> GetAdminQueue() =>
            Ok(await _workflowService.GetPendingAdminQueueAsync());
    }
}