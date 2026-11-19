using Maison.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maison.DAL.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.Sku).IsUnique();
            builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
            builder.Property(p => p.Price).HasPrecision(18, 2);
            builder.Property(p => p.StagedPrice).HasPrecision(18, 2);
            builder.Property(p => p.CostOfGoodsSold).HasPrecision(18, 2);
            builder.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}