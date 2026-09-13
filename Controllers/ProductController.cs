using El_Shaib.Interfaces;
using El_Shaib.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace El_Shaib.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: /Product
        public async Task<IActionResult> Index(ProductFilterViewModel filter)
        {
            var model = await _productService.GetFilteredProductsAsync(filter);
            return View(model);
        }

        // GET: /Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductDetailsAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}
