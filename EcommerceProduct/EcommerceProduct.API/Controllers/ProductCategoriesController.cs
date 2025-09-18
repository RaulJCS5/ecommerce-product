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
    }
}