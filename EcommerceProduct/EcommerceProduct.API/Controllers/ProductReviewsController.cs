using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceProduct.API.Services.Interface;
using System.Security.Claims;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/reviews")]
    public class ProductReviewsController : ControllerBase
    {
        private readonly IProductReviewService _productReviewService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ProductReviewsController(IProductReviewService productReviewService,
            IUserService userService,
            IMapper mapper)
        {
            _productReviewService = productReviewService ??
                throw new ArgumentNullException(nameof(productReviewService));
            _userService = userService ??
                throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Helper method to safely get the current authenticated user ID
        /// </summary>
        /// <returns>User ID from the JWT token claims</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user ID in token.");
            }

            return userId;
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
            try
            {
                var reviewsForProduct = await _productReviewService.GetProductReviewsAsync(productId);
                return Ok(_mapper.Map<IEnumerable<ProductReviewDto>>(reviewsForProduct));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving reviews: {ex.Message}");
            }
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
            try
            {
                var reviewForProduct = await _productReviewService.GetProductReviewAsync(productId, reviewId);

                if (reviewForProduct == null)
                {
                    return NotFound("Review not found.");
                }

                return Ok(_mapper.Map<ProductReviewDto>(reviewForProduct));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the review: {ex.Message}");
            }
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

                var currentUserId = GetCurrentUserId();

                var reviewEntity = await _productReviewService.CreateProductReviewAsync(productId, review, currentUserId);

                var createdReviewToReturn = _mapper.Map<ProductReviewDto>(reviewEntity);

                return CreatedAtRoute("GetProductReview",
                    new { productId = productId, reviewId = createdReviewToReturn.Id },
                    createdReviewToReturn);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
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
            try
            {
                var updated = await _productReviewService.ApproveProductReviewAsync(productId, reviewId, approve);

                if (!updated)
                {
                    return NotFound("Product or review not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the review: {ex.Message}");
            }
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
                var currentUserId = GetCurrentUserId();
                var userRole = User.FindFirst("role_name")?.Value;
                var isAdmin = userRole == "Admin";

                var deleted = await _productReviewService.DeleteProductReviewAsync(productId, reviewId, currentUserId, isAdmin);

                if (!deleted)
                {
                    return NotFound("Product or review not found.");
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the review: {ex.Message}");
            }
        }
    }
}