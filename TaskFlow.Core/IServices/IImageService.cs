using Microsoft.AspNetCore.Http;

namespace TaskFlow.Core.IServices
{
    public interface IImageService
    {
        Task<string> UploadImageOnServer(IFormFile image, bool deleteIfExist = false, string oldPath = null!, CancellationToken cancellationToken = default);
        Task RemoveImage(string oldPath);
    }
}
