namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for Product Review
    /// </summary>
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsApproved { get; set; }
        public int ProductId { get; set; }
        public ProductDto? Product { get; set; }
    }
}