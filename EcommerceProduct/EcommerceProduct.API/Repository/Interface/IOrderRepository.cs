using EcommerceProduct.API.Entities;

namespace EcommerceProduct.API.Repository.Interface
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersForCustomerAsync(int customerId);
        Task<Order?> GetOrderAsync(int orderId, bool includeOrderItems = false);
        Task<bool> OrderExistsAsync(int orderId);
        void AddOrderAsync(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(Order order);
        Task<bool> SaveChangesAsync();
    }
}
