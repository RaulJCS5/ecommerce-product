using AutoMapper;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;

namespace EcommerceProduct.API.Profiles
{
    public class ProductCategoryProfile : Profile
    {
        public ProductCategoryProfile()
        {
            CreateMap<ProductCategory, ProductCategoryDto>()
                .ForMember(dest => dest.NumberOfProducts, opt => opt.MapFrom(src => src.Products.Count));
        }
    }
}