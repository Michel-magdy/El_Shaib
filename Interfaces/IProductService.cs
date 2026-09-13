using El_Shaib.Models;
using El_Shaib.ViewModels;

namespace El_Shaib.Interfaces;

public interface IProductService : IGenericService<Product>
{
    Task<List<Product>> GetProductsAsync(int pageNumber, int pageSize);
    Task<Product?> GetProductDetailsAsync(int id);
    Task<ProductListViewModel> GetFilteredProductsAsync(ProductFilterViewModel filter);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Product>> GetRelatedProductsAsync(int categoryId, int currentProductId, int count = 4);
}
