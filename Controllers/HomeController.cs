using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using El_Shaib.Models;
using El_Shaib.Interfaces;

namespace El_Shaib.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly AppDbContext _context;

    public HomeController(IProductService productService, AppDbContext context)
    {
        _productService = productService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetProductsAsync(1,4);
        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ContactUs()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ContactUs(ContactMessage model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.CreatedAt = DateTime.UtcNow;
        model.IsRead = false;
        _context.ContactMessages.Add(model);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "شكراً لتواصلك معنا! تم إرسال رسالتك بنجاح وسيقوم فريق مزارع الشايب بالرد عليك في أقرب وقت.";
        return RedirectToAction(nameof(ContactUs));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet("/check-storage")]
    public async Task<IActionResult> CheckStorage([FromServices] IConfiguration config)
    {
        var url = (config["Supabase:Url"] 
                ?? config["Supabase__Url"] 
                ?? config["SUPABASE_URL"] 
                ?? Environment.GetEnvironmentVariable("SUPABASE_URL") 
                ?? Environment.GetEnvironmentVariable("Supabase__Url") 
                ?? Environment.GetEnvironmentVariable("Supabase:Url"))?.TrimEnd('/');

        var rawKey = config["Supabase:Key"] 
                  ?? config["Supabase__Key"] 
                  ?? config["SUPABASE_KEY"] 
                  ?? config["SUPABASE_SERVICE_ROLE_KEY"] 
                  ?? config["SUPABASE_SECRET_KEY"] 
                  ?? config["Supabase_Key"]
                  ?? Environment.GetEnvironmentVariable("SUPABASE_KEY")
                  ?? Environment.GetEnvironmentVariable("SUPABASE_SERVICE_ROLE_KEY")
                  ?? Environment.GetEnvironmentVariable("Supabase__Key");

        var bucket = config["Supabase:ReceiptBucket"] 
                  ?? config["Supabase__ReceiptBucket"] 
                  ?? config["SUPABASE_RECEIPT_BUCKET"] 
                  ?? "receipts";

        var key = rawKey?.Trim().Trim('\"', '\'');

        string detectedRole = "unknown";
        if (!string.IsNullOrEmpty(key) && key.Contains('.'))
        {
            try
            {
                var parts = key.Split('.');
                if (parts.Length >= 2)
                {
                    var payload = parts[1];
                    switch (payload.Length % 4)
                    {
                        case 2: payload += "=="; break;
                        case 3: payload += "="; break;
                    }
                    var bytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
                    var json = System.Text.Encoding.UTF8.GetString(bytes);
                    if (json.Contains("\"service_role\"")) detectedRole = "service_role (CORRECT)";
                    else if (json.Contains("\"anon\"")) detectedRole = "anon (WARNING: This is the public anon key! Must use service_role key!)";
                }
            }
            catch { }
        }

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
        {
            return Json(new
            {
                Success = false,
                Error = "Supabase Url or Key is missing from configuration and environment variables!",
                Url = url ?? "MISSING",
                KeyDetected = !string.IsNullOrWhiteSpace(key),
                KeyRole = detectedRole,
                Bucket = bucket,
                Recommendation = "Please add SUPABASE_URL and SUPABASE_KEY (service_role) in Railway Variables."
            });
        }

        try
        {
            using var client = new HttpClient();
            var testUploadUrl = $"{url}/storage/v1/object/{bucket}/diag_test.txt";
            using var req = new HttpRequestMessage(HttpMethod.Post, testUploadUrl);
            req.Headers.Add("apikey", key);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);
            req.Headers.Add("x-upsert", "true");
            req.Content = new StringContent("Storage diagnostic test from server", System.Text.Encoding.UTF8, "text/plain");

            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            return Json(new
            {
                Success = resp.IsSuccessStatusCode,
                StatusCode = (int)resp.StatusCode,
                StatusMessage = resp.StatusCode.ToString(),
                ResponseBody = body,
                Url = url,
                KeyDetected = true,
                KeyPrefix = key.Length > 10 ? key.Substring(0, 10) + "..." : "too short",
                KeyRole = detectedRole,
                Bucket = bucket,
                PublicUrl = $"{url}/storage/v1/object/public/{bucket}/diag_test.txt",
                Diagnosis = resp.IsSuccessStatusCode 
                    ? "Supabase Storage connection is WORKING PERFECTLY! Receipts will be uploaded directly to Supabase." 
                    : detectedRole.Contains("anon") 
                        ? "FAILED: You are using the 'anon' key in Railway. You MUST replace SUPABASE_KEY with the 'service_role' key from Supabase Dashboard -> Project Settings -> API."
                        : "FAILED: Check the StatusCode and ResponseBody above to see why Supabase rejected the upload."
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                Success = false,
                Error = ex.Message,
                Url = url,
                KeyRole = detectedRole,
                Bucket = bucket
            });
        }
    }
}
