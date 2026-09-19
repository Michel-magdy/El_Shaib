using El_Shaib.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace El_Shaib.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;

        public CartController(ICartService cartService, ICouponService couponService)
        {
            _cartService = cartService;
            _couponService = couponService;
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

        // POST: /Cart/ApplyCoupon
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string code)
        {
            var cart = await _cartService.GetCartAsync();
            if (cart.Items.Count == 0)
            {
                return Json(new { success = false, message = "سلة التسوق فارغة." });
            }

            var (success, message, discountAmount, couponCode) = await _couponService.ApplyCouponAsync(code, cart.SubTotal);
            if (!success)
            {
                return Json(new { success = false, message });
            }

            var updatedCart = await _cartService.GetCartAsync();
            return Json(new
            {
                success = true,
                message,
                discount = updatedCart.Discount,
                subTotal = updatedCart.SubTotal,
                total = updatedCart.Total,
                deliveryFee = updatedCart.DeliveryFee,
                couponCode = updatedCart.CouponCode
            });
        }

        // POST: /Cart/RemoveCoupon
        [HttpPost]
        public async Task<IActionResult> RemoveCoupon()
        {
            await _couponService.RemoveCouponAsync();
            var updatedCart = await _cartService.GetCartAsync();
            return Json(new
            {
                success = true,
                message = "تمت إزالة كود الخصم بنجاح.",
                discount = updatedCart.Discount,
                subTotal = updatedCart.SubTotal,
                total = updatedCart.Total,
                deliveryFee = updatedCart.DeliveryFee,
                couponCode = (string?)null
            });
        }
    }
}
