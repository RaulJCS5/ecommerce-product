using AutoMapper;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Repository.Interface;
using EcommerceProduct.API.Services.Interface;

namespace EcommerceProduct.API.Services.Implementation
{
    public class ProductCategoriesService : IProductCategoriesService
    {
        private readonly IProductCategoriesRepository _productCategoriesRepository;
        private readonly IMapper _mapper;

        public ProductCategoriesService(IProductCategoriesRepository productCategoriesRepository, IMapper mapper)
        {
            _productCategoriesRepository = productCategoriesRepository ?? throw new ArgumentNullException(nameof(productCategoriesRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync()
        {
            return await _productCategoriesRepository.GetAllCategoriesAsync();
        }

        public async Task<ProductCategory?> GetCategoryByIdAsync(int categoryId)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Category ID must be greater than 0.", nameof(categoryId));

            return await _productCategoriesRepository.GetCategoryByIdAsync(categoryId);
        }

        public async Task<ProductCategory?> GetCategoryByIdAsync(int categoryId, bool includeProducts = false)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Category ID must be greater than 0.", nameof(categoryId));

            return await _productCategoriesRepository.GetCategoryByIdAsync(categoryId, includeProducts);
        }

        public async Task<ProductCategory> CreateCategoryAsync(ProductCategoryForCreationDto categoryDto)
        {
            if (categoryDto == null)
                throw new ArgumentNullException(nameof(categoryDto));

            if (string.IsNullOrWhiteSpace(categoryDto.Name?.Trim()))
                throw new ArgumentException("Category name cannot be empty or whitespace only.", nameof(categoryDto.Name));

            // Check for duplicate names (business rule)
            var existingCategories = await _productCategoriesRepository.GetAllCategoriesAsync();
            if (existingCategories.Any(c => c.Name.Equals(categoryDto.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"A category with the name '{categoryDto.Name}' already exists.");

            // Map DTO to Entity
            var categoryEntity = _mapper.Map<ProductCategory>(categoryDto);
            categoryEntity.CreatedDate = DateTime.UtcNow;
            categoryEntity.IsActive = true;

            // Create through repository
            _productCategoriesRepository.AddCategoryAsync(categoryEntity);
            await _productCategoriesRepository.SaveChangesAsync();

            return categoryEntity;
        }

        public async Task<bool> UpdateCategoryAsync(int categoryId, ProductCategoryForUpdateDto categoryDto)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Category ID must be greater than 0.", nameof(categoryId));

            if (categoryDto == null)
                throw new ArgumentNullException(nameof(categoryDto));

            // Get existing category
            var existingCategory = await _productCategoriesRepository.GetCategoryByIdAsync(categoryId);
            if (existingCategory == null)
                return false;

            // Update Name only if it's not null or empty
            if (!string.IsNullOrWhiteSpace(categoryDto.Name?.Trim()))
            {
                var trimmedName = categoryDto.Name.Trim();

                // Check for duplicate names (excluding current category)
                var allCategories = await _productCategoriesRepository.GetAllCategoriesAsync();
                if (allCategories.Any(c => c.Id != categoryId && c.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"A category with the name '{trimmedName}' already exists.");

                existingCategory.Name = trimmedName;
            }

            // Update Description only if it's provided (not null)
            if (categoryDto.Description != null)
            {
                existingCategory.Description = string.IsNullOrWhiteSpace(categoryDto.Description) ? null : categoryDto.Description.Trim();
            }

            // Always update IsActive as it's a boolean and has a default value
            existingCategory.IsActive = categoryDto.IsActive;

            // Update through repository
            _productCategoriesRepository.UpdateCategory(existingCategory);
            return await _productCategoriesRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            if (categoryId <= 0)
                throw new ArgumentException("Category ID must be greater than 0.", nameof(categoryId));

            var category = await _productCategoriesRepository.GetCategoryByIdAsync(categoryId, includeProducts: true);
            if (category == null)
                return false;

            // Business rule: Check if category has products
            if (category.Products.Any())
                throw new InvalidOperationException("Cannot delete category that contains products. Please reassign or delete products first.");

            _productCategoriesRepository.DeleteCategory(category);
            return await _productCategoriesRepository.SaveChangesAsync();
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            if (categoryId <= 0)
                return false;

            return await _productCategoriesRepository.CategoryExistsAsync(categoryId);
        }
    }
}