namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for Product with detailed information including reviews
    /// </summary>
    public class ProductWithReviewsDto
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
        public ICollection<ProductReviewDto> Reviews { get; set; } = new List<ProductReviewDto>();
        public double AverageRating { get; set; }
    }
}