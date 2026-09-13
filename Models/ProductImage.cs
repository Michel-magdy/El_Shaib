using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Shaib.Models;

public class ProductImage
{
    public int Id { get; set; }

    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Relative path/key inside the Supabase Storage bucket (e.g. products/filename.jpg)
    /// </summary>
    [MaxLength(500)]
    public string? StoragePath { get; set; }

    [MaxLength(200)]
    public string? AltText { get; set; }

    public bool IsPrimary { get; set; }

    // Foreign Keys
    [ForeignKey("Product")]
    public int ProductId { get; set; }

    // Navigation
    public Product? Product { get; set; }
}
