using Microsoft.EntityFrameworkCore;
using EcommerceProduct.API.DbContexts;
using EcommerceProduct.API.Entities;

namespace EcommerceProduct.API.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductContext _context;

        public ProductRepository(ProductContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Product operations
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Product>, PaginationMetadata)> GetProductsAsync(
            string? name, string? searchQuery, int? categoryId,
            decimal? minPrice, decimal? maxPrice, bool? inStock,
            int pageNumber, int pageSize)
        {
            var collection = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .Where(p => p.IsActive) as IQueryable<Product>;

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                collection = collection.Where(p => p.Name.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                collection = collection.Where(p => p.Name.Contains(searchQuery)
                    || (p.Description != null && p.Description.Contains(searchQuery)));
            }

            if (categoryId.HasValue)
            {
                collection = collection.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue)
            {
                collection = collection.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                collection = collection.Where(p => p.Price <= maxPrice.Value);
            }

            if (inStock.HasValue && inStock.Value)
            {
                collection = collection.Where(p => p.StockQuantity > 0);
            }

            var totalItemCount = await collection.CountAsync();
            var paginationMetadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber);

            var collectionToReturn = await collection
                .OrderBy(p => p.Name)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collectionToReturn, paginationMetadata);
        }

        public async Task<Product?> GetProductAsync(int productId, bool includeReviews = false)
        {
            if (includeReviews)
            {
                return await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Reviews.Where(r => r.IsApproved))
                    .Where(p => p.Id == productId)
                    .FirstOrDefaultAsync();
            }

            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == productId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ProductExistsAsync(int productId)
        {
            return await _context.Products.AnyAsync(p => p.Id == productId);
        }

        public void AddProductAsync(Product product)
        {
            _context.Products.Add(product);
        }

        public void UpdateProduct(Product product)
        {
            product.UpdatedDate = DateTime.UtcNow;
            // EF Core automatically tracks changes
        }

        public void DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
        }

        // Product Category operations
        public async Task<IEnumerable<ProductCategory>> GetProductCategoriesAsync()
        {
            return await _context.ProductCategories
                .Include(c => c.Products.Where(p => p.IsActive))
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ProductCategory?> GetProductCategoryAsync(int categoryId, bool includeProducts = false)
        {
            if (includeProducts)
            {
                return await _context.ProductCategories
                    .Include(c => c.Products.Where(p => p.IsActive))
                    .Where(c => c.Id == categoryId)
                    .FirstOrDefaultAsync();
            }

            return await _context.ProductCategories
                .Where(c => c.Id == categoryId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ProductCategoryExistsAsync(int categoryId)
        {
            return await _context.ProductCategories.AnyAsync(c => c.Id == categoryId);
        }

        // Product Review operations
        public async Task<IEnumerable<ProductReview>> GetProductReviewsAsync(int productId)
        {
            return await _context.ProductReviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<ProductReview?> GetProductReviewAsync(int productId, int reviewId)
        {
            return await _context.ProductReviews
                .Where(r => r.ProductId == productId && r.Id == reviewId)
                .FirstOrDefaultAsync();
        }

        public async Task AddProductReviewAsync(int productId, ProductReview review)
        {
            var product = await GetProductAsync(productId);
            if (product != null)
            {
                product.Reviews.Add(review);
            }
        }

        public void UpdateProductReview(ProductReview review)
        {
            // EF Core automatically tracks changes
        }

        public void DeleteProductReview(ProductReview review)
        {
            _context.ProductReviews.Remove(review);
        }

        // Customer operations
        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .Include(c => c.User)
                .Where(c => c.User.IsActive)
                .OrderBy(c => c.User.LastName)
                .ThenBy(c => c.User.FirstName)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerAsync(int customerId, bool includeOrders = false)
        {
            if (includeOrders)
            {
                return await _context.Customers
                    .Include(c => c.User)
                    .Include(c => c.Orders)
                        .ThenInclude(o => o.OrderItems)
                            .ThenInclude(oi => oi.Product)
                    .Where(c => c.Id == customerId)
                    .FirstOrDefaultAsync();
            }

            return await _context.Customers
                .Include(c => c.User)
                .Where(c => c.Id == customerId)
                .FirstOrDefaultAsync();
        }

        public async Task<Customer?> GetCustomerByUserEmailAsync(string email)
        {
            return await _context.Customers
                .Include(c => c.User)
                .Where(c => c.User.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            return await _context.Customers.AnyAsync(c => c.Id == customerId);
        }

        public void AddCustomerAsync(Customer customer)
        {
            _context.Customers.AddAsync(customer);
        }

        public void UpdateCustomer(Customer customer)
        {
            // EF Core automatically tracks changes
        }

        // Order operations
        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersForCustomerAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderAsync(int orderId, bool includeOrderItems = false)
        {
            if (includeOrderItems)
            {
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.Id == orderId)
                    .FirstOrDefaultAsync();
            }

            return await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.Id == orderId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> OrderExistsAsync(int orderId)
        {
            return await _context.Orders.AnyAsync(o => o.Id == orderId);
        }

        public void AddOrderAsync(Order order)
        {
            _context.Orders.Add(order);
        }

        public void UpdateOrder(Order order)
        {
            // EF Core automatically tracks changes
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }
    }
}