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

    public static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        var sb = new System.Text.StringBuilder();
        foreach (var ch in phone.Trim())
        {
            if (ch >= '٠' && ch <= '٩')
                sb.Append((char)('0' + (ch - '٠')));
            else if (char.IsDigit(ch) || ch == '+')
                sb.Append(ch);
        }

        var cleaned = sb.ToString();

        // Standardize Egyptian mobile numbers
        if (cleaned.StartsWith("+20"))
            cleaned = "0" + cleaned.Substring(3);
        else if (cleaned.StartsWith("0020"))
            cleaned = "0" + cleaned.Substring(4);
        else if (cleaned.StartsWith("20") && cleaned.Length == 12)
            cleaned = "0" + cleaned.Substring(2);
        else if (cleaned.Length == 10 && (cleaned.StartsWith("10") || cleaned.StartsWith("11") || cleaned.StartsWith("12") || cleaned.StartsWith("15")))
            cleaned = "0" + cleaned;

        return cleaned;
    }

    public async Task<(bool Success, string? Error, Customer? Customer)> RegisterAsync(RegisterViewModel model)
    {
        var cleanPhone = NormalizePhone(model.Phone);
        if (string.IsNullOrWhiteSpace(cleanPhone) || cleanPhone.Length < 8)
        {
            return (false, "يرجى إدخال رقم هاتف صحيح.", null);
        }

        var phoneExists = await _context.Customers.AnyAsync(c => c.Phone == cleanPhone || c.Phone == model.Phone.Trim());
        if (phoneExists)
        {
            return (false, "رقم الهاتف مسجل مسبقاً، يرجى تسجيل الدخول مباشرة.", null);
        }

        // Generate faked or normalized email
        string cleanEmail;
        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            cleanEmail = model.Email.Trim().ToLowerInvariant();
            var emailExists = await _context.Customers.AnyAsync(c => c.Email.ToLower() == cleanEmail);
            if (emailExists)
            {
                return (false, "البريد الإلكتروني مسجل مسبقاً، يرجى استخدام بريد آخر أو تركه فارغاً.", null);
            }
        }
        else
        {
            // Auto-fake email based on normalized phone
            var digitsOnly = new string(cleanPhone.Where(char.IsDigit).ToArray());
            cleanEmail = $"{digitsOnly}@customer.elshaib.local";

            // If the faked email already exists for any reason, ensure uniqueness
            var emailExists = await _context.Customers.AnyAsync(c => c.Email.ToLower() == cleanEmail);
            if (emailExists)
            {
                cleanEmail = $"{digitsOnly}_{Guid.NewGuid():N}@customer.elshaib.local";
            }
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
        var rawInput = (model.Phone ?? model.EmailOrPhone ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return (false, "يرجى إدخال رقم الهاتف.", null);
        }

        var normalizedPhone = NormalizePhone(rawInput);
        var lowerInput = rawInput.ToLowerInvariant();

        // Customer can log in with normalized phone, raw phone, or email
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Phone == normalizedPhone 
                                   || c.Phone == rawInput 
                                   || c.Email.ToLower() == lowerInput);

        if (customer == null)
        {
            return (false, "رقم الهاتف أو كلمة المرور غير صحيحة.", null);
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
            return (false, "رقم الهاتف أو كلمة المرور غير صحيحة.", null);
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
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}

