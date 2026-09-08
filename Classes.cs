/*

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


public class CartItem
{
    public int Id { get; set; }

    public int Quantity { get; set; } = 1;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int CustomerId { get; set; }
    public int ProductId { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Product Product { get; set; } = null!;
}


public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}


public class ContactMessage
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required, MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


public class Customer
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? PasswordHash { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}


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
{
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

        /// <summary>
        /// Delivery address snapshot at time of order
        /// </summary>
        [MaxLength(500)]
        public string? DeliveryAddress { get; set; }

        [MaxLength(20)]
        public string? DeliveryPhone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeliveredAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        // Foreign Keys
        public int CustomerId { get; set; }

        // Navigation
        public Customer Customer { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Processing = 2,
        OutForDelivery = 3,
        Delivered = 4,
        Cancelled = 5,
        Returned = 6
    }

    public enum PaymentMethod
    {
        CashOnDelivery = 0,
        Online = 1,
        BankTransfer = 2
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Failed = 2,
        Refunded = 3
    }
}


public class OrderItem
{
    public int Id { get; set; }

    public int Quantity { get; set; }

    /// <summary>
    /// Price at time of purchase (snapshot)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Product name snapshot in case product is deleted later
    /// </summary>
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    // Foreign Keys
    public int OrderId { get; set; }
    public int ProductId { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}


public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// e.g. "٩٠ بيضة", "١٢ بيضة", "١ كيلو"
    /// </summary>
    [MaxLength(100)]
    public string? Weight { get; set; }

    /// <summary>
    /// e.g. "طبق مفلطح", "كرتون"
    /// </summary>
    [MaxLength(100)]
    public string? PackageType { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? OldPrice { get; set; }

    [MaxLength(300)]
    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign Keys
    public int CategoryId { get; set; }

    // Navigation
    public Category? Category { get; set; } = null!;
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    public List<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public List<ProductImage> Images { get; set; } = new List<ProductImage>();
}


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


public class Wishlist
{
    public int Id { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int CustomerId { get; set; }
    public int ProductId { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Product Product { get; set; } = null!;
}


*/