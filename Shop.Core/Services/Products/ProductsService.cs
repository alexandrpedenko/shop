using AutoMapper;
using Shop.Core.DataEF;
using Shop.Core.DataEF.Models;
using Shop.Domain.Product;

namespace Shop.Core.Services.Products
{
    /// <summary>
    /// Product management service
    /// </summary>
    /// <param name="productRepository"></param>
    /// <param name="mapper"></param>
    public class ProductService(ShopContext context, IMapper mapper)
    {
        private readonly ShopContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<Product> CreateProductAsync(Product product)
        {
            var dbModel = _mapper.Map<ProductModel>(product);

            _context.Products.Add(dbModel);
            await _context.SaveChangesAsync();

            return _mapper.Map<Product>(dbModel);
        }
    }
}
