using System.ComponentModel.DataAnnotations;

namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for updating product stock quantity
    /// </summary>
    public class ProductStockUpdateDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be 0 or greater")]
        public int StockQuantity { get; set; }
    }
}
