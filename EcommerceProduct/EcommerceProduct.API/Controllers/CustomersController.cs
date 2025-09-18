using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Authorize(Policy = "MustBeUser")]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public CustomersController(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all customers
        /// </summary>
        /// <returns>List of customers</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            var cityName = User.Claims.FirstOrDefault(c => c.Type == "city")?.Value;
            if (cityName != "Altares")
            {
                return Forbid();
            }
            var customerEntities = await _productRepository.GetCustomersAsync();
            return Ok(_mapper.Map<IEnumerable<CustomerDto>>(customerEntities));
        }

        /// <summary>
        /// Get a specific customer by ID
        /// </summary>
        /// <param name="customerId">The ID of the customer</param>
        /// <param name="includeOrders">Whether to include customer orders</param>
        /// <returns>Customer details</returns>
        [HttpGet("{customerId}", Name = "GetCustomer")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int customerId, bool includeOrders = false)
        {
            var customerEntity = await _productRepository.GetCustomerAsync(customerId, includeOrders);

            if (customerEntity == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CustomerDto>(customerEntity));
        }

        /// <summary>
        /// Get orders for a specific customer
        /// </summary>
        /// <param name="customerId">The ID of the customer</param>
        /// <returns>List of customer orders</returns>
        [HttpGet("{customerId}/orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrders(int customerId)
        {
            if (!await _productRepository.CustomerExistsAsync(customerId))
            {
                return NotFound();
            }

            var ordersForCustomer = await _productRepository.GetOrdersForCustomerAsync(customerId);
            return Ok(_mapper.Map<IEnumerable<OrderDto>>(ordersForCustomer));
        }
    }
}