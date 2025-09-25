using EcommerceProduct.API.Repository.Interface;
using EcommerceProduct.API.Services.Interface;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;
using AutoMapper;

namespace EcommerceProduct.API.Services.Implementation
{
    public class ProductReviewService : IProductReviewService
    {
        private readonly IProductReviewRepository _productReviewRepository;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ProductReviewService(
            IProductReviewRepository productReviewRepository,
            IProductService productService,
            IUserService userService,
            IMapper mapper)
        {
            _productReviewRepository = productReviewRepository ?? throw new ArgumentNullException(nameof(productReviewRepository));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<ProductReview>> GetProductReviewsAsync(int productId)
        {
            // Verify product exists
            if (!await _productService.ProductExistsAsync(productId))
            {
                throw new InvalidOperationException("Product not found.");
            }

            return await _productReviewRepository.GetProductReviewsAsync(productId);
        }

        public async Task<ProductReview?> GetProductReviewAsync(int productId, int reviewId)
        {
            // Verify product exists
            if (!await _productService.ProductExistsAsync(productId))
            {
                throw new InvalidOperationException("Product not found.");
            }

            return await _productReviewRepository.GetProductReviewAsync(productId, reviewId);
        }

        public async Task<ProductReview> CreateProductReviewAsync(int productId, ProductReviewForCreationDto reviewDto, int userId)
        {
            // Verify product exists
            if (!await _productService.ProductExistsAsync(productId))
            {
                throw new InvalidOperationException("Product not found.");
            }

            // Get user information
            var user = await _userService.GetUserWithCustomerAsync(userId);
            if (user?.Customer == null)
            {
                throw new InvalidOperationException("Customer profile not found. Please create a customer profile first.");
            }

            // Check if user has already reviewed this product
            if (await HasUserReviewedProductAsync(productId, user.Email))
            {
                throw new InvalidOperationException("You have already reviewed this product.");
            }

            // Create review entity

            var reviewEntity = new ProductReview(
                rating: reviewDto.Rating,
                customerName: $"{user.FirstName} {user.LastName}",
                productId: productId)
            {
                CustomerEmail = user.Email,
                Comment = reviewDto.Comment,
                CreatedDate = DateTime.UtcNow,
                IsApproved = false
            };



            _productReviewRepository.AddProductReviewAsync(productId, reviewEntity);
            await _productReviewRepository.SaveChangesAsync();

            return reviewEntity;
        }

        public async Task<bool> ApproveProductReviewAsync(int productId, int reviewId, bool approve)
        {
            // Verify product exists
            if (!await _productService.ProductExistsAsync(productId))
            {
                return false;
            }

            var reviewEntity = await _productReviewRepository.GetProductReviewAsync(productId, reviewId);
            if (reviewEntity == null)
            {
                return false;
            }

            reviewEntity.IsApproved = approve;
            _productReviewRepository.UpdateProductReview(reviewEntity);
            await _productReviewRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteProductReviewAsync(int productId, int reviewId, int? userId = null, bool isAdmin = false)
        {
            // Verify product exists
            if (!await _productService.ProductExistsAsync(productId))
            {
                return false;
            }

            var reviewEntity = await _productReviewRepository.GetProductReviewAsync(productId, reviewId);
            if (reviewEntity == null)
            {
                return false;
            }

            // Check permission if not admin
            if (!isAdmin && userId.HasValue)
            {
                var user = await _userService.GetUserWithCustomerAsync(userId.Value);
                if (user?.Email != reviewEntity.CustomerEmail)
                {
                    throw new UnauthorizedAccessException("You can only delete your own reviews.");
                }
            }

            _productReviewRepository.DeleteProductReview(reviewEntity);
            await _productReviewRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> HasUserReviewedProductAsync(int productId, string userEmail)
        {
            var existingReviews = await _productReviewRepository.GetProductReviewsAsync(productId);
            return existingReviews.Any(r => r.CustomerEmail == userEmail);
        }
    }
}
