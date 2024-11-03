using AutoMapper;
using Shop.Core.DataEF.Models;
using Shop.Domain.Product;

namespace Shop.Core.Mapping.ProductProfile
{
    public class ProductProfile : Profile
    {

        public ProductProfile()
        {
            CreateMap<Product, ProductModel>();
            CreateMap<ProductModel, Product>();
        }
    }
}
