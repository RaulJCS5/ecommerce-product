using EcommerceProduct.API.Repository.Interface;
using EcommerceProduct.API.Services.Interface;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;

namespace EcommerceProduct.API.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        // Product operations
        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _productRepository.GetProductsAsync();
        }

        public async Task<(IEnumerable<Product>, PaginationMetadata)> GetProductsAsync(
            string? name, string? searchQuery, int? categoryId,
            decimal? minPrice, decimal? maxPrice, bool? inStock,
            int pageNumber, int pageSize)
        {
            return await _productRepository.GetProductsAsync(
                name, searchQuery, categoryId, minPrice, maxPrice, inStock, pageNumber, pageSize);
        }

        public async Task<Product?> GetProductAsync(int productId, bool includeReviews = false)
        {
            return await _productRepository.GetProductAsync(productId, includeReviews);
        }

        public async Task<bool> ProductExistsAsync(int productId)
        {
            return await _productRepository.ProductExistsAsync(productId);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            // Business logic validation could be added here
            await _productRepository.AddProductAsync(product);
            await _productRepository.SaveChangesAsync();

            // Return the created product with its ID
            var createdProduct = await _productRepository.GetProductAsync(product.Id);
            return createdProduct!;
        }

        public async Task<bool> UpdateProductAsync(int productId, Product updatedProduct)
        {
            var existingProduct = await _productRepository.GetProductAsync(productId);
            if (existingProduct == null)
            {
                return false;
            }

            // Only update properties that are provided (not null/empty/default)
            var hasChanges = false;

            // Update Name (only if not null or empty)
            if (!string.IsNullOrWhiteSpace(updatedProduct.Name) && existingProduct.Name != updatedProduct.Name)
            {
                existingProduct.Name = updatedProduct.Name.Trim();
                hasChanges = true;
            }

            // Update Description (only if not null, allow empty string to clear description)
            if (updatedProduct.Description != null && existingProduct.Description != updatedProduct.Description)
            {
                existingProduct.Description = string.IsNullOrWhiteSpace(updatedProduct.Description) ? null : updatedProduct.Description.Trim();
                hasChanges = true;
            }

            // Update Price (only if greater than 0 and different)
            if (updatedProduct.Price > 0 && existingProduct.Price != updatedProduct.Price)
            {
                existingProduct.Price = updatedProduct.Price;
                hasChanges = true;
            }

            // Update StockQuantity (allow 0 as valid stock, only if different)
            if (existingProduct.StockQuantity != updatedProduct.StockQuantity)
            {
                existingProduct.StockQuantity = updatedProduct.StockQuantity;
                hasChanges = true;
            }

            // Update CategoryId (only if greater than 0 and different)
            if (updatedProduct.CategoryId > 0 && existingProduct.CategoryId != updatedProduct.CategoryId)
            {
                existingProduct.CategoryId = updatedProduct.CategoryId;
                hasChanges = true;
            }

            // Update ImageUrl (only if not null, allow empty string to clear image)
            if (updatedProduct.ImageUrl != null && existingProduct.ImageUrl != updatedProduct.ImageUrl)
            {
                existingProduct.ImageUrl = string.IsNullOrWhiteSpace(updatedProduct.ImageUrl) ? null : updatedProduct.ImageUrl.Trim();
                hasChanges = true;
            }

            // Update SKU (only if not null or empty)
            if (!string.IsNullOrWhiteSpace(updatedProduct.SKU) && existingProduct.SKU != updatedProduct.SKU)
            {
                existingProduct.SKU = updatedProduct.SKU.Trim();
                hasChanges = true;
            }

            // Update IsActive (always update since it's a boolean)
            if (existingProduct.IsActive != updatedProduct.IsActive)
            {
                existingProduct.IsActive = updatedProduct.IsActive;
                hasChanges = true;
            }

            // Only save if there are actual changes
            if (!hasChanges)
            {
                return true; // No changes needed, but operation is successful
            }

            // Set the updated date
            existingProduct.UpdatedDate = DateTime.UtcNow;

            _productRepository.UpdateProduct(existingProduct);
            return await _productRepository.SaveChangesAsync();
        }

        /// <summary>
        /// More efficient update method using DTO that only updates provided fields
        /// </summary>
        public async Task<bool> UpdateProductAsync(int productId, ProductForUpdateDto productDto)
        {
            var existingProduct = await _productRepository.GetProductAsync(productId);
            if (existingProduct == null)
            {
                return false;
            }

            var hasChanges = false;

            // Update Name - always provided in DTO and required
            if (!string.IsNullOrWhiteSpace(productDto.Name) && existingProduct.Name != productDto.Name.Trim())
            {
                existingProduct.Name = productDto.Name.Trim();
                hasChanges = true;
            }

            // Update Description - can be null to clear, empty string is also valid
            if (productDto.Description != null && existingProduct.Description != productDto.Description)
            {
                existingProduct.Description = string.IsNullOrWhiteSpace(productDto.Description) ? null : productDto.Description.Trim();
                hasChanges = true;
            }

            // Update Price - always check since it's required in DTO
            if (existingProduct.Price != productDto.Price)
            {
                existingProduct.Price = productDto.Price;
                hasChanges = true;
            }

            // Update StockQuantity - always check since it's required in DTO
            if (existingProduct.StockQuantity != productDto.StockQuantity)
            {
                existingProduct.StockQuantity = productDto.StockQuantity;
                hasChanges = true;
            }

            // Update CategoryId - always check since it's required in DTO
            if (existingProduct.CategoryId != productDto.CategoryId)
            {
                existingProduct.CategoryId = productDto.CategoryId;
                hasChanges = true;
            }

            // Update SKU - optional field
            if (!string.IsNullOrEmpty(productDto.SKU) && existingProduct.SKU != productDto.SKU.Trim())
            {
                existingProduct.SKU = productDto.SKU.Trim();
                hasChanges = true;
            }

            // Update ImageUrl - optional field, can be null to clear
            if (productDto.ImageUrl != null && existingProduct.ImageUrl != productDto.ImageUrl)
            {
                existingProduct.ImageUrl = string.IsNullOrWhiteSpace(productDto.ImageUrl) ? null : productDto.ImageUrl.Trim();
                hasChanges = true;
            }

            // Update IsActive - always check since it's in DTO
            if (existingProduct.IsActive != productDto.IsActive)
            {
                existingProduct.IsActive = productDto.IsActive;
                hasChanges = true;
            }

            // Only save if there are actual changes
            if (!hasChanges)
            {
                return true; // No changes needed, but operation is successful
            }

            // Set the updated date
            existingProduct.UpdatedDate = DateTime.UtcNow;

            _productRepository.UpdateProduct(existingProduct);
            return await _productRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Ultra-efficient partial update - only updates fields that are explicitly provided (not null)
        /// Perfect for PATCH operations where only specific fields need updating
        /// </summary>
        public async Task<bool> PartialUpdateProductAsync(int productId, ProductPartialUpdateDto productDto)
        {
            var existingProduct = await _productRepository.GetProductAsync(productId);
            if (existingProduct == null)
            {
                return false;
            }

            var hasChanges = false;

            // Only update fields that are explicitly provided (not null)
            
            if (productDto.Name != null && !string.IsNullOrWhiteSpace(productDto.Name) && existingProduct.Name != productDto.Name.Trim())
            {
                existingProduct.Name = productDto.Name.Trim();
                hasChanges = true;
            }

            if (productDto.Description != null && existingProduct.Description != productDto.Description)
            {
                existingProduct.Description = string.IsNullOrWhiteSpace(productDto.Description) ? null : productDto.Description.Trim();
                hasChanges = true;
            }

            if (productDto.Price.HasValue && existingProduct.Price != productDto.Price.Value)
            {
                existingProduct.Price = productDto.Price.Value;
                hasChanges = true;
            }

            if (productDto.StockQuantity.HasValue && existingProduct.StockQuantity != productDto.StockQuantity.Value)
            {
                existingProduct.StockQuantity = productDto.StockQuantity.Value;
                hasChanges = true;
            }

            if (productDto.CategoryId.HasValue && existingProduct.CategoryId != productDto.CategoryId.Value)
            {
                existingProduct.CategoryId = productDto.CategoryId.Value;
                hasChanges = true;
            }

            if (productDto.SKU != null && existingProduct.SKU != productDto.SKU)
            {
                existingProduct.SKU = string.IsNullOrWhiteSpace(productDto.SKU) ? null : productDto.SKU.Trim();
                hasChanges = true;
            }

            if (productDto.ImageUrl != null && existingProduct.ImageUrl != productDto.ImageUrl)
            {
                existingProduct.ImageUrl = string.IsNullOrWhiteSpace(productDto.ImageUrl) ? null : productDto.ImageUrl.Trim();
                hasChanges = true;
            }

            if (productDto.IsActive.HasValue && existingProduct.IsActive != productDto.IsActive.Value)
            {
                existingProduct.IsActive = productDto.IsActive.Value;
                hasChanges = true;
            }

            // Only save if there are actual changes
            if (!hasChanges)
            {
                return true; // No changes needed, but operation is successful
            }

            // Set the updated date
            existingProduct.UpdatedDate = DateTime.UtcNow;

            _productRepository.UpdateProduct(existingProduct);
            return await _productRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _productRepository.GetProductAsync(productId);
            if (product == null)
            {
                return false;
            }

            _productRepository.DeleteProduct(product);
            return await _productRepository.SaveChangesAsync();
        }

        public async Task<bool> UpdateProductStockAsync(int productId, int stockQuantity)
        {
            var product = await _productRepository.GetProductAsync(productId);
            if (product == null)
            {
                return false;
            }

            product.StockQuantity = stockQuantity;
            _productRepository.UpdateProduct(product);
            return await _productRepository.SaveChangesAsync();
        }

        // Category validation
        public async Task<bool> ProductCategoryExistsAsync(int categoryId)
        {
            return await _productRepository.ProductCategoryExistsAsync(categoryId);
        }
    }
}
