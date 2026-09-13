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

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Query.ContainsKey("isAjax"))
            {
                return PartialView("_ProductGridPartial", model);
            }

            return View(model);
        }

        // GET: /Product/Filter
        [HttpGet]
        public async Task<IActionResult> Filter(ProductFilterViewModel filter)
        {
            var model = await _productService.GetFilteredProductsAsync(filter);
            return PartialView("_ProductGridPartial", model);
        }

        // GET: /Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductDetailsAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.RelatedProducts = await _productService.GetRelatedProductsAsync(product.CategoryId, product.Id, 4);

            return View(product);
        }
    }
}
