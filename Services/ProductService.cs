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
                .Where(product => product.IsVisible)
                .Include(product => product.Images)
                .Include(product => product.Category)
                .ToListAsync();
        }) ?? new List<Product>();
    }

    public async Task<Product?> GetProductDetailsAsync(int id)
    {
        return await entity
            .AsNoTracking()
            .Where(p => p.IsVisible)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetProductsAsync(int pageNumber, int pageSize)
    {
        return await entity
            .AsNoTracking()
            .Where(product => product.IsVisible)
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
        var page = Math.Max(1, filter.Page);
        const int pageSize = 8;
        var queryTerm = filter.Query?.Trim();
        var categoryId = (filter.CategoryId.HasValue && filter.CategoryId.Value > 0) ? filter.CategoryId.Value : (int?)null;
        var sortBy = string.IsNullOrWhiteSpace(filter.SortBy) ? "popular" : filter.SortBy.Trim();

        // Ensure filter object reflects normalized parameters
        filter.Page = page;
        filter.PageSize = pageSize;
        filter.CategoryId = categoryId;
        filter.SortBy = sortBy;

        var cacheKey = $"filtered_{queryTerm}_{categoryId}_{sortBy}_{page}_{pageSize}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

            var query = context.Products
                .AsNoTracking()
                .Where(p => p.IsVisible)
                .Include(p => p.Images)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(queryTerm))
            {
                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, $"%{queryTerm}%") ||
                    (p.Description != null && EF.Functions.ILike(p.Description, $"%{queryTerm}%")) ||
                    (p.PackageType != null && EF.Functions.ILike(p.PackageType, $"%{queryTerm}%")) ||
                    (p.UnitSize != null && EF.Functions.ILike(p.UnitSize, $"%{queryTerm}%")));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.Id)
            };

            var totalCount = await query.CountAsync();

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
                SelectedCategoryId = categoryId,
                SelectedSort = sortBy,
                Query = queryTerm
            };
        }) ?? new ProductListViewModel();
    }

    public async Task<List<Product>> GetRelatedProductsAsync(int categoryId, int currentProductId, int count = 4)
    {
        var query = context.Products
            .AsNoTracking()
            .Where(p => p.IsVisible && p.Id != currentProductId)
            .Include(p => p.Images)
            .Include(p => p.Category);

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
