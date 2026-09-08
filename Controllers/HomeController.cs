using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using El_Shaib.Models;
using El_Shaib.Interfaces;

namespace El_Shaib.Controllers;

public class HomeController : Controller
{
    IProductService productService;

    public HomeController(IProductService productService)
    {
        this.productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var Products = await productService.GetAllAsync();
        return View(Products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
