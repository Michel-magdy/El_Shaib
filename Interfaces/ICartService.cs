using El_Shaib.ViewModels;

namespace El_Shaib.Interfaces;

public interface ICartService
{
    Task<CartViewModel> GetCartAsync();
    Task<CartActionResult> AddToCartAsync(int productId, int quantity = 1);
    Task<CartActionResult> UpdateQuantityAsync(int productId, int quantity);
    Task<CartActionResult> RemoveFromCartAsync(int productId);
    Task ClearCartAsync();
    Task<int> GetCartItemCountAsync();
}

