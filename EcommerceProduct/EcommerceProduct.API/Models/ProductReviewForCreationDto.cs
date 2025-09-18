using System.ComponentModel.DataAnnotations;

namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for creating a new product review
    /// </summary>
    public class ProductReviewForCreationDto
    {
        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment can't be more than 1000 characters.")]
        public string? Comment { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [MaxLength(100, ErrorMessage = "Customer name can't be more than 100 characters.")]
        public string CustomerName { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Customer email can't be more than 200 characters.")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
        public string? CustomerEmail { get; set; }
    }
}