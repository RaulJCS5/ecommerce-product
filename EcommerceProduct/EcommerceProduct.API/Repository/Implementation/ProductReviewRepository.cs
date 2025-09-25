using EcommerceProduct.API.DbContexts;
using EcommerceProduct.API.Repository.Interface;
using EcommerceProduct.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProduct.API.Repository.Implementation
{
    public class ProductReviewRepository : IProductReviewRepository
    {
        private readonly ProductContext _context;

        public ProductReviewRepository(ProductContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<ProductReview>> GetProductReviewsAsync(int productId)
        {
            return await _context.ProductReviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.Product!)
                    .ThenInclude(p => p.Category!)
                .ToListAsync();
        }

        public async Task<ProductReview?> GetProductReviewAsync(int productId, int reviewId)
        {
            return await _context.ProductReviews
                .Where(r => r.ProductId == productId && r.Id == reviewId)
                .Include(r => r.Product!)
                    .ThenInclude(p => p.Category!)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ProductReviewExistsAsync(int productId, int reviewId)
        {
            return await _context.ProductReviews
                .AnyAsync(r => r.ProductId == productId && r.Id == reviewId);
        }

        public void AddProductReviewAsync(int productId, ProductReview review)
        {
            review.ProductId = productId;
            _context.ProductReviews.Add(review);
        }

        public void UpdateProductReview(ProductReview review)
        {
            // No need to do anything here since the entity is tracked
        }

        public void DeleteProductReview(ProductReview review)
        {
            _context.ProductReviews.Remove(review);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }
    }
}
