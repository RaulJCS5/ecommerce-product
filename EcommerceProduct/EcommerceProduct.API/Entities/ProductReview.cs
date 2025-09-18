using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceProduct.API.Entities
{
    public class ProductReview
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? CustomerEmail { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = false;

        // Foreign key to Product
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public int ProductId { get; set; }

        public ProductReview(int rating, string customerName, int productId)
        {
            Rating = rating;
            CustomerName = customerName;
            ProductId = productId;
        }
    }
}