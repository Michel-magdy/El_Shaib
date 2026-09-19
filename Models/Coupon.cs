using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Shaib.Models;

public class Coupon
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    [Column(TypeName = "decimal(10,2)")]
    public decimal DiscountValue { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? MinOrderAmount { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? MaxDiscountAmount { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime EndDate { get; set; }

    public int? UsageLimit { get; set; }

    public int UsageCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Helper computed properties
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow > EndDate;

    [NotMapped]
    public bool HasStarted => DateTime.UtcNow >= StartDate;

    [NotMapped]
    public bool IsValidNow => IsActive && HasStarted && !IsExpired && (!UsageLimit.HasValue || UsageCount < UsageLimit.Value);

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

