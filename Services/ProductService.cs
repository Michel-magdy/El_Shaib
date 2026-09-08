using System;
using El_Shaib.Interfaces;
using El_Shaib.Models;

namespace El_Shaib.Services;

public class ProductService : GenericService<Product>, IProductService
{
    public ProductService(AppDbContext _context) : base(_context)
    {
    }

    async Task<List<Product>> IProductService.GetProducts(int pageNumber, int pageSize)
    {
        return entity.Skip((pageNumber - 1)).Take(pageSize).ToList();
    }
}
