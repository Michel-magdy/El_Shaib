using System.ComponentModel.DataAnnotations;
using El_Shaib.Models;
using Microsoft.AspNetCore.Http;

namespace El_Shaib.ViewModels;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "يرجى إدخال الاسم بالكامل")]
    [Display(Name = "الاسم بالكامل")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال رقم الهاتف للتواصل")]
    [Display(Name = "رقم الهاتف")]
    [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
    [Display(Name = "البريد الإلكتروني (اختياري)")]
    [MaxLength(150)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "يرجى اختيار منطقة التوصيل")]
    [Display(Name = "منطقة التوصيل")]
    public int DeliveryAreaId { get; set; }

    [Required(ErrorMessage = "يرجى إدخال اسم الشارع ورقم المبنى")]
    [Display(Name = "الشارع والمبنى")]
    [MaxLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال الحي أو المنطقة")]
    [Display(Name = "الحي")]
    [MaxLength(100)]
    public string District { get; set; } = string.Empty;

    [Display(Name = "المدينة")]
    [MaxLength(100)]
    public string City { get; set; } = "الأقصر";

    [Display(Name = "ملاحظات إضافية للتوصيل")]
    [MaxLength(500)]
    public string? AdditionalNotes { get; set; }

    [Display(Name = "طريقة الدفع")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    [Display(Name = "صورة إيصال التحويل (إنستاباي)")]
    public IFormFile? ReceiptImage { get; set; }

    [Display(Name = "رقم العملية / المرجع إن وُجد")]
    [MaxLength(100)]
    public string? TransactionReference { get; set; }

    public string? PaymentReceiptUrl { get; set; }

    // View data
    public CartViewModel Cart { get; set; } = new();
    public List<DeliveryArea> AvailableDeliveryAreas { get; set; } = new();
    public decimal SelectedAreaDeliveryFee { get; set; }
}

public class OrderConfirmationViewModel
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? EstimatedTime { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentReceiptUrl { get; set; }
    public string? TransactionReference { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemViewModel> Items { get; set; } = new();
}

public class OrderItemViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

