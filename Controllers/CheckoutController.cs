using El_Shaib.Interfaces;
using El_Shaib.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace El_Shaib.Controllers;

public class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;

    public CheckoutController(ICartService cartService, IOrderService orderService)
    {
        _cartService = cartService;
        _orderService = orderService;
    }

    // GET: /Checkout
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await _cartService.GetCartAsync();
        if (cart.Items.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        var areas = await _orderService.GetDeliveryAreasAsync();
        var model = new CheckoutViewModel
        {
            Cart = cart,
            AvailableDeliveryAreas = areas,
            DeliveryAreaId = areas.FirstOrDefault()?.Id ?? 1
        };

        return View(model);
    }

    // POST: /Checkout/PlaceOrder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
    {
        var cart = await _cartService.GetCartAsync();
        if (cart.Items.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        if (!ModelState.IsValid)
        {
            model.Cart = cart;
            model.AvailableDeliveryAreas = await _orderService.GetDeliveryAreasAsync();
            return View("Index", model);
        }

        try
        {
            var order = await _orderService.CreateOrderAsync(model, cart);
            await _cartService.ClearCartAsync();
            return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.Cart = cart;
            model.AvailableDeliveryAreas = await _orderService.GetDeliveryAreasAsync();
            return View("Index", model);
        }
    }

    // GET: /Checkout/Confirmation?orderNumber=SHP-...
    [HttpGet]
    public async Task<IActionResult> Confirmation(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            return RedirectToAction("Index", "Home");
        }

        var confirmation = await _orderService.GetOrderConfirmationAsync(orderNumber);
        if (confirmation == null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(confirmation);
    }
}

