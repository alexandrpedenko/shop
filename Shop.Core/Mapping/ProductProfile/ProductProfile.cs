using AutoMapper;
using Shop.Core.DataEF.Models;
using Shop.Domain.Common;
using Shop.Domain.Products;

namespace Shop.Core.Mapping.ProductProfile
{
    public sealed class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductModel>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title.Value))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description.Value))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Value))
                .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.SKU.Value));

            CreateMap<ProductModel, Product>()
                .ForMember(p => p.Title, opt => opt.MapFrom(src => new Title(src.Title)))
                .ForMember(p => p.Description, opt => opt.MapFrom(src => new Description(src.Description)))
                .ForMember(p => p.Price, opt => opt.MapFrom(src => new Price(src.Price)))
                .ForMember(p => p.SKU, opt => opt.MapFrom(src => new SKU(src.SKU)));
        }
    }
}
