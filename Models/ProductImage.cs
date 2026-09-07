using System.ComponentModel.DataAnnotations;

namespace El_Shaib.Models;

public class ProductImage
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AltText { get; set; }

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }

    // Foreign Keys
    public int ProductId { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}

