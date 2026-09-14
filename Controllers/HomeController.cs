using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using El_Shaib.Models;
using El_Shaib.Interfaces;

namespace El_Shaib.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly AppDbContext _context;

    public HomeController(IProductService productService, AppDbContext context)
    {
        _productService = productService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetProductsAsync(1,4);
        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ContactUs()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ContactUs(ContactMessage model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.CreatedAt = DateTime.UtcNow;
        model.IsRead = false;
        _context.ContactMessages.Add(model);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "شكراً لتواصلك معنا! تم إرسال رسالتك بنجاح وسيقوم فريق مزارع الشايب بالرد عليك في أقرب وقت.";
        return RedirectToAction(nameof(ContactUs));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
