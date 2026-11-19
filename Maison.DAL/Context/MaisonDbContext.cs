using Maison.DAL.Configurations;
using Maison.Domain.Entities;
using Maison.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Maison.DAL.Context
{
    public class MaisonDbContext : DbContext
    {
        public MaisonDbContext(DbContextOptions<MaisonDbContext> options) : base(options) { }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<RefundRequest> RefundRequests => Set<RefundRequest>();
        public DbSet<AuditLog> SystemAuditLogs => Set<AuditLog>();
        public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
        public DbSet<PromotionBanner> PromotionBanners => Set<PromotionBanner>();
        public DbSet<WorkflowApproval> WorkflowApprovals => Set<WorkflowApproval>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new OrderConfiguration());

            // Decimal Precision
            modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<RefundRequest>().Property(r => r.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<PromotionBanner>().Property(pb => pb.DiscountPercentage).HasPrecision(5, 2);
            modelBuilder.Entity<EmployeeProfile>().Property(ep => ep.PerformanceScore).HasPrecision(5, 2);
            modelBuilder.Entity<WorkflowApproval>().Property(approval => approval.ThresholdValue).HasPrecision(18, 2);
            modelBuilder.Entity<WorkflowApproval>()
                .HasOne(approval => approval.RequestedBy)
                .WithMany()
                .HasForeignKey(approval => approval.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WorkflowApproval>()
                .HasOne(approval => approval.AdminReviewer)
                .WithMany()
                .HasForeignKey(approval => approval.AdminReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = UserRoleType.SystemAdmin, Description = "Ultimate administrative authority and global sign-off." },
                new Role { Id = 2, Name = UserRoleType.ProductManager, Description = "Catalog, staging submissions, and inventory control." },
                new Role { Id = 3, Name = UserRoleType.MarketingSales, Description = "Promotional campaigns, discounts, and visual banners." },
                new Role { Id = 4, Name = UserRoleType.OperationsManager, Description = "Logistics, fulfillment, and warehouse stock maintenance." },
                new Role { Id = 5, Name = UserRoleType.CustomerService, Description = "Customer support, inquiry management, and refund requests." },
                new Role { Id = 6, Name = UserRoleType.AuditOfficer, Description = "Compliance review, Tier 3 COGS & margin verification." },
                new Role { Id = 7, Name = UserRoleType.HRManager, Description = "Workforce management and role allocation requests." },
                new Role { Id = 8, Name = UserRoleType.Customer, Description = "Purchaser and storefront client." }
            );

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Summer Silk Couture", GenderSector = "Female", Occasion = "Summer", Description = "Bespoke linen and airy silk silhouettes." },
                new Category { Id = 2, Name = "Winter Monolith Overcoats", GenderSector = "Male", Occasion = "Winter", Description = "Double-faced cashmere and architectural wool coats." },
                new Category { Id = 3, Name = "Eid Royal Heritage", GenderSector = "Unisex", Occasion = "Eid", Description = "Artisanal hand-embroidered luxury garments." },
                new Category { Id = 4, Name = "Puja Golden Weave", GenderSector = "Female", Occasion = "Puja", Description = "Traditional festive weaving merged with avant-garde cuts." },
                new Category { Id = 5, Name = "Executive Atelier", GenderSector = "Unisex", Occasion = "Office", Description = "Tailored modern formal wear." }
            );
        }
    }
}