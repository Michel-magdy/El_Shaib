using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Shaib.Models;

public class ProductImage
{
    public int Id { get; set; }

    [Required(ErrorMessage = "must provide an Image Link"), MaxLength(500)]
    public string ImageUrl { get; set; }

    [MaxLength(200)]
    public string? AltText { get; set; }

    public bool IsPrimary { get; set; }

    // Foreign Keys
    [ForeignKey("Product")]
    public int ProductId { get; set; }

    // Navigation
    public Product? Product { get; set; }
}

