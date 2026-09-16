using System.IO.Compression;
using El_Shaib.Interfaces;
using El_Shaib.Models;
using El_Shaib.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Railway/cloud: listen on the PORT env var
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Performance: Response Compression (Brotli & Gzip)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "image/svg+xml",
        "application/javascript",
        "text/css",
        "text/html",
        "application/json"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Performance: In-Memory Caching for catalog and areas
builder.Services.AddMemoryCache();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStorageService, SupabaseStorageService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionString"))
);

var app = builder.Build();

// Automatically apply database migrations if possible
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        // Enforce single Admin account in database
        var adminEmail = (app.Configuration["AdminSettings:Email"] 
            ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") 
            ?? "admin@elshaib.com").Trim().ToLowerInvariant();

        var adminUser = db.Customers.FirstOrDefault(c => c.Email.ToLower() == adminEmail);
        if (adminUser != null)
        {
            if (adminUser.Role != UserRole.Admin)
            {
                adminUser.Role = UserRole.Admin;
                db.SaveChanges();
            }
        }
        else
        {
            var newAdmin = new Customer
            {
                FullName = "مدير النظام",
                Email = adminEmail,
                Phone = "01000000000",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Customers.Add(newAdmin);
            db.SaveChanges();
        }

        // Ensure ONLY this single account has Admin role in the project
        var otherAdmins = db.Customers.Where(c => c.Role == UserRole.Admin && c.Email.ToLower() != adminEmail).ToList();
        if (otherAdmins.Count > 0)
        {
            foreach (var other in otherAdmins)
            {
                other.Role = UserRole.Customer;
            }
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not apply database migrations or seed admin on startup: {Message}", ex.Message);
    }
}

// Performance: Enable Response Compression early in pipeline
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

// Support reverse proxy headers (Railway, Docker, etc.)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
        | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// Performance: Static file caching headers (7 days)
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name.ToLowerInvariant();
        if (path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".jpg") || 
            path.EndsWith(".jpeg") || path.EndsWith(".png") || path.EndsWith(".webp") || 
            path.EndsWith(".svg") || path.EndsWith(".woff2"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800,must-revalidate");
        }
    }
});

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
