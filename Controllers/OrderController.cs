using El_Shaib.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace El_Shaib.Controllers;

public class OrderController : Controller
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET: /Order/Track?orderNumber=SHP-...
    [HttpGet]
    public async Task<IActionResult> Track(string? orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            return View();
        }

        var order = await _orderService.GetOrderConfirmationAsync(orderNumber);
        if (order == null)
        {
            ViewBag.ErrorMessage = $"عفواً، لم يتم العثور على أي طلب برقم \"{orderNumber}\". يرجى التأكد من الرقم والمحاولة مرة أخرى.";
        }

        return View(order);
    }
}

