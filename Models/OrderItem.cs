using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Shaib.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }

    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    // Foreign Keys
    [ForeignKey("Order")]
    public int OrderId { get; set; }

    [ForeignKey("Product")]
    public int ProductId { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

