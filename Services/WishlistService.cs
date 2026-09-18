using System.Text.Json;
using El_Shaib.Interfaces;
using El_Shaib.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace El_Shaib.Services;

public class WishlistService : IWishlistService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;
    private const string WishlistSessionKey = "ELSHAIB_WISHLIST_SESSION";

    public WishlistService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    private ISession Session => _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("Session is not available in the current context.");

    private HashSet<int> GetSessionWishlist()
    {
        var json = Session.GetString(WishlistSessionKey);
        if (string.IsNullOrEmpty(json))
        {
            return new HashSet<int>();
        }

        try
        {
            return JsonSerializer.Deserialize<HashSet<int>>(json) ?? new HashSet<int>();
        }
        catch
        {
            return new HashSet<int>();
        }
    }

    private void SaveSessionWishlist(HashSet<int> wishlist)
    {
        var json = JsonSerializer.Serialize(wishlist);
        Session.SetString(WishlistSessionKey, json);
    }

    public Task<bool> IsInWishlistAsync(int productId)
    {
        var set = GetSessionWishlist();
        return Task.FromResult(set.Contains(productId));
    }

    public async Task<bool> ToggleWishlistAsync(int productId)
    {
        var set = GetSessionWishlist();
        bool added;

        if (set.Contains(productId))
        {
            set.Remove(productId);
            added = false;
        }
        else
        {
            set.Add(productId);
            added = true;
        }

        SaveSessionWishlist(set);
        return await Task.FromResult(added);
    }

    public async Task<List<Product>> GetWishlistProductsAsync()
    {
        var set = GetSessionWishlist();
        if (set.Count == 0)
        {
            return new List<Product>();
        }

        return await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Where(p => set.Contains(p.Id) && p.IsVisible)
            .ToListAsync();
    }

    public Task<int> GetWishlistCountAsync()
    {
        var set = GetSessionWishlist();
        return Task.FromResult(set.Count);
    }
}

