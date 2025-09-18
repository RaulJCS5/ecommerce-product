using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        const int maxProductsPageSize = 20;

        public ProductsController(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
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
            if (pageSize > maxProductsPageSize)
            {
                pageSize = maxProductsPageSize;
            }

            var (productEntities, paginationMetadata) = await _productRepository.GetProductsAsync(
                name, searchQuery, categoryId, minPrice, maxPrice, inStock, pageNumber, pageSize);

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(_mapper.Map<IEnumerable<ProductDto>>(productEntities));
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
            var product = await _productRepository.GetProductAsync(productId, includeReviews);

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

        /// <summary>
        /// Create a new product (Admin only)
        /// </summary>
        /// <param name="product">Product creation data</param>
        /// <returns>Created product</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductForCreationDto product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate category exists
                if (!await _productRepository.ProductCategoryExistsAsync(product.CategoryId))
                {
                    return BadRequest("Category does not exist.");
                }

                var productEntity = _mapper.Map<Entities.Product>(product);
                _productRepository.AddProductAsync(productEntity);
                await _productRepository.SaveChangesAsync();

                var createdProductToReturn = _mapper.Map<ProductDto>(productEntity);

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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateProduct(int productId, ProductForUpdateDto product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var productEntity = await _productRepository.GetProductAsync(productId);

                if (productEntity == null)
                {
                    return NotFound("Product not found.");
                }

                // Validate category exists
                if (!await _productRepository.ProductCategoryExistsAsync(product.CategoryId))
                {
                    return BadRequest("Category does not exist.");
                }

                _mapper.Map(product, productEntity);
                _productRepository.UpdateProduct(productEntity);
                await _productRepository.SaveChangesAsync();

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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProduct(int productId)
        {
            try
            {
                var productEntity = await _productRepository.GetProductAsync(productId);

                if (productEntity == null)
                {
                    return NotFound("Product not found.");
                }

                _productRepository.DeleteProduct(productEntity);
                await _productRepository.SaveChangesAsync();

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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateProductStock(int productId, [FromBody] ProductStockUpdateDto stockUpdate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var productEntity = await _productRepository.GetProductAsync(productId);

                if (productEntity == null)
                {
                    return NotFound("Product not found.");
                }

                productEntity.StockQuantity = stockUpdate.StockQuantity;
                _productRepository.UpdateProduct(productEntity);
                await _productRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating product stock: {ex.Message}");
            }
        }
    }
}