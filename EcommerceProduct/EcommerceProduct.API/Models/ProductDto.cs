namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for Product without sensitive information
    /// </summary>
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? SKU { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int NumberOfReviews { get; set; }
        public double AverageRating { get; set; }
    }
}