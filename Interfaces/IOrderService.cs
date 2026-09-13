using El_Shaib.Models;
using El_Shaib.ViewModels;

namespace El_Shaib.Interfaces;

public interface IOrderService
{
    Task<List<DeliveryArea>> GetDeliveryAreasAsync();
    Task<DeliveryArea?> GetDeliveryAreaByIdAsync(int id);
    Task<Order> CreateOrderAsync(CheckoutViewModel model, CartViewModel cart, int? customerId = null);
    Task<Order?> GetOrderByNumberAsync(string orderNumber);
    Task<OrderConfirmationViewModel?> GetOrderConfirmationAsync(string orderNumber);
}

