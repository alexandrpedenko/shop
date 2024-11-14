using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Shop.Core.DataEF;
using Shop.Core.DataEF.Models;
using Shop.Domain.Products;

namespace Shop.Core.Services.Products
{
    /// <summary>
    /// Product management service
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="mapper"></param>
    public sealed class ProductService(ShopContext dbContext, IMapper mapper)
    {
        private readonly ShopContext _dbContext = dbContext;
        private readonly IMapper _mapper = mapper;

        public async Task<Product> CreateProductAsync(Product product)
        {
            var dbModel = _mapper.Map<ProductModel>(product);

            _dbContext.Products.Add(dbModel);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<Product>(dbModel);
        }

        public async Task<bool> CheckIfExistBySkuAsync(string sku)
        {
            return await _dbContext.Products.AnyAsync(p => p.SKU == sku);
        }
    }
}
