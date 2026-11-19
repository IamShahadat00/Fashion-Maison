using Microsoft.AspNetCore.Mvc;
using Maison.BLL.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Maison.BLL.Interfaces;
using Maison.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Maison.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkflowApprovalsController : ControllerBase
    {
        private readonly IWorkflowApprovalService _workflowService;

        public WorkflowApprovalsController(IWorkflowApprovalService workflowService)
        {
            _workflowService = workflowService;
        }

        public record CreateEscalationDto(string RequestType, Guid RequesterId, decimal Amount, string Payload);
        public record ResolveEscalationDto(Guid AdminId, bool Approve, string Reason);

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] CreateEscalationDto dto)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var requesterId))
            {
                return Unauthorized();
            }

            if (dto.RequesterId != requesterId)
            {
                return Forbid();
            }

            var id = await _workflowService.ProcessRequestAsync(dto.RequestType, requesterId, dto.Amount, dto.Payload);
            return Ok(new { id, status = "Pending" });
        }

        [Authorize(Roles = UserRoleType.SystemAdmin)]
        [HttpPost("{id:guid}/resolve")]
        public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveEscalationDto dto)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId))
            {
                return Unauthorized();
            }

            if (dto.AdminId != adminId)
            {
                return Forbid();
            }

            var success = await _workflowService.ResolveEscalationAsync(id, adminId, dto.Approve, dto.Reason);
            if (!success) return BadRequest(new { message = "Could not resolve request or already finalized." });
            return Ok(new { message = "Workflow action recorded and executed." });
        }
    }
}