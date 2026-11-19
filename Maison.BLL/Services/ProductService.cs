using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maison.BLL.DTOs;
using Maison.BLL.Interfaces;
using Maison.BLL.Mappings;
using Maison.DAL.Context;
using Maison.Domain.Entities;
using Maison.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Maison.BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly MaisonDbContext _context;
        private readonly IAuditService _auditService;

        public ProductService(MaisonDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // STAGE 1 & 2: Staging Submission & Automated Threshold Policy Check
        public async Task<ProductResponseDto> StageNewProductAsync(CreateStagingProductDto dto, Guid initiatorId)
        {
            if (await _context.Products.AnyAsync(p => p.Sku == dto.Sku))
                throw new InvalidOperationException($"SKU '{dto.Sku}' already exists.");

            var product = new Product
            {
                CategoryId = dto.CategoryId,
                Title = dto.Title,
                EditorialStory = dto.EditorialStory,
                FashionEra = dto.FashionEra,
                Sku = dto.Sku,
                CostOfGoodsSold = dto.CostOfGoodsSold,
                Price = dto.TargetPrice,
                StagedPrice = dto.TargetPrice,
                StockQuantity = dto.StockQuantity,
                ImageUrl = dto.ImageUrl,
                CreatedById = initiatorId,
                IsPublished = false
            };

            // Evaluate Threshold: High-value couture or custom launch
            bool isHighImpact = dto.TargetPrice > 1500m || dto.StockQuantity > 500;
            if (isHighImpact)
            {
                product.PublishingStatus = ApprovalStatus.PendingAdminApproval;
            }
            else
            {
                // Auto-publish standard catalog items
                product.IsPublished = true;
                product.PublishingStatus = ApprovalStatus.PublishedLive;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(initiatorId, "initiator", UserRoleType.ProductManager,
                product.IsPublished ? "PRODUCT_AUTO_PUBLISHED" : "PRODUCT_STAGED_FOR_APPROVAL",
                "Product Catalog & Pricing",
                "/api/products/stage",
                $"SKU {product.Sku} staged. AutoPublished: {product.IsPublished}");

            return (await _context.Products.Include(p => p.Category).FirstAsync(p => p.Id == product.Id)).ToDto();
        }

        public async Task<ProductResponseDto> StagePriceChangeAsync(SubmitPriceChangeDto dto, Guid initiatorId)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == dto.ProductId)
                ?? throw new KeyNotFoundException("Product not found.");

            decimal oldPrice = product.Price;
            decimal newPrice = dto.NewTargetPrice;
            decimal percentageDelta = oldPrice > 0 ? ((newPrice - oldPrice) / oldPrice) * 100m : 0m;
            product.StagedPrice = newPrice;

            // EVALUATE MULTI-TIER THRESHOLD GOVERNANCE
            if (newPrice < oldPrice)
            {
                decimal reductionPercent = Math.Abs(percentageDelta);
                bool belowCostMargin = newPrice < product.CostOfGoodsSold;

                if (reductionPercent > 10m || belowCostMargin)
                {
                    // TIER 3: High Risk -> Gate 1: Audit Officer Review
                    product.PublishingStatus = ApprovalStatus.PendingAuditVerification;
                }
                else if (reductionPercent > 3m)
                {
                    // TIER 2: Medium Risk -> Admin Only Review
                    product.PublishingStatus = ApprovalStatus.PendingAdminApproval;
                }
                else
                {
                    // TIER 1: Low Risk (<= 3%) -> Auto-Publish
                    product.Price = newPrice;
                    product.StagedPrice = null;
                    product.PublishingStatus = ApprovalStatus.PublishedLive;
                }
            }
            else // Price Increase
            {
                if (percentageDelta > 15m)
                {
                    // TIER 3: Market shock check -> Audit Officer first
                    product.PublishingStatus = ApprovalStatus.PendingAuditVerification;
                }
                else if (percentageDelta > 5m)
                {
                    // TIER 2: Medium Risk -> Admin Only
                    product.PublishingStatus = ApprovalStatus.PendingAdminApproval;
                }
                else
                {
                    // TIER 1: Low Risk (<= 5%) -> Auto-Publish
                    product.Price = newPrice;
                    product.StagedPrice = null;
                    product.PublishingStatus = ApprovalStatus.PublishedLive;
                }
            }

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(initiatorId, "initiator", UserRoleType.ProductManager,
                $"PRICE_CHANGE_STAGED_{product.PublishingStatus}",
                "Product Catalog & Pricing",
                "/api/products/price-change",
                $"Product {product.Sku} price change {oldPrice} -> {newPrice} ({percentageDelta:F1}%). Status: {product.PublishingStatus}");

            return product.ToDto();
        }

        // STAGE 2: Gate 1 - Audit Officer Financial & COGS Verification
        public async Task<ProductResponseDto> AuditOfficerVerifyAsync(Guid productId, Guid auditorId, WorkflowGateDecisionDto decision)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new KeyNotFoundException("Product not found.");

            if (product.PublishingStatus != ApprovalStatus.PendingAuditVerification)
                throw new InvalidOperationException("Product is not currently pending Audit Officer verification.");

            if (decision.Approved)
            {
                // Gate 1 Cleared -> Move to Gate 2 (System Admin)
                product.PublishingStatus = ApprovalStatus.PendingAdminApproval;
            }
            else
            {
                // Audit Flagged -> Revert to Draft
                product.PublishingStatus = ApprovalStatus.AuditFlaggedDeclined;
                product.StagedPrice = null;
            }

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(auditorId, "auditor", UserRoleType.AuditOfficer,
                decision.Approved ? "GATE1_AUDIT_PASSED" : "GATE1_AUDIT_FLAGGED",
                "Audit Officer Review",
                $"/api/products/{productId}/audit-verify",
                $"Audit Decision: {decision.Reason}");

            return product.ToDto();
        }

        // STAGE 3: Gate 2 - System Admin Final Executive Sign-off
        public async Task<ProductResponseDto> SystemAdminPublishDecisionAsync(Guid productId, Guid adminId, WorkflowGateDecisionDto decision)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new KeyNotFoundException("Product not found.");

            if (product.PublishingStatus != ApprovalStatus.PendingAdminApproval)
                throw new InvalidOperationException("Product is not in Pending Admin Approval status.");

            if (decision.Approved)
            {
                if (product.StagedPrice.HasValue)
                {
                    product.Price = product.StagedPrice.Value;
                    product.StagedPrice = null;
                }
                product.IsPublished = true;
                product.PublishingStatus = ApprovalStatus.PublishedLive;
            }
            else
            {
                product.PublishingStatus = ApprovalStatus.AdminDeclined;
                product.StagedPrice = null;
            }

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(adminId, "admin", UserRoleType.SystemAdmin,
                decision.Approved ? "GATE2_ADMIN_PUBLISHED_LIVE" : "GATE2_ADMIN_DECLINED",
                "System Admin Sign-off",
                $"/api/products/{productId}/admin-publish",
                $"Admin Decision: {decision.Reason}");

            return product.ToDto();
        }

        public async Task<IEnumerable<ProductResponseDto>> GetStorefrontProductsAsync(string? occasion, string? gender)
        {
            var query = _context.Products.Include(p => p.Category).Where(p => p.IsPublished && p.PublishingStatus == ApprovalStatus.PublishedLive).AsQueryable();

            if (!string.IsNullOrEmpty(occasion))
                query = query.Where(p => p.Category.Occasion.ToLower() == occasion.ToLower());

            if (!string.IsNullOrEmpty(gender))
                query = query.Where(p => p.Category.GenderSector.ToLower() == gender.ToLower());

            return await query.Select(p => p.ToDto()).ToListAsync();
        }

        public async Task<IEnumerable<ProductResponseDto>> GetStagedQueueAsync()
        {
            return await _context.Products.Include(p => p.Category)
                .Where(p => p.PublishingStatus != ApprovalStatus.PublishedLive)
                .Select(p => p.ToDto())
                .ToListAsync();
        }
    }
}