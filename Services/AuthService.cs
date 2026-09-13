using El_Shaib.Interfaces;
using El_Shaib.Models;
using El_Shaib.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace El_Shaib.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Customer> _passwordHasher;

    public AuthService(AppDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<Customer>();
    }

    public async Task<(bool Success, string? Error, Customer? Customer)> RegisterAsync(RegisterViewModel model)
    {
        var cleanEmail = model.Email.Trim().ToLowerInvariant();
        var cleanPhone = model.Phone.Trim();

        var emailExists = await _context.Customers.AnyAsync(c => c.Email.ToLower() == cleanEmail);
        if (emailExists)
        {
            return (false, "البريد الإلكتروني مسجل مسبقاً، يرجى تسجيل الدخول أو استخدام بريد آخر.", null);
        }

        var phoneExists = await _context.Customers.AnyAsync(c => c.Phone == cleanPhone);
        if (phoneExists)
        {
            return (false, "رقم الهاتف مسجل مسبقاً في حساب آخر.", null);
        }

        var customer = new Customer
        {
            FullName = model.FullName.Trim(),
            Email = cleanEmail,
            Phone = cleanPhone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        customer.PasswordHash = _passwordHasher.HashPassword(customer, model.Password);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return (true, null, customer);
    }

    public async Task<(bool Success, string? Error, Customer? Customer)> AuthenticateAsync(LoginViewModel model)
    {
        var input = model.EmailOrPhone.Trim().ToLowerInvariant();

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email.ToLower() == input || c.Phone == model.EmailOrPhone.Trim());

        if (customer == null)
        {
            return (false, "بيانات الدخول غير صحيحة. يرجى التأكد من البريد الإلكتروني أو الهاتف وكلمة المرور.", null);
        }

        if (!customer.IsActive)
        {
            return (false, "تم تعطيل هذا الحساب. يرجى التواصل مع إدارة المتجر.", null);
        }

        if (string.IsNullOrEmpty(customer.PasswordHash))
        {
            return (false, "لم يتم تعيين كلمة مرور لهذا الحساب بعد.", null);
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, model.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return (false, "بيانات الدخول غير صحيحة. يرجى التأكد من كلمة المرور.", null);
        }

        customer.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, null, customer);
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return await _context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(int customerId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}

