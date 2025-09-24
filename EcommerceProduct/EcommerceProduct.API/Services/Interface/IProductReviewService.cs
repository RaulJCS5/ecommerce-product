using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;

namespace EcommerceProduct.API.Services.Interface
{
    public interface IProductReviewService
    {
        Task<IEnumerable<ProductReview>> GetProductReviewsAsync(int productId);
        Task<ProductReview?> GetProductReviewAsync(int productId, int reviewId);
        Task<ProductReview> CreateProductReviewAsync(int productId, ProductReviewForCreationDto reviewDto, int userId);
        Task<bool> ApproveProductReviewAsync(int productId, int reviewId, bool approve);
        Task<bool> DeleteProductReviewAsync(int productId, int reviewId, int? userId = null, bool isAdmin = false);
        Task<bool> HasUserReviewedProductAsync(int productId, string userEmail);
    }
}
