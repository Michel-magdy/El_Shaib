using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Shaib.Models;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? UnitSize { get; set; }

    [MaxLength(100)]
    public string? PackageType { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? OldPrice { get; set; }

    [Display(Name = "Quantity")]
    public int StockQuantity { get; set; }


    public bool IsFeatured { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign Keys
    [ForeignKey("Category")]
    public int CategoryId { get; set; }

    // Navigation
    public Category? Category { get; set; } = null!;
    // public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    // public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    // public List<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public List<ProductImage> Images { get; set; } = new List<ProductImage>();
}