using Maison.BLL.DTOs;
using Maison.Domain.Entities;

namespace Maison.BLL.Mappings
{
    public static class MappingExtensions
    {
        public static ProductResponseDto ToDto(this Product p)
        {
            return new ProductResponseDto(
                p.Id,
                p.CategoryId,
                p.Category?.Name ?? string.Empty,
                p.Title,
                p.Sku,
                p.Price,
                p.StagedPrice,
                p.CostOfGoodsSold,
                p.StockQuantity,
                p.InventoryAgeDays,
                p.IsPublished,
                p.PublishingStatus,
                p.ImageUrl
            );
        }

        public static AuditLogDto ToDto(this AuditLog a)
        {
            return new AuditLogDto(
                a.Id,
                a.ActorId,
                a.ActorEmail,
                a.ActorRole,
                a.Action,
                a.Module,
                a.Endpoint,
                a.Details,
                a.Timestamp
            );
        }
    }
}