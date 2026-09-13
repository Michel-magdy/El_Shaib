using El_Shaib.Models;
using El_Shaib.ViewModels;

namespace El_Shaib.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string? Error, Customer? Customer)> RegisterAsync(RegisterViewModel model);
    Task<(bool Success, string? Error, Customer? Customer)> AuthenticateAsync(LoginViewModel model);
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<List<Order>> GetCustomerOrdersAsync(int customerId);
}

