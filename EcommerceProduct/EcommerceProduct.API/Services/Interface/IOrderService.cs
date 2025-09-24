using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;

namespace EcommerceProduct.API.Services.Interface
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersForCustomerAsync(int customerId);
        Task<Order?> GetOrderAsync(int orderId, bool includeOrderItems = false);
        Task<Order> CreateOrderAsync(OrderForCreationDto orderDto, int customerId);
        Task<bool> UpdateOrderAsync(int orderId, OrderForUpdateDto orderDto);
        Task<bool> CancelOrderAsync(int orderId);
        Task<bool> OrderExistsAsync(int orderId);
    }
}
