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
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.NumberOfReviews, opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews.Count : 0))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => CalculateAverageRating(src.Reviews)));

            CreateMap<Product, ProductWithReviewsDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => CalculateAverageRating(src.Reviews)));

            CreateMap<ProductForCreationDto, Product>();
            CreateMap<ProductForUpdateDto, Product>();
            CreateMap<Product, ProductForUpdateDto>();
        }

        private static double CalculateAverageRating(ICollection<ProductReview>? reviews)
        {
            if (reviews == null || !reviews.Any())
                return 0;

            var approvedReviews = reviews.Where(r => r.IsApproved).ToList();
            return approvedReviews.Any() ? approvedReviews.Average(r => r.Rating) : 0;
        }
    }
}