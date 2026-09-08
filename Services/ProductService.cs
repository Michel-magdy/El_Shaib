using El_Shaib.Interfaces;
using El_Shaib.Models;
using Microsoft.EntityFrameworkCore;

namespace El_Shaib.Services;

public class ProductService : GenericService<Product>, IProductService
{
    public ProductService(AppDbContext _context) : base(_context)
    {
    }

    public override async Task<List<Product>> GetAllAsync()
    {
        return await entity
            .Include(product => product.Images)
            .ToListAsync();
    }


    public async Task<Product?> GetProductDetailsAsync(int id)
    {
        return entity.Include(p => p.Category).Include(p => p.Images).FirstOrDefault(p => p.Id == id);
    }

    async Task<List<Product>> IProductService.GetProductsAsync(int pageNumber, int pageSize)
    {
        return await entity
            .Include(product => product.Images)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
