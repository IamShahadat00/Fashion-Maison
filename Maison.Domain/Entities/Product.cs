using System;
using System.Collections.Generic;
using Maison.Domain.Enums;

namespace Maison.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string EditorialStory { get; set; } = string.Empty;
        public string FashionEra { get; set; } = "Modern Avant-Garde";
        public string Sku { get; set; } = string.Empty;
        public decimal CostOfGoodsSold { get; set; }  // COGS for margin guardrail checks
        public decimal Price { get; set; }             // Current Selling Price
        public decimal? StagedPrice { get; set; }       // Sandbox/Staging Target Price
        public int StockQuantity { get; set; }
        public int InventoryAgeDays { get; set; } = 0; // For Clearance sale guardrails (>180 days)
        public string? ImageUrl { get; set; }
        public bool IsPublished { get; set; } = false; // Hidden from store until published
        public ApprovalStatus PublishingStatus { get; set; } = ApprovalStatus.Draft;
        public Guid? CreatedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}