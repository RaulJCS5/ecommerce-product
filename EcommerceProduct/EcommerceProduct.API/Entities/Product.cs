using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceProduct.API.Entities
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [MaxLength(50)]
        public string? SKU { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        // Foreign key to ProductCategory
        [ForeignKey("CategoryId")]
        public ProductCategory? Category { get; set; }
        public int CategoryId { get; set; }

        // Navigation property for reviews
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();

        // Navigation property for order items
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public Product(string name, decimal price, int stockQuantity, int categoryId)
        {
            Name = name;
            Price = price;
            StockQuantity = stockQuantity;
            CategoryId = categoryId;
        }
    }
}