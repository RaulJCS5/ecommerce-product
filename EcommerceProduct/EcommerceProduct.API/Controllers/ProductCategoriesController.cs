using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceProduct.API.Repository.Interface;

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
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductCategoryDto>>> GetProductCategories()
        {
            try
            {
                var categoryEntities = await _productRepository.GetProductCategoriesAsync();
                return Ok(_mapper.Map<IEnumerable<ProductCategoryDto>>(categoryEntities));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving product categories: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific product category by ID
        /// </summary>
        /// <param name="categoryId">The ID of the category</param>
        /// <param name="includeProducts">Whether to include products in this category</param>
        /// <returns>Category details</returns>
        [HttpGet("{categoryId}", Name = "GetProductCategory")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductCategoryDto>> GetProductCategory(int categoryId, bool includeProducts = false)
        {
            try
            {
                if (categoryId <= 0)
                {
                    return BadRequest("Category ID must be greater than 0.");
                }

                var categoryEntity = await _productRepository.GetProductCategoryAsync(categoryId, includeProducts);

                if (categoryEntity == null)
                {
                    return NotFound("Product category not found.");
                }

                return Ok(_mapper.Map<ProductCategoryDto>(categoryEntity));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the product category: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new product category (Admin only)
        /// </summary>
        /// <param name="category">Category creation data</param>
        /// <returns>Created category with HTTP 201 status</returns>
        [HttpPost]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult<ProductCategoryDto>> CreateProductCategory([FromBody] ProductCategoryForCreationDto category)
        {
            try
            {
                // Validate input data
                if (category == null)
                {
                    return BadRequest("Category data is required.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Additional business validation
                if (string.IsNullOrWhiteSpace(category.Name?.Trim()))
                {
                    return BadRequest("Category name cannot be empty or whitespace only.");
                }

                // Create and configure the entity
                var categoryEntity = _mapper.Map<Entities.ProductCategory>(category);

                // Add to repository (includes duplicate checking)
                var addResult = await _productRepository.AddProductCategoryAsync(categoryEntity);
                
                if (!addResult)
                {
                    return BadRequest($"A category with the name '{categoryEntity.Name}' already exists.");
                }

                var saveResult = await _productRepository.SaveChangesAsync();

                if (!saveResult)
                {
                    return StatusCode(500, "Failed to save the category to the database.");
                }

                // Map to DTO for response
                var createdCategoryToReturn = _mapper.Map<ProductCategoryDto>(categoryEntity);

                // Return created response with location header
                return CreatedAtRoute("GetProductCategory",
                    new { categoryId = createdCategoryToReturn.Id },
                    createdCategoryToReturn);
            }
            catch (ArgumentException argEx)
            {
                // Handle known validation issues
                return BadRequest($"Invalid input: {argEx.Message}");
            }
            catch (InvalidOperationException opEx)
            {
                // Handle business logic violations
                return Conflict($"Operation failed: {opEx.Message}");
            }
            catch (Exception)
            {
                // Log the full exception (in real application, use proper logging)
                // _logger.LogError(ex, "Error creating product category: {@Category}", category);
                
                return StatusCode(500, "An unexpected error occurred while creating the product category. Please try again later.");
            }
        }

        /// <summary>
        /// Update an existing product category (Admin only)
        /// </summary>
        /// <param name="categoryId">The ID of the category to update</param>
        /// <param name="category">Category update data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{categoryId}")]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult> UpdateProductCategory(int categoryId, [FromBody] ProductCategoryForUpdateDto category)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (categoryId <= 0)
                {
                    return BadRequest("Category ID must be greater than 0.");
                }

                var categoryEntity = await _productRepository.GetProductCategoryAsync(categoryId);
                if (categoryEntity == null)
                {
                    return NotFound("Product category not found.");
                }

                _mapper.Map(category, categoryEntity);
                _productRepository.UpdateProductCategory(categoryEntity);
                await _productRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the product category: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a product category (Admin only)
        /// </summary>
        /// <param name="categoryId">The ID of the category to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{categoryId}")]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult> DeleteProductCategory(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                {
                    return BadRequest("Category ID must be greater than 0.");
                }

                var categoryEntity = await _productRepository.GetProductCategoryAsync(categoryId, includeProducts: true);
                if (categoryEntity == null)
                {
                    return NotFound("Product category not found.");
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
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the product category: {ex.Message}");
            }
        }
    }
}