using EcommerceProduct.API.DbContexts;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProduct.API.Repository.Implementation
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ProductContext _context;

        public CustomerRepository(ProductContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .Include(c => c.User)
                .Where(c => c.User.IsActive)
                .OrderBy(c => c.User.LastName)
                .ThenBy(c => c.User.FirstName)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerAsync(int customerId, bool includeOrders = false)
        {
            if (includeOrders)
            {
                return await _context.Customers
                    .Include(c => c.User)
                    .Include(c => c.Orders)
                        .ThenInclude(o => o.OrderItems)
                            .ThenInclude(oi => oi.Product)
                    .Where(c => c.Id == customerId)
                    .FirstOrDefaultAsync();
            }

            return await _context.Customers
                .Include(c => c.User)
                .Where(c => c.Id == customerId)
                .FirstOrDefaultAsync();
        }

        public async Task<Customer?> GetCustomerByUserIdAsync(int userId)
        {
            return await _context.Customers
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<Customer?> GetCustomerByUserEmailAsync(string email)
        {
            return await _context.Customers
                .Include(c => c.User)
                .Where(c => c.User.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersForCustomerAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            return await _context.Customers.AnyAsync(c => c.Id == customerId);
        }

        public async Task<bool> UserHasCustomerProfileAsync(int userId)
        {
            return await _context.Customers.AnyAsync(c => c.UserId == userId);
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }

        public void UpdateCustomer(Customer customer)
        {
            _context.Customers.Update(customer);
        }

        public void DeleteCustomer(Customer customer)
        {
            _context.Customers.Remove(customer);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }
    }
}
