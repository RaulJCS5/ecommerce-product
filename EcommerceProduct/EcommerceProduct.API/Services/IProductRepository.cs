using EcommerceProduct.API.Entities;

namespace EcommerceProduct.API.Services
{
    public interface IProductRepository
    {
        // Product operations
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<(IEnumerable<Product>, PaginationMetadata)> GetProductsAsync(
            string? name, string? searchQuery, int? categoryId,
            decimal? minPrice, decimal? maxPrice, bool? inStock,
            int pageNumber, int pageSize);
        Task<Product?> GetProductAsync(int productId, bool includeReviews = false);
        Task<bool> ProductExistsAsync(int productId);
        void AddProductAsync(Product product);
        void UpdateProduct(Product product);
        void DeleteProduct(Product product);

        // Product Category operations
        Task<IEnumerable<ProductCategory>> GetProductCategoriesAsync();
        Task<ProductCategory?> GetProductCategoryAsync(int categoryId, bool includeProducts = false);
        Task<bool> ProductCategoryExistsAsync(int categoryId);
        void AddProductCategoryAsync(ProductCategory category);
        void UpdateProductCategory(ProductCategory category);
        void DeleteProductCategory(ProductCategory category);

        // Product Review operations
        Task<IEnumerable<ProductReview>> GetProductReviewsAsync(int productId);
        Task<ProductReview?> GetProductReviewAsync(int productId, int reviewId);
        Task AddProductReviewAsync(int productId, ProductReview review);
        void UpdateProductReview(ProductReview review);
        void DeleteProductReview(ProductReview review);

        // Customer operations
        Task<IEnumerable<Customer>> GetCustomersAsync();
        Task<Customer?> GetCustomerAsync(int customerId, bool includeOrders = false);
        Task<Customer?> GetCustomerByUserEmailAsync(string email);
        Task<bool> CustomerExistsAsync(int customerId);
        void AddCustomerAsync(Customer customer);
        void UpdateCustomer(Customer customer);

        // Order-related methods
        Task<IEnumerable<Order>> GetOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersForCustomerAsync(int customerId);
        Task<Order?> GetOrderAsync(int orderId, bool includeOrderItems = false);
        Task<bool> OrderExistsAsync(int orderId);
        void AddOrderAsync(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(Order order);

        // Analytics methods
        Task<int> GetTotalCustomersCountAsync();
        Task<int> GetTotalProductsCountAsync();
        Task<int> GetTotalOrdersCountAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<int> GetPendingOrdersCountAsync();
        Task<IEnumerable<ProductReview>> GetPendingReviewsAsync();
        Task<ProductReview?> GetReviewByIdAsync(int reviewId);

        // Save changes
        Task<bool> SaveChangesAsync();
    }
}