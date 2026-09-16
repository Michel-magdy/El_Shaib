using El_Shaib.Interfaces;
using El_Shaib.Models;
using El_Shaib.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace El_Shaib.Controllers;

public class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IStorageService _storageService;

    public CheckoutController(ICartService cartService, IOrderService orderService, IStorageService storageService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _storageService = storageService;
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

        // Validate InstaPay receipt if paying electronically / online
        if (model.PaymentMethod == PaymentMethod.Online || model.PaymentMethod == PaymentMethod.BankTransfer)
        {
            if (model.ReceiptImage == null || model.ReceiptImage.Length == 0)
            {
                ModelState.AddModelError("ReceiptImage", "يرجى رفع صورة إيصال تحويل إنستاباي لإتمام عملية الدفع.");
            }
            else
            {
                var ext = Path.GetExtension(model.ReceiptImage.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("ReceiptImage", "صيغة الملف غير مدعومة. يرجى رفع صورة بصيغة JPG أو PNG أو WEBP.");
                }
                else if (model.ReceiptImage.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ReceiptImage", "حجم الصورة كبير جداً. الحد الأقصى المسموح به هو 5 ميجابايت.");
                }
                else
                {
                    try
                    {
                        var fileName = $"receipt_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
                        model.PaymentReceiptUrl = await _storageService.UploadReceiptAsync(model.ReceiptImage, fileName);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ReceiptImage", $"حدث خطأ أثناء حفظ صورة الإيصال: {ex.Message}");
                    }
                }
            }
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

