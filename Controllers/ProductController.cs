using El_Shaib.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace El_Shaib.Controllers
{
    public class ProductController : Controller
    {
        IProductService productService;

        public ProductController(IProductService productService)
        {
            this.productService = productService;
        }

        // GET: ProductController
        public async Task<ActionResult> Index()
        {
            var product = await productService.GetAllAsync();
            return View(product);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await productService.GetProductDetailsAsync(id);
            return View(product);
        }


    }
}
