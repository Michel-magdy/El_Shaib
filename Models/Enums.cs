namespace El_Shaib.Models;

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

