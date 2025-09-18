using System.ComponentModel.DataAnnotations;

namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for updating an existing product
    /// </summary>
    public class ProductForUpdateDto
    {
        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(100, ErrorMessage = "Product name can't be more than 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description can't be more than 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be 0 or greater.")]
        public int StockQuantity { get; set; }

        [MaxLength(50, ErrorMessage = "SKU can't be more than 50 characters.")]
        public string? SKU { get; set; }

        [MaxLength(255, ErrorMessage = "Image URL can't be more than 255 characters.")]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Category ID is required.")]
        public int CategoryId { get; set; }
    }
}