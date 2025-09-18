using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/categories")]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductCategoriesController(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all product categories
        /// </summary>
        /// <returns>List of product categories</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCategoryDto>>> GetProductCategories()
        {
            var categoryEntities = await _productRepository.GetProductCategoriesAsync();
            return Ok(_mapper.Map<IEnumerable<ProductCategoryDto>>(categoryEntities));
        }

        /// <summary>
        /// Get a specific product category by ID
        /// </summary>
        /// <param name="categoryId">The ID of the category</param>
        /// <param name="includeProducts">Whether to include products in this category</param>
        /// <returns>Category details</returns>
        [HttpGet("{categoryId}", Name = "GetProductCategory")]
        public async Task<ActionResult<ProductCategoryDto>> GetProductCategory(int categoryId, bool includeProducts = false)
        {
            var categoryEntity = await _productRepository.GetProductCategoryAsync(categoryId, includeProducts);

            if (categoryEntity == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ProductCategoryDto>(categoryEntity));
        }

        /// <summary>
        /// Create a new product category (Admin only)
        /// </summary>
        /// <param name="category">Category creation data</param>
        /// <returns>Created category</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductCategoryDto>> CreateProductCategory([FromBody] ProductCategoryForCreationDto category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryEntity = _mapper.Map<Entities.ProductCategory>(category);
            _productRepository.AddProductCategoryAsync(categoryEntity);
            await _productRepository.SaveChangesAsync();

            var createdCategoryToReturn = _mapper.Map<ProductCategoryDto>(categoryEntity);

            return CreatedAtRoute("GetProductCategory",
                new { categoryId = createdCategoryToReturn.Id },
                createdCategoryToReturn);
        }

        /// <summary>
        /// Update an existing product category (Admin only)
        /// </summary>
        /// <param name="categoryId">The ID of the category to update</param>
        /// <param name="category">Category update data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateProductCategory(int categoryId, [FromBody] ProductCategoryForUpdateDto category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryEntity = await _productRepository.GetProductCategoryAsync(categoryId);
            if (categoryEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(category, categoryEntity);
            _productRepository.UpdateProductCategory(categoryEntity);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Delete a product category (Admin only)
        /// </summary>
        /// <param name="categoryId">The ID of the category to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProductCategory(int categoryId)
        {
            var categoryEntity = await _productRepository.GetProductCategoryAsync(categoryId, includeProducts: true);
            if (categoryEntity == null)
            {
                return NotFound();
            }

            // Check if category has products
            if (categoryEntity.Products.Any())
            {
                return BadRequest("Cannot delete category that contains products. Please reassign or delete products first.");
            }

            _productRepository.DeleteProductCategory(categoryEntity);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}