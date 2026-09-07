using System.ComponentModel.DataAnnotations;

namespace El_Shaib.Models;

public class Address
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string District { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Region { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(500)]
    public string? AdditionalDetails { get; set; }

    public bool IsDefault { get; set; }

    // Foreign Keys
    public int CustomerId { get; set; }

    // Navigation
    public Customer? Customer { get; set; } = null!;
}

