using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using Microsoft.AspNetCore.Authorization;
using EcommerceProduct.API.Entities;
using System.Security.Claims;
using EcommerceProduct.API.Repository.Implementation;
using EcommerceProduct.API.Repository.Interface;
using EcommerceProduct.API.Services.Interface;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public OrdersController(IOrderService orderService, IUserService userService, IMapper mapper)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
        /// Get all orders (Admin only)
        /// </summary>
        /// <returns>List of orders</returns>
        [HttpGet]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var orderEntities = await _orderService.GetOrdersAsync();
            return Ok(_mapper.Map<IEnumerable<OrderDto>>(orderEntities));
        }

        /// <summary>
        /// Get current user's orders
        /// </summary>
        /// <returns>List of user's orders</returns>
        [HttpGet("my-orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var user = await _userService.GetUserWithCustomerAsync(currentUserId);
                if (user?.Customer == null)
                {
                    return NotFound("Customer profile not found. Please create a customer profile first.");
                }

                var orders = await _orderService.GetOrdersForCustomerAsync(user.Customer.Id);
                return Ok(_mapper.Map<IEnumerable<OrderDto>>(orders));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving your orders: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific order by ID
        /// </summary>
        /// <param name="orderId">The ID of the order</param>
        /// <param name="includeOrderItems">Whether to include order items</param>
        /// <returns>Order details</returns>
        [HttpGet("{orderId}", Name = "GetOrder")]
        public async Task<ActionResult<OrderDto>> GetOrder(int orderId, bool includeOrderItems = true)
        {
            try
            {
                var orderEntity = await _orderService.GetOrderAsync(orderId, includeOrderItems);

                if (orderEntity == null)
                {
                    return NotFound();
                }

                // Check if user can access this order
                var currentUserId = GetCurrentUserId();
                var userRole = User.FindFirst("role_name")?.Value;

                if (userRole != "Admin" && orderEntity.Customer?.UserId != currentUserId)
                {
                    return Forbid("You can only access your own orders.");
                }

                return Ok(_mapper.Map<OrderDto>(orderEntity));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the order: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        /// <param name="orderDto">Order creation data</param>
        /// <returns>Created order</returns>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderForCreationDto orderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();

                var user = await _userService.GetUserWithCustomerAsync(currentUserId);
                if (user?.Customer == null)
                {
                    return BadRequest("Customer profile not found. Please create a customer profile first.");
                }

                var order = await _orderService.CreateOrderAsync(orderDto, user.Customer.Id);

                var createdOrderToReturn = _mapper.Map<OrderDto>(order);
                return CreatedAtRoute("GetOrder", new { orderId = order.Id }, createdOrderToReturn);
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
                return StatusCode(500, $"An error occurred while creating the order: {ex.Message}");
            }
        }

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        /// <param name="orderId">The ID of the order to update</param>
        /// <param name="orderUpdateDto">Order update data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{orderId}")]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult> UpdateOrder(int orderId, [FromBody] OrderForUpdateDto orderUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updated = await _orderService.UpdateOrderAsync(orderId, orderUpdateDto);

                if (!updated)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the order: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancel an order (Customer can cancel own orders, Admin can cancel any)
        /// </summary>
        /// <param name="orderId">The ID of the order to cancel</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("{orderId}/cancel")]
        public async Task<ActionResult> CancelOrder(int orderId)
        {
            try
            {
                var orderEntity = await _orderService.GetOrderAsync(orderId);
                if (orderEntity == null)
                {
                    return NotFound();
                }

                // Check if user can cancel this order
                var currentUserId = GetCurrentUserId();
                var userRole = User.FindFirst("role_name")?.Value;

                if (userRole != "Admin" && orderEntity.Customer?.UserId != currentUserId)
                {
                    return Forbid("You can only cancel your own orders.");
                }

                var cancelled = await _orderService.CancelOrderAsync(orderId);

                if (!cancelled)
                {
                    return NotFound();
                }

                return NoContent();
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
                return StatusCode(500, $"An error occurred while canceling the order: {ex.Message}");
            }
        }
    }
}