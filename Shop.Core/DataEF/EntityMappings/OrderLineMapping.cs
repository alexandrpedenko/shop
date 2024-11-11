using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Core.DataEF.Models;

namespace Shop.Core.DataEF.EntityMappings
{
    /// <summary>
    /// Map OrderLine
    /// </summary>
    public class OrderLineMapping : IEntityTypeConfiguration<OrderLineModel>
    {
        public void Configure(EntityTypeBuilder<OrderLineModel> builder)
        {
            builder.HasKey(ol => ol.Id);

            builder.Property(ol => ol.Price)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(ol => ol.Quantity)
                   .IsRequired();

            builder.Property(ol => ol.ProductSKU)
                  .IsRequired()
                  .HasMaxLength(50);

            builder.Property(ol => ol.ProductId)
                   .IsRequired();

            builder.HasOne(ol => ol.Order)
                   .WithMany(o => o.OrderLines)
                   .HasForeignKey(ol => ol.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ol => ol.Product)
                   .WithMany()
                   .HasPrincipalKey(p => p.SKU)
                   .HasForeignKey(ol => ol.ProductSKU)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
