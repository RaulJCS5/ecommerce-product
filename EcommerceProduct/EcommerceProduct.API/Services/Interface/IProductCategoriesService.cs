using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;

namespace EcommerceProduct.API.Services.Interface
{
    public interface IProductCategoriesService
    {
        Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync();
        Task<ProductCategory?> GetCategoryByIdAsync(int categoryId);
        Task<ProductCategory?> GetCategoryByIdAsync(int categoryId, bool includeProducts = false);
        Task<ProductCategory> CreateCategoryAsync(ProductCategoryForCreationDto category);
        Task<bool> UpdateCategoryAsync(int categoryId, ProductCategoryForUpdateDto category);
        Task<bool> DeleteCategoryAsync(int categoryId);
        Task<bool> CategoryExistsAsync(int categoryId);
    }
}