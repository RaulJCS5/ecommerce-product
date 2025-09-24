using EcommerceProduct.API.DbContexts;
using EcommerceProduct.API.Repository.Interface;
using EcommerceProduct.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProduct.API.Repository.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ProductContext _context;

        public OrderRepository(ProductContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c!.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersForCustomerAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderAsync(int orderId, bool includeOrderItems = false)
        {
            if (includeOrderItems)
            {
                return await _context.Orders
                    .Include(o => o.Customer)
                        .ThenInclude(c => c!.User)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);
            }

            return await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<bool> OrderExistsAsync(int orderId)
        {
            return await _context.Orders.AnyAsync(o => o.Id == orderId);
        }

        public void AddOrderAsync(Order order)
        {
            _context.Orders.Add(order);
        }

        public void UpdateOrder(Order order)
        {
            // No need to do anything here since the entity is tracked
        }

        public void DeleteOrder(Order order)
        {
            _context.Orders.Remove(order);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }
    }
}
