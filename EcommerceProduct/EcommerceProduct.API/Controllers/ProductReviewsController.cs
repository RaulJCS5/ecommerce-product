using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/products/{productId}/reviews")]
    public class ProductReviewsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductReviewsController(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all reviews for a specific product
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <returns>List of product reviews</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductReviewDto>>> GetProductReviews(int productId)
        {
            if (!await _productRepository.ProductExistsAsync(productId))
            {
                return NotFound();
            }

            var reviewsForProduct = await _productRepository.GetProductReviewsAsync(productId);
            return Ok(_mapper.Map<IEnumerable<ProductReviewDto>>(reviewsForProduct));
        }

        /// <summary>
        /// Get a specific review for a product
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <param name="reviewId">The ID of the review</param>
        /// <returns>Product review details</returns>
        [HttpGet("{reviewId}", Name = "GetProductReview")]
        public async Task<ActionResult<ProductReviewDto>> GetProductReview(int productId, int reviewId)
        {
            if (!await _productRepository.ProductExistsAsync(productId))
            {
                return NotFound();
            }

            var reviewForProduct = await _productRepository.GetProductReviewAsync(productId, reviewId);

            if (reviewForProduct == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<ProductReviewDto>(reviewForProduct));
        }

        /// <summary>
        /// Create a new review for a product
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <param name="review">Review creation data</param>
        /// <returns>Created review</returns>
        [HttpPost]
        public async Task<ActionResult<ProductReviewDto>> CreateProductReview(
            int productId, ProductReviewForCreationDto review)
        {
            if (!await _productRepository.ProductExistsAsync(productId))
            {
                return NotFound();
            }

            var reviewEntity = _mapper.Map<Entities.ProductReview>(review);
            await _productRepository.AddProductReviewAsync(productId, reviewEntity);
            await _productRepository.SaveChangesAsync();

            var createdReviewToReturn = _mapper.Map<ProductReviewDto>(reviewEntity);

            return CreatedAtRoute("GetProductReview",
                new { productId = productId, reviewId = createdReviewToReturn.Id },
                createdReviewToReturn);
        }

        /// <summary>
        /// Approve or disapprove a product review (Admin only)
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <param name="reviewId">The ID of the review</param>
        /// <param name="approve">Whether to approve or disapprove the review</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("{reviewId}/approve")]
        public async Task<ActionResult> ApproveProductReview(int productId, int reviewId, [FromBody] bool approve)
        {
            if (!await _productRepository.ProductExistsAsync(productId))
            {
                return NotFound();
            }

            var reviewEntity = await _productRepository.GetProductReviewAsync(productId, reviewId);

            if (reviewEntity == null)
            {
                return NotFound();
            }

            reviewEntity.IsApproved = approve;
            _productRepository.UpdateProductReview(reviewEntity);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Delete a product review
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <param name="reviewId">The ID of the review</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{reviewId}")]
        public async Task<ActionResult> DeleteProductReview(int productId, int reviewId)
        {
            if (!await _productRepository.ProductExistsAsync(productId))
            {
                return NotFound();
            }

            var reviewEntity = await _productRepository.GetProductReviewAsync(productId, reviewId);

            if (reviewEntity == null)
            {
                return NotFound();
            }

            _productRepository.DeleteProductReview(reviewEntity);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}