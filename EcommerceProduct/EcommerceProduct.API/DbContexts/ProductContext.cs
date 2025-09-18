using Microsoft.EntityFrameworkCore;
using EcommerceProduct.API.Entities;

namespace EcommerceProduct.API.DbContexts
{
    public class ProductContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }

        public ProductContext(DbContextOptions<ProductContext> options)
            : base(options)
        {
            // Database.EnsureCreated();
            // Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Customer)
                .WithMany()
                .HasForeignKey(u => u.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure ProductCategory
            modelBuilder.Entity<ProductCategory>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Configure Product
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Configure Customer
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Configure Order
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // Configure OrderItem
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            // Seed data for ProductCategories
            //modelBuilder.Entity<ProductCategory>()
            //    .HasData(
            //        new ProductCategory("Electronics")
            //        {
            //            Id = 1,
            //            Description = "Electronic devices and gadgets"
            //        },
            //        new ProductCategory("Clothing")
            //        {
            //            Id = 2,
            //            Description = "Apparel and fashion items"
            //        },
            //        new ProductCategory("Books")
            //        {
            //            Id = 3,
            //            Description = "Books and educational materials"
            //        },
            //        new ProductCategory("Home & Garden")
            //        {
            //            Id = 4,
            //            Description = "Home improvement and garden supplies"
            //        }
            //    );

            // Seed data for Products
            //modelBuilder.Entity<Product>()
            //    .HasData(
            //        new Product("Smartphone", 599.99m, 50, 1)
            //        {
            //            Id = 1,
            //            Description = "Latest model smartphone with advanced features",
            //            SKU = "ELEC-PHONE-001",
            //            ImageUrl = "/images/smartphone.jpg"
            //        },
            //        new Product("Laptop", 999.99m, 25, 1)
            //        {
            //            Id = 2,
            //            Description = "High-performance laptop for work and gaming",
            //            SKU = "ELEC-LAPTOP-001",
            //            ImageUrl = "/images/laptop.jpg"
            //        },
            //        new Product("T-Shirt", 19.99m, 100, 2)
            //        {
            //            Id = 3,
            //            Description = "Comfortable cotton t-shirt",
            //            SKU = "CLOTH-TSHIRT-001",
            //            ImageUrl = "/images/tshirt.jpg"
            //        },
            //        new Product("Programming Book", 45.99m, 30, 3)
            //        {
            //            Id = 4,
            //            Description = "Learn programming with this comprehensive guide",
            //            SKU = "BOOK-PROG-001",
            //            ImageUrl = "/images/programming-book.jpg"
            //        }
            //    );

            // Seed data for Customers
            //modelBuilder.Entity<Customer>()
            //    .HasData(
            //        new Customer("John", "Doe", "john.doe@email.com")
            //        {
            //            Id = 1,
            //            PhoneNumber = "+1234567890",
            //            Address = "123 Main St",
            //            City = "New York",
            //            PostalCode = "10001",
            //            Country = "USA"
            //        },
            //        new Customer("Jane", "Smith", "jane.smith@email.com")
            //        {
            //            Id = 2,
            //            PhoneNumber = "+0987654321",
            //            Address = "456 Oak Ave",
            //            City = "Los Angeles",
            //            PostalCode = "90210",
            //            Country = "USA"
            //        }
            //    );

            base.OnModelCreating(modelBuilder);
        }
    }
}