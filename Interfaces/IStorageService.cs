using Microsoft.AspNetCore.Http;

namespace El_Shaib.Interfaces;

public interface IStorageService
{
    Task<string> UploadReceiptAsync(IFormFile file, string fileName);
}

