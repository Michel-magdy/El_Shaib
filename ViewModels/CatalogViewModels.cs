using El_Shaib.Models;

namespace El_Shaib.ViewModels;

public class ProductFilterViewModel
{
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public string? SortBy { get; set; } = "popular";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 8;
}

public class ProductListViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 8;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;
    public string? SelectedSort { get; set; } = "popular";
    public int? SelectedCategoryId { get; set; }
    public string? Query { get; set; }

    public Category? CurrentCategory =>
        SelectedCategoryId.HasValue ? Categories.FirstOrDefault(c => c.Id == SelectedCategoryId.Value) : null;
}

