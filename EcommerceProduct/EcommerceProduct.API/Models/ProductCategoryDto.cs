namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for Product Category
    /// </summary>
    public class ProductCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int NumberOfProducts { get; set; }
    }
}