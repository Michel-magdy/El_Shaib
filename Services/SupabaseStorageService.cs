using El_Shaib.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace El_Shaib.Services;

public class SupabaseStorageService : IStorageService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SupabaseStorageService> _logger;
    private static readonly HttpClient _httpClient = new();

    public SupabaseStorageService(IConfiguration config, IWebHostEnvironment env, ILogger<SupabaseStorageService> logger)
    {
        _config = config;
        _env = env;
        _logger = logger;
    }

    private string? GetConfigValue(params string[] keys)
    {
        foreach (var key in keys)
        {
            var val = _config[key];
            if (!string.IsNullOrWhiteSpace(val))
                return val.Trim();

            val = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrWhiteSpace(val))
                return val.Trim();
        }
        return null;
    }

    public async Task<string> UploadReceiptAsync(IFormFile file, string fileName)
    {
        var url = GetConfigValue(
            "Supabase:Url", 
            "Supabase__Url", 
            "SUPABASE_URL", 
            "Supabase_Url")?.TrimEnd('/');

        var key = GetConfigValue(
            "Supabase:Key", 
            "Supabase__Key", 
            "SUPABASE_KEY", 
            "SUPABASE_SERVICE_ROLE_KEY", 
            "SUPABASE_SECRET_KEY", 
            "Supabase_Key");

        var bucket = GetConfigValue(
            "Supabase:ReceiptBucket", 
            "Supabase__ReceiptBucket", 
            "Supabase:ReceiptsBucket", 
            "Supabase__ReceiptsBucket", 
            "SUPABASE_RECEIPT_BUCKET", 
            "SUPABASE_RECEIPTS_BUCKET") ?? "receipts";

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
        {
            _logger.LogError("Supabase Storage configuration missing on server! Url='{Url}', KeyConfigured={HasKey}. Falling back to local disk.", 
                url ?? "EMPTY", !string.IsNullOrWhiteSpace(key));
            return await UploadLocallyAsync(file, fileName);
        }

        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();
            var contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "image/jpeg" : file.ContentType;

            // Ensure bucket exists in Supabase
            await EnsureBucketExistsAsync(url, key, bucket);

            // Attempt upload to target bucket (e.g. receipts)
            var uploadSuccess = await UploadToObjectStorageAsync(url, key, bucket, fileName, bytes, contentType);
            if (uploadSuccess)
            {
                var publicUrl = $"{url}/storage/v1/object/public/{bucket}/{fileName}";
                _logger.LogInformation("Receipt successfully uploaded to Supabase Storage: {Url}", publicUrl);
                return publicUrl;
            }

            // If receipts bucket failed (e.g. not created yet), try the fallback bucket (e.g. products)
            var fallbackBucket = GetConfigValue("Supabase:Bucket", "Supabase__Bucket", "SUPABASE_BUCKET") ?? "products";
            if (!string.Equals(bucket, fallbackBucket, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Attempting upload to fallback bucket '{FallbackBucket}/receipts/{FileName}'", fallbackBucket, fileName);
                var fallbackSuccess = await UploadToObjectStorageAsync(url, key, fallbackBucket, $"receipts/{fileName}", bytes, contentType);
                if (fallbackSuccess)
                {
                    var fallbackUrl = $"{url}/storage/v1/object/public/{fallbackBucket}/receipts/{fileName}";
                    _logger.LogInformation("Receipt successfully uploaded to Supabase Storage (fallback bucket): {Url}", fallbackUrl);
                    return fallbackUrl;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during Supabase upload: {Message}", ex.Message);
        }

        // Final fallback: save locally so user checkout flow is NEVER blocked
        _logger.LogWarning("Supabase upload failed. Saving receipt to local storage.");
        return await UploadLocallyAsync(file, fileName);
    }

    private async Task EnsureBucketExistsAsync(string url, string key, string bucket)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{url}/storage/v1/bucket")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { id = bucket, name = bucket, @public = true }),
                    Encoding.UTF8,
                    "application/json"
                )
            };
            request.Headers.Add("apikey", key);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Supabase bucket '{Bucket}' created or verified successfully.", bucket);
            }
        }
        catch
        {
            // Bucket may already exist or creation managed via dashboard
        }
    }

    private async Task<bool> UploadToObjectStorageAsync(string url, string key, string bucket, string objectPath, byte[] bytes, string contentType)
    {
        try
        {
            var uploadUrl = $"{url}/storage/v1/object/{bucket}/{objectPath}";
            using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            request.Headers.Add("apikey", key);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
            request.Headers.Add("x-upsert", "true");

            var byteContent = new ByteArrayContent(bytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            request.Content = byteContent;

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Supabase upload to {Url} failed with HTTP status {Status}: {Body}", uploadUrl, (int)response.StatusCode, errorBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed HTTP upload to Supabase: {Message}", ex.Message);
            return false;
        }
    }

    private async Task<string> UploadLocallyAsync(IFormFile file, string fileName)
    {
        var uploadFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "receipts");
        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var filePath = Path.Combine(uploadFolder, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/receipts/{fileName}";
    }
}
