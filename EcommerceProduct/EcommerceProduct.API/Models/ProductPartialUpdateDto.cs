using System.ComponentModel.DataAnnotations;

namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for partial product updates - all fields are optional
    /// Use this when you want to update only specific fields without affecting others
    /// </summary>
    public class ProductPartialUpdateDto
    {
        [MaxLength(100, ErrorMessage = "Product name can't be more than 100 characters.")]
        public string? Name { get; set; }

        [MaxLength(500, ErrorMessage = "Description can't be more than 500 characters.")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be 0 or greater.")]
        public int? StockQuantity { get; set; }

        public int? CategoryId { get; set; }

        [MaxLength(50, ErrorMessage = "SKU can't be more than 50 characters.")]
        public string? SKU { get; set; }

        [MaxLength(255, ErrorMessage = "Image URL can't be more than 255 characters.")]
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }
    }
}