using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;
using Microsoft.AspNetCore.Authorization;
using EcommerceProduct.API.Entities;
using System.Security.Claims;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public OrdersController(IProductRepository productRepository, IUserRepository userRepository, IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all orders (Admin only)
        /// </summary>
        /// <returns>List of orders</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var orderEntities = await _productRepository.GetOrdersAsync();
            return Ok(_mapper.Map<IEnumerable<OrderDto>>(orderEntities));
        }

        /// <summary>
        /// Get current user's orders
        /// </summary>
        /// <returns>List of user's orders</returns>
        [HttpGet("my-orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
        {
            var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            if (currentUserId == 0)
            {
                return Unauthorized("User ID not found in token.");
            }

            var user = await _userRepository.GetUserWithCustomerAsync(currentUserId);
            if (user?.Customer == null)
            {
                return NotFound("Customer profile not found. Please create a customer profile first.");
            }

            var orders = await _productRepository.GetOrdersForCustomerAsync(user.Customer.Id);
            return Ok(_mapper.Map<IEnumerable<OrderDto>>(orders));
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
            var orderEntity = await _productRepository.GetOrderAsync(orderId, includeOrderItems);

            if (orderEntity == null)
            {
                return NotFound();
            }

            // Check if user can access this order
            var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            if (userRole != "Admin" && orderEntity.Customer?.UserId != currentUserId)
            {
                return Forbid("You can only access your own orders.");
            }

            return Ok(_mapper.Map<OrderDto>(orderEntity));
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

                // Validate products and calculate total
                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();

                foreach (var itemDto in orderDto.OrderItems)
                {
                    var product = await _productRepository.GetProductAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        return BadRequest($"Product with ID {itemDto.ProductId} not found.");
                    }

                    if (product.StockQuantity < itemDto.Quantity)
                    {
                        return BadRequest($"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {itemDto.Quantity}");
                    }

                    var orderItem = new OrderItem(itemDto.Quantity, product.Price, 0, itemDto.ProductId);
                    orderItems.Add(orderItem);
                    totalAmount += itemDto.Quantity * product.Price;
                }

                // Generate order number
                var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

                // Create order
                var order = new Order(orderNumber, user.Customer.Id)
                {
                    Notes = orderDto.Notes,
                    ShippingAddress = orderDto.ShippingAddress,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending
                };

                _productRepository.AddOrderAsync(order);
                await _productRepository.SaveChangesAsync();

                // Add order items
                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                }

                // Note: This might need a separate method in the repository
                // For now, we'll assume OrderItems are handled through the Order entity

                var createdOrderToReturn = _mapper.Map<OrderDto>(order);
                return CreatedAtRoute("GetOrder", new { orderId = order.Id }, createdOrderToReturn);
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateOrder(int orderId, [FromBody] OrderForUpdateDto orderUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderEntity = await _productRepository.GetOrderAsync(orderId);
            if (orderEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(orderUpdateDto, orderEntity);
            _productRepository.UpdateOrder(orderEntity);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Cancel an order (Customer can cancel own orders, Admin can cancel any)
        /// </summary>
        /// <param name="orderId">The ID of the order to cancel</param>
        /// <returns>No content if successful</returns>
        [HttpPatch("{orderId}/cancel")]
        public async Task<ActionResult> CancelOrder(int orderId)
        {
            var orderEntity = await _productRepository.GetOrderAsync(orderId);
            if (orderEntity == null)
            {
                return NotFound();
            }

            // Check if user can cancel this order
            var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var userRole = User.FindFirst("role")?.Value;

            if (userRole != "Admin" && orderEntity.Customer?.UserId != currentUserId)
            {
                return Forbid("You can only cancel your own orders.");
            }

            // Check if order can be cancelled
            if (orderEntity.Status == OrderStatus.Shipped || orderEntity.Status == OrderStatus.Delivered)
            {
                return BadRequest("Cannot cancel order that has been shipped or delivered.");
            }

            if (orderEntity.Status == OrderStatus.Cancelled)
            {
                return BadRequest("Order is already cancelled.");
            }

            orderEntity.Status = OrderStatus.Cancelled;
            _productRepository.UpdateOrder(orderEntity);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}