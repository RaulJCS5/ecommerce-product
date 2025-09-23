using AutoMapper;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        const int maxProductsPageSize = 20;

        public ProductsController(IProductService productService, IMapper mapper)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all products with optional filtering and pagination
        /// </summary>
        /// <param name="name">Filter by product name</param>
        /// <param name="searchQuery">Search in name and description</param>
        /// <param name="categoryId">Filter by category ID</param>
        /// <param name="minPrice">Minimum price filter</param>
        /// <param name="maxPrice">Maximum price filter</param>
        /// <param name="inStock">Filter for products in stock</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>List of products</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
            [FromQuery] string? name,
            [FromQuery] string? searchQuery,
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] bool? inStock,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageSize > maxProductsPageSize)
                {
                    pageSize = maxProductsPageSize;
                }

                // Validate page number
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }

                // Validate price range
                if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                {
                    return BadRequest("Minimum price cannot be greater than maximum price.");
                }

                var (productEntities, paginationMetadata) = await _productService.GetProductsAsync(
                    name, searchQuery, categoryId, minPrice, maxPrice, inStock, pageNumber, pageSize);

                Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

                return Ok(_mapper.Map<IEnumerable<ProductDto>>(productEntities));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving products: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific product by ID
        /// </summary>
        /// <param name="productId">The ID of the product to retrieve</param>
        /// <param name="includeReviews">Whether to include product reviews</param>
        /// <returns>Product details</returns>
        [HttpGet("{productId}", Name = "GetProduct")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProduct(int productId, bool includeReviews = false)
        {
            try
            {
                var product = await _productService.GetProductAsync(productId, includeReviews);

                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                if (includeReviews)
                {
                    return Ok(_mapper.Map<ProductWithReviewsDto>(product));
                }

                return Ok(_mapper.Map<ProductDto>(product));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the product: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new product (Admin only)
        /// </summary>
        /// <param name="product">Product creation data</param>
        /// <returns>Created product</returns>
        [HttpPost]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductForCreationDto product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate category exists
                if (!await _productService.ProductCategoryExistsAsync(product.CategoryId))
                {
                    return BadRequest("Category does not exist.");
                }

                var productEntity = _mapper.Map<Product>(product);
                var createdProduct = await _productService.CreateProductAsync(productEntity);

                var createdProductToReturn = _mapper.Map<ProductDto>(createdProduct);

                return CreatedAtRoute("GetProduct",
                    new { productId = createdProductToReturn.Id },
                    createdProductToReturn);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the product: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing product (Admin only)
        /// </summary>
        /// <param name="productId">The ID of the product to update</param>
        /// <param name="product">Product update data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{productId}")]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult> UpdateProduct(int productId, ProductForUpdateDto product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate category exists
                if (!await _productService.ProductCategoryExistsAsync(product.CategoryId))
                {
                    return BadRequest("Category does not exist.");
                }

                // Use the more efficient DTO-based update method
                var updated = await _productService.UpdateProductAsync(productId, product);

                if (!updated)
                {
                    return NotFound("Product not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the product: {ex.Message}");
            }
        }

        /// <summary>
        /// Partially update a product (Admin only) - only provided fields are updated
        /// </summary>
        /// <param name="productId">The ID of the product to update</param>
        /// <param name="product">Product partial update data</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("{productId}")]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult> PartialUpdateProduct(int productId, ProductPartialUpdateDto product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate category exists if it's being updated
                if (product.CategoryId.HasValue && !await _productService.ProductCategoryExistsAsync(product.CategoryId.Value))
                {
                    return BadRequest("Category does not exist.");
                }

                // Use the ultra-efficient partial update method
                var updated = await _productService.PartialUpdateProductAsync(productId, product);

                if (!updated)
                {
                    return NotFound("Product not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the product: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a product (Admin only)
        /// </summary>
        /// <param name="productId">The ID of the product to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{productId}")]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult> DeleteProduct(int productId)
        {
            try
            {
                var deleted = await _productService.DeleteProductAsync(productId);

                if (!deleted)
                {
                    return NotFound("Product not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the product: {ex.Message}");
            }
        }

        /// <summary>
        /// Update product stock quantity (Admin only)
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <param name="stockUpdate">Stock update data</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("{productId}/stock")]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult> UpdateProductStock(int productId, [FromBody] ProductStockUpdateDto stockUpdate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updated = await _productService.UpdateProductStockAsync(productId, stockUpdate.StockQuantity);

                if (!updated)
                {
                    return NotFound("Product not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating product stock: {ex.Message}");
            }
        }
    }
}