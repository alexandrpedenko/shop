using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Core.DataEF.Models;

namespace Shop.Core.DataEF.EntityMappings
{
    /// <summary>
    /// Map Product
    /// </summary>
    public class ProductMapping : IEntityTypeConfiguration<ProductModel>
    {
        /// <summary>
        /// Configure Product table mapping
        /// </summary>
        public void Configure(EntityTypeBuilder<ProductModel> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Title)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("Title");

            builder.Property(p => p.Description)
                   .HasMaxLength(500)
                   .HasColumnName("Description");

            builder.Property(p => p.Price)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(p => p.SKU)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasColumnName("SKU");

            builder.HasIndex(p => p.SKU)
                   .IsUnique();

            builder.HasAlternateKey(p => p.SKU);
        }
    }
}
