using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace El_Shaib.Models;

public class Order
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [Column(TypeName = "decimal(10,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DeliveryFee { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Discount { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? DeliveryAddress { get; set; }

    [MaxLength(20)]
    public string? DeliveryPhone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeliveredAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    // Foreign Keys
    [ForeignKey("Customer")]
    public int CustomerId { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

