using El_Shaib.Interfaces;
using El_Shaib.Models;
using El_Shaib.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace El_Shaib.Services;

public class ProductService : GenericService<Product>, IProductService
{
    private readonly IMemoryCache _cache;

    public ProductService(AppDbContext context, IMemoryCache cache) : base(context)
    {
        _cache = cache;
    }

    public override async Task<List<Product>> GetAllAsync()
    {
        return await _cache.GetOrCreateAsync("all_featured_products", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await entity
                .AsNoTracking()
                .Include(product => product.Images)
                .Include(product => product.Category)
                .ToListAsync();
        }) ?? new List<Product>();
    }

    public async Task<Product?> GetProductDetailsAsync(int id)
    {
        return await entity
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetProductsAsync(int pageNumber, int pageSize)
    {
        return await entity
            .AsNoTracking()
            .Include(product => product.Images)
            .Include(product => product.Category)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _cache.GetOrCreateAsync("active_categories", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            return await context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }) ?? new List<Category>();
    }

    public async Task<ProductListViewModel> GetFilteredProductsAsync(ProductFilterViewModel filter)
    {
        var query = context.Products
            .AsNoTracking()
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

    public async Task<List<Product>> GetRelatedProductsAsync(int categoryId, int currentProductId, int count = 4)
    {
        var query = context.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Where(p => p.Id != currentProductId);

        var related = await query
            .Where(p => p.CategoryId == categoryId)
            .Take(count)
            .ToListAsync();

        if (related.Count < count)
        {
            var needed = count - related.Count;
            var relatedIds = related.Select(r => r.Id).Append(currentProductId).ToList();
            var extra = await query
                .Where(p => !relatedIds.Contains(p.Id))
                .Take(needed)
                .ToListAsync();
            related.AddRange(extra);
        }

        return related;
    }
}
