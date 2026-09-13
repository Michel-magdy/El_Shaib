using System.ComponentModel.DataAnnotations;
using El_Shaib.Models;

namespace El_Shaib.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "يرجى إدخال رقم الهاتف")]
    [Display(Name = "رقم الهاتف")]
    public string Phone { get; set; } = string.Empty;

    public string EmailOrPhone
    {
        get => Phone;
        set => Phone = value;
    }

    [Required(ErrorMessage = "يرجى إدخال كلمة المرور")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "تذكرني على هذا الجهاز")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "يرجى إدخال الاسم بالكامل")]
    [MaxLength(100)]
    [Display(Name = "الاسم بالكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال رقم الهاتف")]
    [MaxLength(20)]
    [Display(Name = "رقم الهاتف")]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(150)]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    [Display(Name = "البريد الإلكتروني (اختياري)")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "يرجى إدخال كلمة المرور")]
    [StringLength(100, ErrorMessage = "يجب أن تكون كلمة المرور 6 أحرف على الأقل", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "تأكيد كلمة المرور")]
    [Compare("Password", ErrorMessage = "كلمة المرور وتأكيد كلمة المرور غير متطابقين.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class CustomerProfileViewModel
{
    public Customer Customer { get; set; } = null!;
    public List<Order> Orders { get; set; } = new();
}

