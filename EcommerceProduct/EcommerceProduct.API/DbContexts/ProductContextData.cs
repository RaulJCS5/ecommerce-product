//using Microsoft.EntityFrameworkCore;
//using EcommerceProduct.API.Entities;

//namespace EcommerceProduct.API.DbContexts
//{
//    public class ProductContext : DbContext
//    {
//        public DbSet<Product> Products { get; set; }
//        public DbSet<ProductCategory> ProductCategories { get; set; }
//        public DbSet<ProductReview> ProductReviews { get; set; }
//        public DbSet<Customer> Customers { get; set; }
//        public DbSet<Order> Orders { get; set; }
//        public DbSet<OrderItem> OrderItems { get; set; }

//        public ProductContext(DbContextOptions<ProductContext> options)
//            : base(options)
//        {
//        }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            // Configure Product entity
//            modelBuilder.Entity<Product>()
//                .Property(p => p.Price)
//                .HasColumnType("decimal(18,2)");

//            // Configure OrderItem entity
//            modelBuilder.Entity<OrderItem>()
//                .Property(oi => oi.UnitPrice)
//                .HasColumnType("decimal(18,2)");

//            // Configure Order entity
//            modelBuilder.Entity<Order>()
//                .Property(o => o.TotalAmount)
//                .HasColumnType("decimal(18,2)");

//            // Seed Product Categories
//            modelBuilder.Entity<ProductCategory>()
//                .HasData(
//                    new ProductCategory("Electronics")
//                    {
//                        Id = 1,
//                        Description = "Electronic devices and gadgets"
//                    },
//                    new ProductCategory("Clothing")
//                    {
//                        Id = 2,
//                        Description = "Apparel and fashion items"
//                    },
//                    new ProductCategory("Books")
//                    {
//                        Id = 3,
//                        Description = "Books and educational materials"
//                    },
//                    new ProductCategory("Home & Garden")
//                    {
//                        Id = 4,
//                        Description = "Items for home and garden improvement"
//                    },
//                    new ProductCategory("Sports & Outdoors")
//                    {
//                        Id = 5,
//                        Description = "Sports equipment and outdoor gear"
//                    }
//                );

//            // Seed Products
//            modelBuilder.Entity<Product>()
//                .HasData(
//                    new Product("Smartphone", 699.99m, 50, 1)
//                    {
//                        Id = 1,
//                        Description = "Latest smartphone with advanced features",
//                        SKU = "SP001",
//                        ImageUrl = "https://example.com/images/smartphone.jpg"
//                    },
//                    new Product("Laptop", 1299.99m, 25, 1)
//                    {
//                        Id = 2,
//                        Description = "High-performance laptop for professionals",
//                        SKU = "LP001",
//                        ImageUrl = "https://example.com/images/laptop.jpg"
//                    },
//                    new Product("T-Shirt", 29.99m, 100, 2)
//                    {
//                        Id = 3,
//                        Description = "Comfortable cotton t-shirt",
//                        SKU = "TS001",
//                        ImageUrl = "https://example.com/images/tshirt.jpg"
//                    },
//                    new Product("Programming Book", 49.99m, 30, 3)
//                    {
//                        Id = 4,
//                        Description = "Comprehensive guide to modern programming",
//                        SKU = "BK001",
//                        ImageUrl = "https://example.com/images/book.jpg"
//                    },
//                    new Product("Garden Tool Set", 89.99m, 15, 4)
//                    {
//                        Id = 5,
//                        Description = "Complete set of essential garden tools",
//                        SKU = "GT001",
//                        ImageUrl = "https://example.com/images/garden-tools.jpg"
//                    },
//                    new Product("Running Shoes", 129.99m, 40, 5)
//                    {
//                        Id = 6,
//                        Description = "Professional running shoes for athletes",
//                        SKU = "RS001",
//                        ImageUrl = "https://example.com/images/running-shoes.jpg"
//                    }
//                );

