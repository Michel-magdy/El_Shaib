using System.Security.Claims;
using El_Shaib.Interfaces;
using El_Shaib.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace El_Shaib.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;

    public AccountController(IAuthService authService, IConfiguration config)
    {
        _authService = authService;
        _config = config;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error, customer) = await _authService.AuthenticateAsync(model);
        if (!success || customer == null)
        {
            ModelState.AddModelError("", error ?? "فشل تسجيل الدخول. يرجى مراجعة البيانات.");
            return View(model);
        }

        var adminEmail = (_config["AdminSettings:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@elshaib.com").Trim().ToLowerInvariant();
        if (customer.Role == Models.UserRole.Admin || string.Equals(customer.Email, adminEmail, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("", "حساب الإدارة مخصص للاستخدام عبر تطبيق الهاتف فقط، لا يمكن تسجيل الدخول به عبر الموقع.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new(ClaimTypes.Name, customer.FullName),
            new(ClaimTypes.Email, customer.Email),
            new(ClaimTypes.MobilePhone, customer.Phone),
            new(ClaimTypes.Role, customer.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error, customer) = await _authService.RegisterAsync(model);
        if (!success || customer == null)
        {
            ModelState.AddModelError("", error ?? "فشل إنشاء الحساب.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new(ClaimTypes.Name, customer.FullName),
            new(ClaimTypes.Email, customer.Email),
            new(ClaimTypes.MobilePhone, customer.Phone),
            new(ClaimTypes.Role, customer.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    // GET or POST: /Account/Logout
    [Authorize]
    [HttpGet, HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Orders
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Orders()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var customerId))
        {
            return RedirectToAction("Login");
        }

        var orders = await _authService.GetCustomerOrdersAsync(customerId);
        return View(orders);
    }
}

