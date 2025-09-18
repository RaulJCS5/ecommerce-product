using System.ComponentModel.DataAnnotations;

namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// DTO for updating user role
    /// </summary>
    public class UserRoleUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;
    }
}
