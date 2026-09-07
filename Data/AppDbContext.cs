using El_Shaib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace El_Shaib.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<DeliveryArea> DeliveryAreas => Set<DeliveryArea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
        });

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(p => p.CategoryId);
            e.HasIndex(p => p.IsActive);
            e.HasIndex(p => p.IsFeatured);
        });

        // ProductImage
        modelBuilder.Entity<ProductImage>(e =>
        {
            e.HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Customer
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasIndex(c => c.Email).IsUnique();
            e.HasIndex(c => c.Phone).IsUnique();
        });

        // Address
        // Address — UserId is Supabase Auth UUID (no FK to local table)
        modelBuilder.Entity<Address>(e =>
        {
            e.HasOne(a => a.Customer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            // e.HasIndex(a => a.UserId);
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.OrderNumber).IsUnique();
            // e.HasIndex(o => o.UserId);
            e.HasIndex(o => o.Status);
            e.HasIndex(o => o.CreatedAt);

            e.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CartItem — one item per product per customer
        // CartItem — one item per product per user
        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasIndex(ci => new { ci.CustomerId, ci.ProductId }).IsUnique();
            // e.HasIndex(ci => new { ci.UserId, ci.ProductId }).IsUnique();
            // e.HasIndex(ci => ci.UserId);

            e.HasOne(ci => ci.Customer)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Wishlist — one entry per product per customer
        // Wishlist — one entry per product per user
        modelBuilder.Entity<Wishlist>(e =>
        {
            e.HasIndex(w => new { w.CustomerId, w.ProductId }).IsUnique();
            // e.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
            // e.HasIndex(w => w.UserId);

            e.HasOne(w => w.Customer)
                .WithMany(c => c.Wishlists)
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(w => w.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DeliveryArea
        modelBuilder.Entity<DeliveryArea>(e =>
        {
            e.HasIndex(d => d.Name).IsUnique();
        });
    }
}

