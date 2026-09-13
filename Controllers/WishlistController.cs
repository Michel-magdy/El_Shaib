using El_Shaib.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace El_Shaib.Controllers;

public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    // GET: /Wishlist
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var products = await _wishlistService.GetWishlistProductsAsync();
        return View(products);
    }

    // POST: /Wishlist/Toggle
    [HttpPost]
    public async Task<IActionResult> Toggle(int productId)
    {
        var isAdded = await _wishlistService.ToggleWishlistAsync(productId);
        var count = await _wishlistService.GetWishlistCountAsync();
        return Json(new
        {
            success = true,
            inWishlist = isAdded,
            count,
            message = isAdded ? "تمت إضافة المنتج إلى المفضلة" : "تمت إزالة المنتج من المفضلة"
        });
    }

    // GET: /Wishlist/Count
    [HttpGet]
    public async Task<IActionResult> Count()
    {
        var count = await _wishlistService.GetWishlistCountAsync();
        return Json(new { count });
    }
}

