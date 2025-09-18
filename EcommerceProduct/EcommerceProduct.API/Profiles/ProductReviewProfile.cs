using AutoMapper;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;

namespace EcommerceProduct.API.Profiles
{
    public class ProductReviewProfile : Profile
    {
        public ProductReviewProfile()
        {
            CreateMap<ProductReview, ProductReviewDto>();
            CreateMap<ProductReviewForCreationDto, ProductReview>();
        }
    }
}