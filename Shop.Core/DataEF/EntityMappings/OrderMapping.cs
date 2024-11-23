using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Core.DataEF.Models;

namespace Shop.Core.DataEF.EntityMappings
{
    /// <summary>
    /// Map Order
    /// </summary>
    public class OrderMapping : IEntityTypeConfiguration<OrderModel>
    {
        public void Configure(EntityTypeBuilder<OrderModel> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderDate)
                   .IsRequired()
                   .HasColumnType("datetime");

            builder.HasMany(o => o.OrderLines)
                   .WithOne(ol => ol.Order)
                   .HasForeignKey(ol => ol.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(o => o.TotalPrice)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(o => o.DiscountPercentage)
                   .IsRequired()
                   .HasColumnType("decimal(5,2)")
                   .HasDefaultValue(0);
        }
    }
}
