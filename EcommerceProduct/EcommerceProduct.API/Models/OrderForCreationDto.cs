using System.ComponentModel.DataAnnotations;

namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for creating a new order
    /// </summary>
    public class OrderForCreationDto
    {
        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(300)]
        public string? ShippingAddress { get; set; }

        [Required]
        public List<OrderItemForCreationDto> OrderItems { get; set; } = new List<OrderItemForCreationDto>();
    }

    /// <summary>
    /// DTO for creating order items
    /// </summary>
    public class OrderItemForCreationDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be greater than 0")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }
    }
}
