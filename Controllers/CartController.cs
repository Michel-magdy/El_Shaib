using El_Shaib.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace El_Shaib.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetCartAsync();
            return View(cart);
        }

        // POST: /Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var result = await _cartService.AddToCartAsync(productId, quantity);
            return Json(result);
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var result = await _cartService.UpdateQuantityAsync(productId, quantity);
            return Json(result);
        }

        // POST: /Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var result = await _cartService.RemoveFromCartAsync(productId);
            return Json(result);
        }

        // POST: /Cart/Clear
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            await _cartService.ClearCartAsync();
            return Json(new { success = true, message = "تم تفريغ السلة بنجاح." });
        }

        // GET: /Cart/Count
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _cartService.GetCartItemCountAsync();
            return Json(new { count });
        }
    }
}
