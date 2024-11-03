using Microsoft.EntityFrameworkCore;
using Shop.Core.DataEF.EntityMappings;
using Shop.Core.DataEF.Models;

namespace Shop.Core.DataEF
{
    /// <summary>
    /// Db context
    /// </summary>
    public class ShopContext(DbContextOptions<ShopContext> options) : DbContext(options)
    {
        /// <summary>
        /// Products db set
        /// </summary>
        public DbSet<ProductModel> Products => Set<ProductModel>();


        /// <summary>
        /// Map entities config
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductMapping());
        }
    }
}
