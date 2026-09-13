using El_Shaib.Models;

namespace El_Shaib.Interfaces;

public interface IWishlistService
{
    Task<bool> ToggleWishlistAsync(int productId);
    Task<List<Product>> GetWishlistProductsAsync();
    Task<int> GetWishlistCountAsync();
    Task<bool> IsInWishlistAsync(int productId);
}

