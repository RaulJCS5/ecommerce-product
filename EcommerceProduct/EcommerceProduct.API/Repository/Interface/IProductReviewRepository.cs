using EcommerceProduct.API.Entities;

namespace EcommerceProduct.API.Repository.Interface
{
    public interface IProductReviewRepository
    {
        Task<IEnumerable<ProductReview>> GetProductReviewsAsync(int productId);
        Task<ProductReview?> GetProductReviewAsync(int productId, int reviewId);
        Task<bool> ProductReviewExistsAsync(int productId, int reviewId);
        void AddProductReviewAsync(int productId, ProductReview review);
        void UpdateProductReview(ProductReview review);
        void DeleteProductReview(ProductReview review);
        Task<bool> SaveChangesAsync();
    }
}
