using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Shaib.Models;

public class DeliveryArea
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    /// <summary>
    /// e.g. "القصيم", "بريدة", "عنيزة"
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Delivery fee for this area
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// Estimated delivery time, e.g. "خلال ساعتين"
    /// </summary>
    [MaxLength(100)]
    public string? EstimatedTime { get; set; }
}
