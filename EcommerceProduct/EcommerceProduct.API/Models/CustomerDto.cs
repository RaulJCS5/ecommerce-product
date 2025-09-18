namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for Customer
    /// </summary>
    public class CustomerDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfOrders { get; set; }
    }
}