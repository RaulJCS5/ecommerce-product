using EcommerceProduct.API.Entities;

namespace EcommerceProduct.API.Services.Interface
{
    public interface IUserService
    {
        Task<User?> ValidateUserAsync(string username, string password);
        Task<User> RegisterUserAsync(User user, string password);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserWithCustomerAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string username, string email);
        Task UpdateLastLoginAsync(int userId);
        Task<bool> DeleteUserAsync(int userId);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}
