using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceProduct.API.Entities
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice => Quantity * UnitPrice;

        // Foreign key to Order
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
        public int OrderId { get; set; }

        // Foreign key to Product
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
        public int ProductId { get; set; }

        public OrderItem(int quantity, decimal unitPrice, int orderId, int productId)
        {
            Quantity = quantity;
            UnitPrice = unitPrice;
            OrderId = orderId;
            ProductId = productId;
        }
    }
}