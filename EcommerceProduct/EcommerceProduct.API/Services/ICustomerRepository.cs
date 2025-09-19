using EcommerceProduct.API.Entities;

namespace EcommerceProduct.API.Services
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetCustomersAsync();
        Task<Customer?> GetCustomerAsync(int customerId, bool includeOrders = false);
        Task<Customer?> GetCustomerByUserIdAsync(int userId);
        Task<Customer?> GetCustomerByUserEmailAsync(string email);
        Task<IEnumerable<Order>> GetOrdersForCustomerAsync(int customerId);
        Task<bool> CustomerExistsAsync(int customerId);
        Task<bool> UserHasCustomerProfileAsync(int userId);
        Task AddCustomerAsync(Customer customer);
        void UpdateCustomer(Customer customer);
        void DeleteCustomer(Customer customer);
        Task<bool> SaveChangesAsync();
    }
}
