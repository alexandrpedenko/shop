using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shop.Core.DataEF.EntityMappings;
using Shop.Core.DataEF.Models;

namespace Shop.Core.DataEF
{
    /// <summary>
    /// Db context
    /// </summary>
    public class ShopContext(DbContextOptions<ShopContext> options) : IdentityDbContext<IdentityUser>(options)
    {
        /// <summary>
        /// Products db set
        /// </summary>
        public DbSet<ProductModel> Products => Set<ProductModel>();
        public DbSet<OrderModel> Orders => Set<OrderModel>();
        public DbSet<OrderLineModel> OrderLines => Set<OrderLineModel>();

        /// <summary>
        /// Map entities config
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ProductMapping());
        }
    }
}
