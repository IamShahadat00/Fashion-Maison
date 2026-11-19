using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Maison.BLL.DTOs;
using Maison.BLL.Interfaces;
using Maison.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maison.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // Public Storefront (R)
        [HttpGet]
        public async Task<IActionResult> GetStorefront([FromQuery] string? occasion, [FromQuery] string? gender) =>
            Ok(await _productService.GetStorefrontProductsAsync(occasion, gender));

        // Staging Queue for Internal Teams
        [Authorize(Roles = $"{UserRoleType.SystemAdmin},{UserRoleType.ProductManager},{UserRoleType.AuditOfficer}")]
        [HttpGet("staging-queue")]
        public async Task<IActionResult> GetStagingQueue() =>
            Ok(await _productService.GetStagedQueueAsync());

        // Stage New Product (Product Manager)
        [Authorize(Roles = $"{UserRoleType.SystemAdmin},{UserRoleType.ProductManager}")]
        [HttpPost("stage")]
        public async Task<IActionResult> StageProduct([FromBody] CreateStagingProductDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _productService.StageNewProductAsync(dto, userId);
            return Ok(result);
        }

        // Stage Price Adjustment (Evaluates Threshold Matrix: Tier 1, 2, or 3)
        [Authorize(Roles = $"{UserRoleType.SystemAdmin},{UserRoleType.ProductManager}")]
        [HttpPost("price-change")]
        public async Task<IActionResult> StagePriceChange([FromBody] SubmitPriceChangeDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _productService.StagePriceChangeAsync(dto, userId);
            return Ok(result);
        }

        // Gate 1: Audit Officer Financial Verification (COGS & Margin)
        [Authorize(Roles = $"{UserRoleType.SystemAdmin},{UserRoleType.AuditOfficer}")]
        [HttpPost("{id:guid}/audit-verify")]
        public async Task<IActionResult> AuditVerify(Guid id, [FromBody] WorkflowGateDecisionDto decision)
        {
            var auditorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _productService.AuditOfficerVerifyAsync(id, auditorId, decision);
            return Ok(result);
        }

        // Gate 2: System Admin Executive Sign-Off & Live Storefront Publish
        [Authorize(Roles = UserRoleType.SystemAdmin)]
        [HttpPost("{id:guid}/admin-publish")]
        public async Task<IActionResult> AdminPublish(Guid id, [FromBody] WorkflowGateDecisionDto decision)
        {
            var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _productService.SystemAdminPublishDecisionAsync(id, adminId, decision);
            return Ok(result);
        }
    }
}