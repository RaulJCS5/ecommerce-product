using AutoMapper;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "MustBeAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AdminController(IProductRepository productRepository, IUserRepository userRepository, IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        /// <returns>Dashboard statistics</returns>
        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardDto>> GetDashboardStats()
        {
            try
            {
                var totalUsers = await _userRepository.GetTotalUsersCountAsync();
                var totalCustomers = await _productRepository.GetTotalCustomersCountAsync();
                var totalProducts = await _productRepository.GetTotalProductsCountAsync();
                var totalOrders = await _productRepository.GetTotalOrdersCountAsync();
                var totalRevenue = await _productRepository.GetTotalRevenueAsync();
                var pendingOrders = await _productRepository.GetPendingOrdersCountAsync();

                var dashboard = new AdminDashboardDto
                {
                    TotalUsers = totalUsers,
                    TotalCustomers = totalCustomers,
                    TotalProducts = totalProducts,
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,
                    PendingOrders = pendingOrders,
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving dashboard data: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all users for admin management
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                return Ok(_mapper.Map<IEnumerable<UserDto>>(users));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving users: {ex.Message}");
            }
        }

        /// <summary>
        /// Update user role (Admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="updateDto">Role update data</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("users/{userId}/role")]
        public async Task<ActionResult> UpdateUserRole(int userId, [FromBody] UserRoleUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (updateDto.Role != "Admin" && updateDto.Role != "User")
                {
                    return BadRequest("Role must be either 'Admin' or 'User'.");
                }

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                user.Role = updateDto.Role;
                _userRepository.UpdateUser(user);
                await _userRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating user role: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all orders for admin management
        /// </summary>
        /// <param name="status">Filter by order status</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of orders</returns>
        [HttpGet("orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders(
            OrderStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var orders = await _productRepository.GetOrdersAsync();

                if (status.HasValue)
                {
                    orders = orders.Where(o => o.Status == status.Value);
                }

                var paginatedOrders = orders
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return Ok(_mapper.Map<IEnumerable<OrderDto>>(paginatedOrders));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving orders: {ex.Message}");
            }
        }

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="status">New order status</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("orders/{orderId}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int orderId, [FromBody] OrderStatus status)
        {
            try
            {
                var order = await _productRepository.GetOrderAsync(orderId);
                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                order.Status = status;
                _productRepository.UpdateOrder(order);
                await _productRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating order status: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all customers for admin management
        /// </summary>
        /// <returns>List of customers</returns>
        //[HttpGet("customers")]
        //public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
        //{
        //    try
        //    {
        //        var customers = await _productRepository.GetCustomersAsync();
        //        return Ok(_mapper.Map<IEnumerable<CustomerDto>>(customers));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred while retrieving customers: {ex.Message}");
        //    }
        //}

        /// <summary>
        /// Get all products for admin management
        /// </summary>
        /// <returns>List of products</returns>
        [HttpGet("products")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            try
            {
                var products = await _productRepository.GetProductsAsync();
                return Ok(_mapper.Map<IEnumerable<ProductDto>>(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving products: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all product reviews pending approval
        /// </summary>
        /// <returns>List of pending reviews</returns>
        [HttpGet("reviews/pending")]
        public async Task<ActionResult<IEnumerable<ProductReviewDto>>> GetPendingReviews()
        {
            try
            {
                var pendingReviews = await _productRepository.GetPendingReviewsAsync();
                return Ok(_mapper.Map<IEnumerable<ProductReviewDto>>(pendingReviews));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving pending reviews: {ex.Message}");
            }
        }

        /// <summary>
        /// Bulk approve or reject reviews
        /// </summary>
        /// <param name="bulkAction">Bulk action data</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("reviews/bulk-approve")]
        public async Task<ActionResult> BulkApproveReviews([FromBody] BulkReviewActionDto bulkAction)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                foreach (var reviewId in bulkAction.ReviewIds)
                {
                    var review = await _productRepository.GetReviewByIdAsync(reviewId);
                    if (review != null)
                    {
                        review.IsApproved = bulkAction.Approve;
                        _productRepository.UpdateProductReview(review);
                    }
                }

                await _productRepository.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while processing bulk review action: {ex.Message}");
            }
        }

        /// <summary>
        /// Get basic analytics - business intelligence
        /// </summary>
        /// <returns>Basic analytics data</returns>
        //[HttpGet("analytics/basic")]
        //public async Task<ActionResult<object>> GetBasicAnalytics()
        //{
        //    try
        //    {
        //        var orders = await _productRepository.GetOrdersAsync();
        //        var products = await _productRepository.GetProductsAsync();
        //        var customers = await _productRepository.GetCustomersAsync();

        //        var analytics = new
        //        {
        //            TotalRevenue = orders.Sum(o => o.TotalAmount),
        //            TotalOrders = orders.Count(),
        //            AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
        //            TotalProducts = products.Count(),
        //            TotalCustomers = customers.Count(),
        //            OrdersByStatus = orders.GroupBy(o => o.Status)
        //                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() }),
        //            RecentOrders = orders.OrderByDescending(o => o.OrderDate)
        //                .Take(5)
        //                .Select(o => new
        //                {
        //                    o.Id,
        //                    o.OrderNumber,
        //                    o.TotalAmount,
        //                    o.Status,
        //                    o.OrderDate
        //                })
        //        };

        //        return Ok(analytics);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred while retrieving analytics: {ex.Message}");
        //    }
        //}
    }
}