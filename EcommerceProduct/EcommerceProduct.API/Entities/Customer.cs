using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceProduct.API.Entities
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        // Foreign key to User entity
        public int UserId { get; set; }

        // Navigation property to User
        public User User { get; set; } = null!;

        // Navigation property for orders
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public Customer()
        {
        }

        public Customer(int userId)
        {
            UserId = userId;
        }
    }
}