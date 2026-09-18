using System.Text.Json;
using El_Shaib.Interfaces;
using El_Shaib.Models;
using El_Shaib.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace El_Shaib.Services;

public class CartService : ICartService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private const string CartSessionKey = "ELSHAIB_CART_SESSION";

    public CartService(IHttpContextAccessor httpContextAccessor, AppDbContext context, IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _cache = cache;
    }

    private ISession Session => _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("Session is not available in the current context.");

    private Dictionary<int, int> GetSessionCart()
    {
        var json = Session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(json))
        {
            return new Dictionary<int, int>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? new Dictionary<int, int>();
        }
        catch
        {
            return new Dictionary<int, int>();
        }
    }

    private void SaveSessionCart(Dictionary<int, int> cart)
    {
        var json = JsonSerializer.Serialize(cart);
        Session.SetString(CartSessionKey, json);
    }

    public async Task<CartViewModel> GetCartAsync()
    {
        var cartMap = GetSessionCart();
        var model = new CartViewModel();

        if (cartMap.Count == 0)
        {
            return model;
        }

        var productIds = cartMap.Keys.ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Where(p => productIds.Contains(p.Id) && p.IsVisible)
            .ToListAsync();

        foreach (var product in products)
        {
            if (!cartMap.TryGetValue(product.Id, out var qty) || qty <= 0)
                continue;

            var primaryImage = product.Images.FirstOrDefault(i => i.IsPrimary)
                ?? product.Images.FirstOrDefault();

            model.Items.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                OldPrice = product.OldPrice,
                ImageUrl = primaryImage?.ImageUrl ?? "/images/HeroImage.jpg",
                AltText = primaryImage?.AltText ?? product.Name,
                Quantity = qty,
                StockQuantity = product.StockQuantity,
                UnitSize = product.UnitSize,
                PackageType = product.PackageType,
                CategoryName = product.Category?.Name
            });
        }

        return model;
    }

    public async Task<CartActionResult> AddToCartAsync(int productId, int quantity = 1)
    {
        if (quantity <= 0) quantity = 1;

        // Cache lightweight product metadata (Name & StockQuantity) to avoid remote DB round-trip on every add-to-cart
        var cacheKey = $"cart_prod_meta_{productId}";
        if (!_cache.TryGetValue(cacheKey, out (string Name, int StockQuantity) prodMeta))
        {
            var p = await _context.Products
                .AsNoTracking()
                .Where(x => x.Id == productId && x.IsVisible)
                .Select(x => new { x.Name, x.StockQuantity })
                .FirstOrDefaultAsync();

            if (p == null)
            {
                return new CartActionResult
                {
                    Success = false,
                    Message = "المنتج غير موجود."
                };
            }

            prodMeta = (p.Name, p.StockQuantity);
            _cache.Set(cacheKey, prodMeta, TimeSpan.FromMinutes(30));
        }

        var cart = GetSessionCart();
        var currentQty = cart.GetValueOrDefault(productId, 0);
        var newQty = currentQty + quantity;

        if (prodMeta.StockQuantity > 0 && newQty > prodMeta.StockQuantity)
        {
            newQty = prodMeta.StockQuantity;
        }

        cart[productId] = newQty;
        SaveSessionCart(cart);

        var totalCount = cart.Values.Sum();

        return new CartActionResult
        {
            Success = true,
            Message = $"تمت إضافة \"{prodMeta.Name}\" إلى سلة التسوق بنجاح!",
            TotalCount = totalCount
        };
    }

    public async Task<CartActionResult> UpdateQuantityAsync(int productId, int quantity)
    {
        var cart = GetSessionCart();

        if (quantity <= 0)
        {
            cart.Remove(productId);
        }
        else
        {
            var stockQty = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == productId)
                .Select(p => p.StockQuantity)
                .FirstOrDefaultAsync();

            if (stockQty > 0 && quantity > stockQty)
            {
                quantity = stockQty;
            }
            cart[productId] = quantity;
        }

        SaveSessionCart(cart);

        var fullCart = await GetCartAsync();
        var item = fullCart.Items.FirstOrDefault(i => i.ProductId == productId);

        return new CartActionResult
        {
            Success = true,
            Message = "تم تحديث الكمية بنجاح.",
            TotalCount = fullCart.TotalItemCount,
            SubTotal = fullCart.SubTotal,
            Total = fullCart.Total,
            DeliveryFee = fullCart.DeliveryFee,
            LineTotal = item?.LineTotal ?? 0m
        };
    }

    public async Task<CartActionResult> RemoveFromCartAsync(int productId)
    {
        var cart = GetSessionCart();
        cart.Remove(productId);
        SaveSessionCart(cart);

        var fullCart = await GetCartAsync();
        return new CartActionResult
        {
            Success = true,
            Message = "تم حذف المنتج من السلة.",
            TotalCount = fullCart.TotalItemCount,
            SubTotal = fullCart.SubTotal,
            Total = fullCart.Total,
            DeliveryFee = fullCart.DeliveryFee
        };
    }

    public Task ClearCartAsync()
    {
        Session.Remove(CartSessionKey);
        return Task.CompletedTask;
    }

    public Task<int> GetCartItemCountAsync()
    {
        var cart = GetSessionCart();
        return Task.FromResult(cart.Values.Sum());
    }
}

