using El_Shaib.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Supabase.Storage;
using System.IO;

namespace El_Shaib.Services;

public class SupabaseStorageService : IStorageService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SupabaseStorageService> _logger;
    private Supabase.Client? _supabaseClient;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public SupabaseStorageService(IConfiguration config, IWebHostEnvironment env, ILogger<SupabaseStorageService> logger)
    {
        _config = config;
        _env = env;
        _logger = logger;
    }

    private async Task<Supabase.Client?> GetClientAsync()
    {
        if (_supabaseClient != null) return _supabaseClient;

        await _initLock.WaitAsync();
        try
        {
            if (_supabaseClient != null) return _supabaseClient;

            var url = _config["Supabase:Url"];
            var key = _config["Supabase:Key"];

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Supabase Url or Key is missing. Falling back to local storage.");
                return null;
            }

            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = false
            };

            var client = new Supabase.Client(url, key, options);
            await client.InitializeAsync();
            _supabaseClient = client;
            return _supabaseClient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Supabase client: {Message}", ex.Message);
            return null;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<string> UploadReceiptAsync(IFormFile file, string fileName)
    {
        var bucket = _config["Supabase:ReceiptBucket"] ?? _config["Supabase:ReceiptsBucket"] ?? "receipts";

        try
        {
            var client = await GetClientAsync();
            if (client != null)
            {
                // Ensure bucket exists or attempt creation (service role allows this)
                try
                {
                    await client.Storage.CreateBucket(bucket, new BucketUpsertOptions { Public = true });
                }
                catch
                {
                    // Bucket may already exist or creation is managed via dashboard
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                var fileOptions = new Supabase.Storage.FileOptions
                {
                    ContentType = file.ContentType,
                    Upsert = true
                };

                await client.Storage.From(bucket).Upload(bytes, fileName, fileOptions);

                var publicUrl = client.Storage.From(bucket).GetPublicUrl(fileName);
                _logger.LogInformation("Receipt successfully uploaded to Supabase Storage: {Url}", publicUrl);
                return publicUrl;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload receipt to Supabase Storage. Falling back to local disk: {Message}", ex.Message);
        }

        // Fallback to local storage so checkout is never interrupted
        return await UploadLocallyAsync(file, fileName);
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
