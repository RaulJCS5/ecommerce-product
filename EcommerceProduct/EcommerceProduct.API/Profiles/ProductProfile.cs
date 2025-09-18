using AutoMapper;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;

namespace EcommerceProduct.API.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category!.Name))
                .ForMember(dest => dest.NumberOfReviews, opt => opt.MapFrom(src => src.Reviews.Count))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                    src.Reviews.Any() ? src.Reviews.Where(r => r.IsApproved).Average(r => r.Rating) : 0));

            CreateMap<Product, ProductWithReviewsDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category!.Name))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                    src.Reviews.Any() ? src.Reviews.Where(r => r.IsApproved).Average(r => r.Rating) : 0));

            CreateMap<ProductForCreationDto, Product>();
            CreateMap<ProductForUpdateDto, Product>();
            CreateMap<Product, ProductForUpdateDto>();
        }
    }
}