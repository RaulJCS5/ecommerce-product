using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public OrdersController(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        /// <returns>List of orders</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var orderEntities = await _productRepository.GetOrdersAsync();
            return Ok(_mapper.Map<IEnumerable<OrderDto>>(orderEntities));
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

            return Ok(_mapper.Map<OrderDto>(orderEntity));
        }
    }
}