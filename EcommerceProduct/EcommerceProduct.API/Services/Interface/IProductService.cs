using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;

namespace EcommerceProduct.API.Services.Interface
{
    public interface IProductService
    {
        // Product operations
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<(IEnumerable<Product>, PaginationMetadata)> GetProductsAsync(
            string? name, string? searchQuery, int? categoryId,
            decimal? minPrice, decimal? maxPrice, bool? inStock,
            int pageNumber, int pageSize);
        Task<Product?> GetProductAsync(int productId, bool includeReviews = false);
        Task<bool> ProductExistsAsync(int productId);
        Task<Product> CreateProductAsync(Product product);
        Task<bool> UpdateProductAsync(int productId, Product product);
        Task<bool> UpdateProductAsync(int productId, ProductForUpdateDto productDto);
        Task<bool> PartialUpdateProductAsync(int productId, ProductPartialUpdateDto productDto);
        Task<bool> DeleteProductAsync(int productId);
        Task<bool> UpdateProductStockAsync(int productId, int stockQuantity);

        // Category validation
        Task<bool> ProductCategoryExistsAsync(int categoryId);
    }
}