//            // Seed Customers
//            modelBuilder.Entity<Customer>()
//                .HasData(
//                    new Customer("John", "Doe", "john.doe@example.com")
//                    {
//                        Id = 1,
//                        PhoneNumber = "+1-555-0101",
//                        Address = "123 Main St",
//                        City = "New York",
//                        PostalCode = "10001",
//                        Country = "USA"
//                    },
//                    new Customer("Jane", "Smith", "jane.smith@example.com")
//                    {
//                        Id = 2,
//                        PhoneNumber = "+1-555-0102",
//                        Address = "456 Oak Ave",
//                        City = "Los Angeles",
//                        PostalCode = "90210",
//                        Country = "USA"
//                    },
//                    new Customer("Bob", "Johnson", "bob.johnson@example.com")
//                    {
//                        Id = 3,
//                        PhoneNumber = "+1-555-0103",
//                        Address = "789 Pine Rd",
//                        City = "Chicago",
//                        PostalCode = "60601",
//                        Country = "USA"
//                    }
//                );

//            // Seed Orders
//            modelBuilder.Entity<Order>()
//                .HasData(
//                    new Order("ORD-2024-001", 1)
//                    {
//                        Id = 1,
//                        TotalAmount = 729.98m,
//                        Status = OrderStatus.Delivered,
//                        OrderDate = DateTime.UtcNow.AddDays(-10),
//                        ShippingAddress = "123 Main St, New York, NY 10001"
//                    },
//                    new Order("ORD-2024-002", 2)
//                    {
//                        Id = 2,
//                        TotalAmount = 1329.98m,
//                        Status = OrderStatus.Shipped,
//                        OrderDate = DateTime.UtcNow.AddDays(-5),
//                        ShippingAddress = "456 Oak Ave, Los Angeles, CA 90210"
//                    },
//                    new Order("ORD-2024-003", 3)
//                    {
//                        Id = 3,
//                        TotalAmount = 179.98m,
//                        Status = OrderStatus.Processing,
//                        OrderDate = DateTime.UtcNow.AddDays(-2),
//                        ShippingAddress = "789 Pine Rd, Chicago, IL 60601"
//                    }
//                );

//            // Seed Order Items
//            modelBuilder.Entity<OrderItem>()
//                .HasData(
//                    // Order 1 items
//                    new OrderItem(1, 699.99m, 1, 1) { Id = 1 }, // Smartphone
//                    new OrderItem(1, 29.99m, 1, 3) { Id = 2 },  // T-Shirt

//                    // Order 2 items
//                    new OrderItem(1, 1299.99m, 2, 2) { Id = 3 }, // Laptop
//                    new OrderItem(1, 29.99m, 2, 3) { Id = 4 },   // T-Shirt

//                    // Order 3 items
//                    new OrderItem(1, 129.99m, 3, 6) { Id = 5 },  // Running Shoes
//                    new OrderItem(1, 49.99m, 3, 4) { Id = 6 }    // Programming Book
//                );

//            // Seed Product Reviews
//            modelBuilder.Entity<ProductReview>()
//                .HasData(
//                    new ProductReview(5, "John Doe", 1)
//                    {
//                        Id = 1,
//                        Comment = "Excellent smartphone! Great performance and battery life.",
//                        CustomerEmail = "john.doe@example.com",
//                        IsApproved = true,
//                        CreatedDate = DateTime.UtcNow.AddDays(-8)
//                    },
//                    new ProductReview(4, "Jane Smith", 2)
//                    {
//                        Id = 2,
//                        Comment = "Very good laptop for work. Fast and reliable.",
//                        CustomerEmail = "jane.smith@example.com",
//                        IsApproved = true,
//                        CreatedDate = DateTime.UtcNow.AddDays(-6)
//                    },
//                    new ProductReview(5, "Bob Johnson", 6)
//                    {
//                        Id = 3,
//                        Comment = "Perfect running shoes! Very comfortable.",
//                        CustomerEmail = "bob.johnson@example.com",
//                        IsApproved = true,
//                        CreatedDate = DateTime.UtcNow.AddDays(-3)
//                    },
//                    new ProductReview(4, "Alice Brown", 3)
//                    {
//                        Id = 4,
//                        Comment = "Good quality t-shirt, fits well.",
//                        CustomerEmail = "alice.brown@example.com",
//                        IsApproved = false, // Pending approval
//                        CreatedDate = DateTime.UtcNow.AddDays(-1)
//                    }
//                );

//            base.OnModelCreating(modelBuilder);
//        }
//    }
//}