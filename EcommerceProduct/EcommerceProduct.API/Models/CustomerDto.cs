namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for Customer
    /// </summary>
    public class CustomerDto
    {
        public int Id { get; set; }

        // User information (from User entity)
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        // Customer-specific information
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public int NumberOfOrders { get; set; }
    }
}