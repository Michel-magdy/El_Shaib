using System.ComponentModel.DataAnnotations.Schema;

namespace El_Shaib.Models;

public class CartItem
{
    public int Id { get; set; }

    public int Quantity { get; set; } = 1;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    [ForeignKey("Customer")]
    public int CustomerId { get; set; }

    [ForeignKey("Product")]
    public int ProductId { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

