using EcommerceProduct.API.Entities;

namespace EcommerceProduct.API.Repository.Interface
{
    public interface IProductCategoriesRepository
    {
        Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync();
        Task<ProductCategory?> GetCategoryByIdAsync(int categoryId);
        Task<ProductCategory?> GetCategoryByIdAsync(int categoryId, bool includeProducts = false);
        void AddCategoryAsync(ProductCategory category);
        void UpdateCategory(ProductCategory category);
        void DeleteCategory(ProductCategory category);
        Task<bool> CategoryExistsAsync(int categoryId);
        Task<bool> SaveChangesAsync();
    }
}