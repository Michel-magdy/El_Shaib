using El_Shaib.Interfaces;
using El_Shaib.Models;
using El_Shaib.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace El_Shaib.Services;

public class ProductService : GenericService<Product>, IProductService
{
    public ProductService(AppDbContext context) : base(context)
    {
    }

    public override async Task<List<Product>> GetAllAsync()
    {
        return await entity
            .Include(product => product.Images)
            .Include(product => product.Category)
            .ToListAsync();
    }

    public async Task<Product?> GetProductDetailsAsync(int id)
    {
        return await entity
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetProductsAsync(int pageNumber, int pageSize)
    {
        return await entity
            .Include(product => product.Images)
            .Include(product => product.Category)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<ProductListViewModel> GetFilteredProductsAsync(ProductFilterViewModel filter)
    {
        var query = context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var term = filter.Query.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{term}%") ||
                (p.Description != null && EF.Functions.ILike(p.Description, $"%{term}%")) ||
                (p.PackageType != null && EF.Functions.ILike(p.PackageType, $"%{term}%")) ||
                (p.UnitSize != null && EF.Functions.ILike(p.UnitSize, $"%{term}%")));
        }

        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        query = filter.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.Id)
        };

        var totalCount = await query.CountAsync();
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var categories = await GetCategoriesAsync();

        return new ProductListViewModel
        {
            Products = products,
            Categories = categories,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize,
            SelectedCategoryId = filter.CategoryId,
            SelectedSort = filter.SortBy ?? "popular",
            Query = filter.Query
        };
    }
}
