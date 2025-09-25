using AutoMapper;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;

namespace EcommerceProduct.API.Profiles
{
    public class ProductReviewProfile : Profile
    {
        public ProductReviewProfile()
        {
            CreateMap<ProductReview, ProductReviewDto>()
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
                .MaxDepth(2); // Prevent infinite recursion

            CreateMap<ProductReviewForCreationDto, ProductReview>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());
        }
    }
}