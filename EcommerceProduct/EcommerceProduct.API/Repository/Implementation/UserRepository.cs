using EcommerceProduct.API.DbContexts;
using EcommerceProduct.API.Entities;
using EcommerceProduct.API.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProduct.API.Repository.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly ProductContext _context;

        public UserRepository(ProductContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserWithCustomerAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Customer> CreateCustomerProfileAsync(int userId, Customer customer)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(userId));
            }

            customer.UserId = userId;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Load the customer with user information
            var createdCustomer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == customer.Id);

            return createdCustomer ?? customer;
        }

        public async Task<bool> UserHasCustomerProfileAsync(int userId)
        {
            return await _context.Customers
                .AnyAsync(c => c.UserId == userId);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username || u.Email == email);
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Customer) // Include customer profile if exists
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false; // User not found
            }

            // If the user has a customer profile, remove it first
            if (user.Customer != null)
            {
                _context.Customers.Remove(user.Customer);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                //.Include(u => u.Customer)
                //.OrderBy(u => u.LastName)
                //.ThenBy(u => u.FirstName)
                .ToListAsync();
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public void UpdateUser(User user)
        {
            // Entity Framework will track changes automatically for entities already in context
            // If the entity is not being tracked, attach and mark as modified
            if (_context.Entry(user).State == EntityState.Detached)
            {
                _context.Users.Attach(user);
                _context.Entry(user).State = EntityState.Modified;
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }
    }
}