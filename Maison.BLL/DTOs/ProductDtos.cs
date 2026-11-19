using System;
using Maison.Domain.Enums;

namespace Maison.BLL.DTOs
{
    public record CreateStagingProductDto(
        int CategoryId,
        string Title,
        string EditorialStory,
        string FashionEra,
        string Sku,
        decimal CostOfGoodsSold,
        decimal TargetPrice,
        int StockQuantity,
        string? ImageUrl
    );

    public record SubmitPriceChangeDto(Guid ProductId, decimal NewTargetPrice, string Justification);
    public record ProductPublishingDecisionDto(bool Approve, string Reason);
    public record ProductResponseDto(
        Guid Id,
        int CategoryId,
        string CategoryName,
        string Title,
        string Sku,
        decimal Price,
        decimal? StagedPrice,
        decimal CostOfGoodsSold,
        int StockQuantity,
        int InventoryAgeDays,
        bool IsPublished,
        ApprovalStatus PublishingStatus,
        string? ImageUrl
    );
}