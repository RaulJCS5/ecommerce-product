using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceProduct.API.Repository.Interface;
using EcommerceProduct.API.Services.Interface;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/categories")]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IProductCategoriesService _productCategoriesService;

        public ProductCategoriesController(IProductCategoriesService productCategoriesService, IMapper mapper)
        {
            _productCategoriesService = productCategoriesService ?? throw new ArgumentNullException(nameof(productCategoriesService));
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
                var categoryEntities = await _productCategoriesService.GetAllCategoriesAsync();
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

                var categoryEntity = await _productCategoriesService.GetCategoryByIdAsync(categoryId, includeProducts);

                if (categoryEntity == null)
                {
                    return NotFound("Product category not found.");
                }

                return Ok(_mapper.Map<ProductCategoryDto>(categoryEntity));
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(argEx.Message);
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

                // Create through service (includes validation and business logic)
                var createdCategory = await _productCategoriesService.CreateCategoryAsync(category);

                // Map to DTO for response
                var createdCategoryToReturn = _mapper.Map<ProductCategoryDto>(createdCategory);

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
                // Handle business logic violations (like duplicate names)
                return Conflict($"Operation failed: {opEx.Message}");
            }
            catch (Exception ex)
            {
                // Log the full exception (in real application, use proper logging)
                // _logger.LogError(ex, "Error creating product category: {@Category}", category);
                return StatusCode(500, $"An unexpected error occurred while creating the product category: {ex.Message}");
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

                var updateResult = await _productCategoriesService.UpdateCategoryAsync(categoryId, category);

                if (!updateResult)
                {
                    return NotFound("Product category not found.");
                }

                return NoContent();
            }
            catch (ArgumentException argEx)
            {
                return BadRequest($"Invalid input: {argEx.Message}");
            }
            catch (InvalidOperationException opEx)
            {
                return Conflict($"Operation failed: {opEx.Message}");
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

                var deleteResult = await _productCategoriesService.DeleteCategoryAsync(categoryId);

                if (!deleteResult)
                {
                    return NotFound("Product category not found.");
                }

                return NoContent();
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(argEx.Message);
            }
            catch (InvalidOperationException opEx)
            {
                return BadRequest(opEx.Message); // Business logic errors like "category has products"
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the product category: {ex.Message}");
            }
        }
    }
}