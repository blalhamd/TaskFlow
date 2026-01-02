using Microsoft.AspNetCore.Http;

namespace TaskFlow.Core.IServices
{
    public interface IImageService
    {
        Task<string> UploadImageOnServer(IFormFile image, bool deleteIfExist = false, string oldPath = null!, CancellationToken cancellationToken = default);
        void RemoveImage(string oldPath);
    }
}
