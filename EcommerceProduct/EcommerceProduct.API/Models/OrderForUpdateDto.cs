using System.ComponentModel.DataAnnotations;
using EcommerceProduct.API.Entities;

namespace EcommerceProduct.API.Models
{
    public class OrderForUpdateDto
    {
        /// <summary>
        /// Order status
        /// </summary>
        [Required]
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Shipping address for the order
        /// </summary>
        [MaxLength(500)]
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Additional notes for the order
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
