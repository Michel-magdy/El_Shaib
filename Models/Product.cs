using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Shaib.Models;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// e.g. "٩٠ بيضة", "١٢ بيضة", "١ كيلو"
    /// </summary>
    [MaxLength(100)]
    public string? Weight { get; set; }

    /// <summary>
    /// e.g. "طبق مفلطح", "كرتون"
    /// </summary>
    [MaxLength(100)]
    public string? PackageType { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? OldPrice { get; set; }

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign Keys
    public int CategoryId { get; set; }

    // Navigation
    public Category? Category { get; set; } = null!;
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    public List<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public List<ProductImage> Images { get; set; } = new List<ProductImage>();
}

