namespace El_Shaib.ViewModels;

public class CartItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int Quantity { get; set; }
    public int StockQuantity { get; set; }
    public string? UnitSize { get; set; }
    public string? PackageType { get; set; }
    public string? CategoryName { get; set; }

    public decimal LineTotal => Price * Quantity;
}

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal SubTotal => Items.Sum(i => i.LineTotal);
    public decimal FreeDeliveryThreshold { get; set; } = 200m;
    public decimal DeliveryFee => SubTotal >= FreeDeliveryThreshold || SubTotal == 0 ? 0m : 25m;
    public decimal Discount { get; set; } = 0m;
    public decimal Total => Math.Max(0m, SubTotal + DeliveryFee - Discount);
    public int TotalItemCount => Items.Sum(i => i.Quantity);
    public bool IsFreeDelivery => DeliveryFee == 0 && SubTotal > 0;
}

public class CartActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Total { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal LineTotal { get; set; }
}

