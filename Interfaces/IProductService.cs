using El_Shaib.Models;

namespace El_Shaib.Interfaces;

public interface IProductService : IGenericService<Product>
{
    Task<List<Product>> GetProductsAsync(int pageNumber, int pageSize);
    Task<Product?> GetProductDetailsAsync(int id);
}
