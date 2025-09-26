using EcommerceProduct.API.DbContexts;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProduct.API.Repository.Implementation
{
    public class ProductCategoriesRepository : IProductCategoriesRepository
    {
        private readonly ProductContext _context;

        public ProductCategoriesRepository(ProductContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync()
        {
            return await _context.ProductCategories
                .Include(c => c.Products)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ProductCategory?> GetCategoryByIdAsync(int categoryId)
        {
            return await _context.ProductCategories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<ProductCategory?> GetCategoryByIdAsync(int categoryId, bool includeProducts = false)
        {
            if (includeProducts)
            {
                return await _context.ProductCategories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == categoryId);
            }

            return await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public void AddCategoryAsync(ProductCategory category)
        {
            _context.ProductCategories.Add(category);
        }

        public void UpdateCategory(ProductCategory category)
        {
            // No need to do anything here since the entity is tracked
        }

        public void DeleteCategory(ProductCategory category)
        {
            _context.ProductCategories.Remove(category);
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.ProductCategories
                .AnyAsync(c => c.Id == categoryId);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }
    }
}