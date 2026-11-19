using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maison.BLL.DTOs;

namespace Maison.BLL.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseDto> StageNewProductAsync(CreateStagingProductDto dto, Guid initiatorId);
        Task<ProductResponseDto> StagePriceChangeAsync(SubmitPriceChangeDto dto, Guid initiatorId);
        Task<IEnumerable<ProductResponseDto>> GetStorefrontProductsAsync(string? occasion, string? gender);
        Task<IEnumerable<ProductResponseDto>> GetStagedQueueAsync();
        Task<ProductResponseDto> AuditOfficerVerifyAsync(Guid productId, Guid auditorId, WorkflowGateDecisionDto decision);
        Task<ProductResponseDto> SystemAdminPublishDecisionAsync(Guid productId, Guid adminId, WorkflowGateDecisionDto decision);
    }
}