using TaskFlow.Core.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TaskFlow.Business.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageService> _logger;
        private readonly string _basePath;

        public ImageService(IWebHostEnvironment environment, ILogger<ImageService> logger)
        {
            _environment = environment;
            _basePath = _environment.WebRootPath;
            _logger = logger;
        }

        public async Task<string> UploadImageOnServer(
            IFormFile image,
            bool deleteIfExist = false,
            string oldPath = null!,
            CancellationToken cancellationToken = default)
        {
            var folderPath = Path.Combine(_basePath, "images");
            Directory.CreateDirectory(folderPath);

            if (deleteIfExist && oldPath is not null)
                RemoveImage(oldPath); // No need to await, now it's sync

            string uniqueFileName = $"{Guid.NewGuid()}_{image.FileName}";
            var fullPath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream, cancellationToken);
            }

            return $"images/{uniqueFileName}";
        }

        public void RemoveImage(string oldPath)
        {
            if (string.IsNullOrWhiteSpace(oldPath))
                return;

            string normalizedPath = oldPath.TrimStart('/').Replace("/", "\\");

            string imagePath = oldPath.StartsWith(_basePath)
                ? oldPath
                : Path.Combine(_basePath, normalizedPath);

            if (File.Exists(imagePath))
            {
                try
                {
                    File.SetAttributes(imagePath, FileAttributes.Normal);
                    File.Delete(imagePath);

                    _logger.LogInformation($"Deleted: {imagePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting file: {imagePath}");
                    throw;
                }
            }
            else
            {
                _logger.LogInformation($"File not found: {imagePath}");
            }
        }
    }

}
