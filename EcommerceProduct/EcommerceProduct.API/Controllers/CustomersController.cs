using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceProduct.API.Models;
using EcommerceProduct.API.Entities;
using System.Security.Claims;
using EcommerceProduct.API.Services.Interface;

namespace EcommerceProduct.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public CustomersController(ICustomerService customerService, IMapper mapper)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all customers (Admin only)
        /// </summary>
        /// <returns>List of customers</returns>
        [HttpGet]
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            var customerEntities = await _customerService.GetCustomersAsync();
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
            var customerEntity = await _customerService.GetCustomerAsync(customerId, includeOrders);

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
        //[HttpGet("{customerId}/orders")]
        //public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrders(int customerId)
        //{
        //    try
        //    {
        //        var ordersForCustomer = await _customerService.GetCustomerOrdersAsync(customerId);
        //        return Ok(_mapper.Map<IEnumerable<OrderDto>>(ordersForCustomer));
        //    }
        //    catch (ArgumentException)
        //    {
        //        return NotFound("Customer not found.");
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Forbid(ex.Message);
        //    }
        //}

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

                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (currentUserId == 0)
                {
                    return Unauthorized("User ID not found in token.");
                }

                // Check if user already has a customer profile
                if (await _customerService.UserHasCustomerProfileAsync(currentUserId))
                {
                    return Conflict("User already has a customer profile.");
                }

                // Map DTO to Customer entity
                var customer = _mapper.Map<Customer>(customerProfile);

                // Create customer profile
                var createdCustomer = await _customerService.CreateCustomerProfileAsync(currentUserId, customer);

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
        /// Update current user customer profile
        /// </summary>
        /// <param name="customerProfile">Updated customer profile data</param>
        /// <returns>No content if successful</returns>
        [HttpPut]
        public async Task<ActionResult> UpdateCustomerProfile([FromBody] CreateCustomerProfileDto customerProfile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Map DTO to Customer entity for updating
                var customerUpdate = _mapper.Map<Customer>(customerProfile);

                // Update customer profile using the service
                var result = await _customerService.UpdateCustomerByUserProfileAsync(currentUserId, customerUpdate);

                if (!result)
                {
                    return NotFound("Customer not found.");
                }

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
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (currentUserId == 0)
                {
                    return Unauthorized("User ID not found in token.");
                }

                var customer = await _customerService.GetCustomerByUserIdAsync(currentUserId);
                if (customer == null)
                {
                    return NotFound("Customer profile not found. Please create a customer profile first.");
                }

                return Ok(_mapper.Map<CustomerDto>(customer));
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
        [Authorize(Policy = "MustBeAdmin")]
        public async Task<ActionResult> DeleteCustomer(int customerId)
        {
            var result = await _customerService.DeleteCustomerAsync(customerId);
            if (!result)
            {
                return NotFound("Customer not found.");
            }

            return NoContent();
        }
    }
}