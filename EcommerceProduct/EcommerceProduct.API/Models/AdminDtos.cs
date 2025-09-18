using System.ComponentModel.DataAnnotations;

namespace EcommerceProduct.API.Models
{
    /// <summary>
    /// Admin dashboard statistics
    /// </summary>
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Bulk review action DTO
    /// </summary>
    public class BulkReviewActionDto
    {
        [Required]
        public List<int> ReviewIds { get; set; } = new List<int>();

        [Required]
        public bool Approve { get; set; }
    }

    /// <summary>
    /// Revenue analytics DTO
    /// </summary>
    public class RevenueAnalyticsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalOrders { get; set; }
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new List<DailyRevenueDto>();
    }

    /// <summary>
    /// Daily revenue data
    /// </summary>
    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    /// <summary>
    /// Top selling product DTO
    /// </summary>
    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
    }

    /// <summary>
    /// Customer analytics DTO
    /// </summary>
    public class CustomerAnalyticsDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal AverageOrdersPerCustomer { get; set; }
        public decimal AverageCustomerValue { get; set; }
    }
}
