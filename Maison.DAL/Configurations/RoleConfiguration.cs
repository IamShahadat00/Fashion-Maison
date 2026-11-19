using Maison.Domain.Entities;
using Maison.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maison.DAL.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Name).HasMaxLength(50).IsRequired();
            builder.Property(r => r.Description).HasMaxLength(255);
            builder.Property(r => r.IsExecutive).IsRequired();

            // Seed Roles
            builder.HasData(
                new Role { Id = 1, Name = UserRoleType.SystemAdmin, Description = "Super Administrator", IsExecutive = true },
                new Role { Id = 2, Name = UserRoleType.ProductManager, Description = "Catalog & Pricing Manager", IsExecutive = true },
                new Role { Id = 3, Name = UserRoleType.MarketingSales, Description = "Marketing & Campaigns", IsExecutive = true },
                new Role { Id = 4, Name = UserRoleType.OperationsManager, Description = "Logistics & Fulfillment", IsExecutive = true },
                new Role { Id = 5, Name = UserRoleType.CustomerService, Description = "Support & Ticket Escalations", IsExecutive = true },
                new Role { Id = 6, Name = UserRoleType.AuditOfficer, Description = "Compliance & Financial Gatekeeper", IsExecutive = true },
                new Role { Id = 7, Name = UserRoleType.HRManager, Description = "Human Resource Operations", IsExecutive = true },
                new Role { Id = 8, Name = UserRoleType.Customer, Description = "E-Commerce Customer", IsExecutive = false }
            );
        }
    }
}