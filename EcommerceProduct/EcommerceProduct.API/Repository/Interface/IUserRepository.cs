using EcommerceProduct.API.Entities;

namespace EcommerceProduct.API.Repository.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserWithCustomerAsync(int id);
        Task<User> CreateUserAsync(User user);
        Task<Customer> CreateCustomerProfileAsync(int userId, Customer customer);
        Task<bool> UserExistsAsync(string username, string email);
        Task<bool> UserHasCustomerProfileAsync(int userId);
        Task UpdateLastLoginAsync(int userId);
        Task<bool> DeleteUserAsync(int userId);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<int> GetTotalUsersCountAsync();
        void UpdateUser(User user);
        Task<bool> SaveChangesAsync();
    }
}
