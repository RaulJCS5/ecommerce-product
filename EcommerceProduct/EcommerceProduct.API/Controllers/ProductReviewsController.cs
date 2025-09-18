using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/reviews")]
    public class ProductReviewsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ProductReviewsController(IProductRepository productRepository, IUserRepository userRepository, IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all reviews for a specific product
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <returns>List of product reviews</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductReviewDto>>> GetProductReviews(int productId)
        {
            if (!await _productRepository.ProductExistsAsync(productId))
            {
                return NotFound("Product not found.");
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
        [AllowAnonymous]
        public async Task<ActionResult<ProductReviewDto>> GetProductReview(int productId, int reviewId)
        {
            if (!await _productRepository.ProductExistsAsync(productId))
            {
                return NotFound("Product not found.");
            }

            var reviewForProduct = await _productRepository.GetProductReviewAsync(productId, reviewId);

            if (reviewForProduct == null)
            {
                return NotFound("Review not found.");
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
        [Authorize]
        public async Task<ActionResult<ProductReviewDto>> CreateProductReview(
            int productId, ProductReviewForCreationDto review)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!await _productRepository.ProductExistsAsync(productId))
                {
                    return NotFound("Product not found.");
                }

                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                if (currentUserId == 0)
                {
                    return Unauthorized("User ID not found in token.");
                }

                var user = await _userRepository.GetUserWithCustomerAsync(currentUserId);
                if (user?.Customer == null)
                {
                    return BadRequest("Customer profile not found. Please create a customer profile first.");
                }

                // Check if user has already reviewed this product
                var existingReviews = await _productRepository.GetProductReviewsAsync(productId);
                var customerEmail = user.Email;
                if (existingReviews.Any(r => r.CustomerEmail == customerEmail))
                {
                    return BadRequest("You have already reviewed this product.");
                }

                var reviewEntity = _mapper.Map<Entities.ProductReview>(review);
                reviewEntity.CustomerName = $"{user.FirstName} {user.LastName}";
                reviewEntity.CustomerEmail = user.Email;
                reviewEntity.CreatedDate = DateTime.UtcNow;

                await _productRepository.AddProductReviewAsync(productId, reviewEntity);
                await _productRepository.SaveChangesAsync();

                var createdReviewToReturn = _mapper.Map<ProductReviewDto>(reviewEntity);

                return CreatedAtRoute("GetProductReview",
                    new { productId = productId, reviewId = createdReviewToReturn.Id },
                    createdReviewToReturn);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the review: {ex.Message}");
            }
        }

        /// <summary>
        /// Approve or disapprove a product review (Admin only)
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <param name="reviewId">The ID of the review</param>
        /// <param name="approve">Whether to approve or disapprove the review</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("{reviewId}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ApproveProductReview(int productId, int reviewId, [FromBody] bool approve)
        {
            if (!await _productRepository.ProductExistsAsync(productId))
            {
                return NotFound("Product not found.");
            }

            var reviewEntity = await _productRepository.GetProductReviewAsync(productId, reviewId);

            if (reviewEntity == null)
            {
                return NotFound("Review not found.");
            }

            reviewEntity.IsApproved = approve;
            _productRepository.UpdateProductReview(reviewEntity);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Delete a product review (Users can delete own reviews, Admins can delete any)
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <param name="reviewId">The ID of the review</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{reviewId}")]
        [Authorize]
        public async Task<ActionResult> DeleteProductReview(int productId, int reviewId)
        {
            try
            {
                if (!await _productRepository.ProductExistsAsync(productId))
                {
                    return NotFound("Product not found.");
                }

                var reviewEntity = await _productRepository.GetProductReviewAsync(productId, reviewId);

                if (reviewEntity == null)
                {
                    return NotFound("Review not found.");
                }

                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var userRole = User.FindFirst("role")?.Value;

                // Check if user can delete this review
                if (userRole != "Admin")
                {
                    var user = await _userRepository.GetUserWithCustomerAsync(currentUserId);
                    if (user?.Email != reviewEntity.CustomerEmail)
                    {
                        return Forbid("You can only delete your own reviews.");
                    }
                }

                _productRepository.DeleteProductReview(reviewEntity);
                await _productRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the review: {ex.Message}");
            }
        }
    }
}