using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Services;
using EcommerceProduct.API.Entities;
using System.Security.Claims;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CustomersController(IProductRepository productRepository, IUserRepository userRepository, IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all customers (Admin only)
        /// </summary>
        /// <returns>List of customers</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
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
            // Check if current user can access this customer's orders
            var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            var customer = await _productRepository.GetCustomerAsync(customerId);

            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            // Allow access if user is admin or accessing their own orders
            var userRole = User.FindFirst("role")?.Value;
            if (userRole != "Admin" && customer.UserId != currentUserId)
            {
                return Forbid("You can only access your own orders.");
            }

            var ordersForCustomer = await _productRepository.GetOrdersForCustomerAsync(customerId);
            return Ok(_mapper.Map<IEnumerable<OrderDto>>(ordersForCustomer));
        }

        /// <summary>
        /// Create a customer profile for the current user
        /// </summary>
        /// <param name="customerProfile">Customer profile data</param>
        /// <returns>Created customer profile</returns>
        [HttpPost("profile")]
        public async Task<ActionResult<CustomerDto>> CreateCustomerProfile([FromBody] CreateCustomerProfileDto customerProfile)
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

                // Check if user already has a customer profile
                if (await _userRepository.UserHasCustomerProfileAsync(currentUserId))
                {
                    return Conflict("User already has a customer profile.");
                }

                // Map DTO to Customer entity
                var customer = _mapper.Map<Customer>(customerProfile);

                // Create customer profile
                var createdCustomer = await _userRepository.CreateCustomerProfileAsync(currentUserId, customer);

                // Map back to DTO for response
                var customerDto = _mapper.Map<CustomerDto>(createdCustomer);

                return CreatedAtRoute("GetCustomer", new { customerId = createdCustomer.Id }, customerDto);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating customer profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Update customer profile
        /// </summary>
        /// <param name="customerId">The ID of the customer to update</param>
        /// <param name="customerProfile">Updated customer profile data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{customerId}")]
        public async Task<ActionResult> UpdateCustomerProfile(int customerId, [FromBody] CreateCustomerProfileDto customerProfile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var customerEntity = await _productRepository.GetCustomerAsync(customerId);
                if (customerEntity == null)
                {
                    return NotFound("Customer not found.");
                }

                // Check if current user can update this customer
                var currentUserId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
                var userRole = User.FindFirst("role")?.Value;

                if (userRole != "Admin" && customerEntity.UserId != currentUserId)
                {
                    return Forbid("You can only update your own profile.");
                }

                // Update customer data
                _mapper.Map(customerProfile, customerEntity);
                _productRepository.UpdateCustomer(customerEntity);
                await _productRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating customer profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current user's customer profile
        /// </summary>
        /// <returns>Customer profile</returns>
        [HttpGet("my-profile")]
        public async Task<ActionResult<CustomerDto>> GetMyProfile()
        {
            try
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

                return Ok(_mapper.Map<CustomerDto>(user.Customer));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete customer profile (Admin only)
        /// </summary>
        /// <param name="customerId">The ID of the customer to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{customerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCustomer(int customerId)
        {
            var customerEntity = await _productRepository.GetCustomerAsync(customerId);
            if (customerEntity == null)
            {
                return NotFound("Customer not found.");
            }

            // Note: This will also delete related orders due to cascade delete
            _productRepository.UpdateCustomer(customerEntity);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }
    }
}