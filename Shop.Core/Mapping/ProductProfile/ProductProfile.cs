using AutoMapper;
using Shop.Core.DataEF.Models;
using Shop.Domain.Products;

namespace Shop.Core.Mapping.ProductProfile
{
    public sealed class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductModel>();
            CreateMap<ProductModel, Product>();
        }
    }
}
