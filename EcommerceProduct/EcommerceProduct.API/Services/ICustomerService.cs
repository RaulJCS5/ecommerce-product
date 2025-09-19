using EcommerceProduct.API.Entities;

namespace EcommerceProduct.API.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetCustomersAsync();
        Task<Customer?> GetCustomerAsync(int customerId, bool includeOrders = false);
        Task<IEnumerable<Order>> GetCustomerOrdersAsync(int customerId);
        Task<Customer> CreateCustomerProfileAsync(int userId, Customer customer);
        Task<bool> UpdateCustomerByUserProfileAsync(int userId, Customer customer);
        Task<Customer?> GetCustomerByUserIdAsync(int userId);
        Task<bool> DeleteCustomerAsync(int customerId);
        Task<bool> UserHasCustomerProfileAsync(int userId);
    }
}
