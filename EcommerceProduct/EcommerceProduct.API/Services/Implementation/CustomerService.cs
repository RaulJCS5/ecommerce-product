using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Repository.Implementation;
using EcommerceProduct.API.Repository.Interface;
using EcommerceProduct.API.Services.Interface;

namespace EcommerceProduct.API.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;

        public CustomerService(ICustomerRepository customerRepository, IUserRepository userRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            return await _customerRepository.GetCustomersAsync();
        }

        public async Task<Customer?> GetCustomerAsync(int customerId, bool includeOrders = false)
        {
            return await _customerRepository.GetCustomerAsync(customerId, includeOrders);
        }

        public async Task<Customer?> GetCustomerByUserIdAsync(int userId)
        {
            return await _customerRepository.GetCustomerByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(int customerId)
        {
            // Check if customer exists
            var customer = await _customerRepository.GetCustomerAsync(customerId);
            if (customer == null)
            {
                throw new ArgumentException("Customer not found.", nameof(customerId));
            }

            return await _customerRepository.GetOrdersForCustomerAsync(customerId);
        }

        public async Task<Customer> CreateCustomerProfileAsync(int userId, Customer customer)
        {
            // Check if user already has a customer profile
            if (await _customerRepository.UserHasCustomerProfileAsync(userId))
            {
                throw new InvalidOperationException("User already has a customer profile.");
            }

            // Set the user ID
            customer.UserId = userId;

            // Add customer
            await _customerRepository.AddCustomerAsync(customer);
            await _customerRepository.SaveChangesAsync();

            // Reload the customer with User data included
            var createdCustomerWithUser = await _customerRepository.GetCustomerByUserIdAsync(userId);

            return createdCustomerWithUser ?? customer; // Return the fully loaded customer
        }

        public async Task<bool> UpdateCustomerByUserProfileAsync(int userId, Customer customer)
        {
            // Get existing User by userId
            var existingCustomer = await _customerRepository.GetCustomerByUserIdAsync(userId);
            if (existingCustomer == null)
            {
                return false;
            }

            // Update customer data (preserve ID and UserId) - only update non-null/non-empty values
            if (!string.IsNullOrWhiteSpace(customer.PhoneNumber))
                existingCustomer.PhoneNumber = customer.PhoneNumber;
            
            if (!string.IsNullOrWhiteSpace(customer.Address))
                existingCustomer.Address = customer.Address;
            
            if (!string.IsNullOrWhiteSpace(customer.City))
                existingCustomer.City = customer.City;
            
            if (!string.IsNullOrWhiteSpace(customer.PostalCode))
                existingCustomer.PostalCode = customer.PostalCode;
            
            if (!string.IsNullOrWhiteSpace(customer.Country))
                existingCustomer.Country = customer.Country;

            _customerRepository.UpdateCustomer(existingCustomer);
            return await _customerRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _customerRepository.GetCustomerAsync(customerId);
            if (customer == null)
            {
                return false;
            }

            _customerRepository.DeleteCustomer(customer);
            return await _customerRepository.SaveChangesAsync();
        }

        public async Task<bool> UserHasCustomerProfileAsync(int userId)
        {
            return await _customerRepository.UserHasCustomerProfileAsync(userId);
        }
    }
}
