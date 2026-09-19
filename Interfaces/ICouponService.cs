using El_Shaib.Models;

namespace El_Shaib.Interfaces;

public interface ICouponService
{
    Task<(bool IsValid, string? ErrorMessage, Coupon? Coupon, decimal DiscountAmount)> ValidateCouponAsync(string? code, decimal subtotal);
    Task<(bool Success, string Message, decimal DiscountAmount, string? Code)> ApplyCouponAsync(string code, decimal subtotal);
    Task<string?> GetAppliedCouponCodeAsync();
    Task RemoveCouponAsync();
    Task<(Coupon? Coupon, decimal DiscountAmount)> GetCurrentCouponDiscountAsync(decimal subtotal);
    Task RecordCouponUsageAsync(int couponId);
}

