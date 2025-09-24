using EcommerceProduct.API.Repository.Interface;
using EcommerceProduct.API.Services.Interface;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Models;

namespace EcommerceProduct.API.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductService _productService;

        public OrderService(IOrderRepository orderRepository, IProductService productService)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _orderRepository.GetOrdersAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersForCustomerAsync(int customerId)
        {
            return await _orderRepository.GetOrdersForCustomerAsync(customerId);
        }

        public async Task<Order?> GetOrderAsync(int orderId, bool includeOrderItems = false)
        {
            return await _orderRepository.GetOrderAsync(orderId, includeOrderItems);
        }

        public async Task<Order> CreateOrderAsync(OrderForCreationDto orderDto, int customerId)
        {
            // Validate products and calculate total
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var itemDto in orderDto.OrderItems)
            {
                var product = await _productService.GetProductAsync(itemDto.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {itemDto.ProductId} not found.");
                }

                if (product.StockQuantity < itemDto.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {itemDto.Quantity}");
                }

                var orderItem = new OrderItem(itemDto.Quantity, product.Price, 0, itemDto.ProductId);
                orderItems.Add(orderItem);
                totalAmount += itemDto.Quantity * product.Price;
            }

            // Generate order number
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            // Create order
            var order = new Order(orderNumber, customerId)
            {
                Notes = orderDto.Notes,
                ShippingAddress = orderDto.ShippingAddress,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending
            };

            // Add order items
            foreach (var item in orderItems)
            {
                order.OrderItems.Add(item);
            }

            _orderRepository.AddOrderAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order;
        }

        public async Task<bool> UpdateOrderAsync(int orderId, OrderForUpdateDto orderDto)
        {
            var order = await _orderRepository.GetOrderAsync(orderId);
            if (order == null)
            {
                return false;
            }

            // Update order properties
            if (!string.IsNullOrEmpty(orderDto.Notes))
                order.Notes = orderDto.Notes;

            if (!string.IsNullOrEmpty(orderDto.ShippingAddress))
                order.ShippingAddress = orderDto.ShippingAddress;

            order.Status = orderDto.Status;

            _orderRepository.UpdateOrder(order);
            await _orderRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderAsync(orderId);
            if (order == null)
            {
                return false;
            }

            // Check if order can be cancelled
            if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
            {
                throw new InvalidOperationException("Cannot cancel order that has been shipped or delivered.");
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Order is already cancelled.");
            }

            order.Status = OrderStatus.Cancelled;
            _orderRepository.UpdateOrder(order);
            await _orderRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> OrderExistsAsync(int orderId)
        {
            return await _orderRepository.OrderExistsAsync(orderId);
        }
    }
}
