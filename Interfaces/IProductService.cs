using El_Shaib.Models;

namespace El_Shaib.Interfaces;

public interface IProductService : IGenericService<Product>
{
    Task<List<Product>> GetProducts(int pageNumber, int pageSize);
}
