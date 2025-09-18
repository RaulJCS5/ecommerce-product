using EcommerceProduct.API.Entities;
using System.Security.Cryptography;
using System.Text;

namespace EcommerceProduct.API.Services
{
    public interface IUserService
    {
        Task<User?> ValidateUserAsync(string username, string password);
        Task<User> RegisterUserAsync(User user, string password);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string username, string email);
        Task UpdateLastLoginAsync(int userId);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user == null || !user.IsActive)
                return null;

            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User> RegisterUserAsync(User user, string password)
        {
            if (await UserExistsAsync(user.Username, user.Email))
                throw new InvalidOperationException("Username or email already exists");

            user.PasswordHash = HashPassword(password);
            user.CreatedAt = DateTime.UtcNow;

            return await _userRepository.CreateUserAsync(user);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetUserByUsernameAsync(username);
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            return await _userRepository.UserExistsAsync(username, email);
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            await _userRepository.UpdateLastLoginAsync(userId);
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var salt = GenerateSalt();
            var passwordWithSalt = password + salt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
            return Convert.ToBase64String(hashedBytes) + ":" + salt;
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                var parts = hashedPassword.Split(':');
                if (parts.Length != 2) return false;

                var hash = parts[0];
                var salt = parts[1];

                using var sha256 = SHA256.Create();
                var passwordWithSalt = password + salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
                var computedHash = Convert.ToBase64String(hashedBytes);

                return hash == computedHash;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }
    }
}