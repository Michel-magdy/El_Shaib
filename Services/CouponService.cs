using El_Shaib.Interfaces;
using El_Shaib.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace El_Shaib.Services;

public class CouponService : ICouponService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CouponSessionKey = "ELSHAIB_APPLIED_COUPON";

    public CouponService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession? Session => _httpContextAccessor.HttpContext?.Session;

    public async Task<(bool IsValid, string? ErrorMessage, Coupon? Coupon, decimal DiscountAmount)> ValidateCouponAsync(string? code, decimal subtotal)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return (false, "يرجى إدخال كود الخصم.", null, 0m);
        }

        var normalizedCode = code.Trim().ToUpperInvariant();

        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToUpper() == normalizedCode);

        if (coupon == null)
        {
            return (false, "كود الخصم غير صحيح أو غير موجود.", null, 0m);
        }

        if (!coupon.IsActive)
        {
            return (false, "كوبون الخصم غير مفعل حالياً.", null, 0m);
        }

        var now = DateTime.UtcNow;

        if (now < coupon.StartDate)
        {
            return (false, $"هذا الكوبون سيبدأ تفعيله بتاريخ {coupon.StartDate:yyyy/MM/dd}.", null, 0m);
        }

        if (now > coupon.EndDate)
        {
            return (false, "كوبون الخصم منتهي الصلاحية.", null, 0m);
        }

        if (coupon.UsageLimit.HasValue && coupon.UsageCount >= coupon.UsageLimit.Value)
        {
            return (false, "لقد استنفد هذا الكوبون الحد الأقصى المسموح لعدد مرات الاستخدام.", null, 0m);
        }

        if (coupon.MinOrderAmount.HasValue && subtotal < coupon.MinOrderAmount.Value)
        {
            return (false, $"الحد الأدنى لقيمة الطلب لتطبيق هذا الكوبون هو {coupon.MinOrderAmount.Value:0.00} ج.م.", null, 0m);
        }

        // Calculate discount
        decimal discount = 0m;
        if (coupon.DiscountType == DiscountType.Percentage)
        {
            discount = Math.Round(subtotal * (coupon.DiscountValue / 100m), 2);
            if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
            {
                discount = coupon.MaxDiscountAmount.Value;
            }
        }
        else
        {
            discount = Math.Min(coupon.DiscountValue, subtotal);
        }

        return (true, null, coupon, discount);
    }

    public async Task<(bool Success, string Message, decimal DiscountAmount, string? Code)> ApplyCouponAsync(string code, decimal subtotal)
    {
        var (isValid, error, coupon, discount) = await ValidateCouponAsync(code, subtotal);
        if (!isValid || coupon == null)
        {
            return (false, error ?? "فشل تطبيق كود الخصم.", 0m, null);
        }

        if (Session != null)
        {
            Session.SetString(CouponSessionKey, coupon.Code);
        }

        string successMsg = coupon.DiscountType == DiscountType.Percentage
            ? $"تم تطبيق خصم {coupon.DiscountValue:0.#}% بنجاح!"
            : $"تم تطبيق خصم بقيمة {coupon.DiscountValue:0.00} ج.م بنجاح!";

        return (true, successMsg, discount, coupon.Code);
    }

    public Task<string?> GetAppliedCouponCodeAsync()
    {
        var code = Session?.GetString(CouponSessionKey);
        return Task.FromResult(code);
    }

    public Task RemoveCouponAsync()
    {
        Session?.Remove(CouponSessionKey);
        return Task.CompletedTask;
    }

    public async Task<(Coupon? Coupon, decimal DiscountAmount)> GetCurrentCouponDiscountAsync(decimal subtotal)
    {
        var code = await GetAppliedCouponCodeAsync();
        if (string.IsNullOrWhiteSpace(code))
        {
            return (null, 0m);
        }

        var (isValid, _, coupon, discount) = await ValidateCouponAsync(code, subtotal);
        if (!isValid || coupon == null)
        {
            // If condition no longer met (e.g. subtotal dropped below min spend), remove from session
            await RemoveCouponAsync();
            return (null, 0m);
        }

        return (coupon, discount);
    }

    public async Task RecordCouponUsageAsync(int couponId)
    {
        var coupon = await _context.Coupons.FindAsync(couponId);
        if (coupon != null)
        {
            coupon.UsageCount += 1;
            await _context.SaveChangesAsync();
        }
    }
}

