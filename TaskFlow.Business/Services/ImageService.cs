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
            _basePath = Path.Combine(_environment.WebRootPath);
            _logger = logger;
        }


        public async Task<string> UploadImageOnServer(IFormFile image, bool deleteIfExist = false, string oldPath = null, CancellationToken cancellationToken = default)
        {

            var folderPath = Path.Combine(_basePath, "assets", "images");
            Directory.CreateDirectory(folderPath); // Ensure the folder exists

            if (deleteIfExist && oldPath is not null)
            {
                await RemoveImage($"{oldPath}");
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;

            var fullPath = Path.Combine(folderPath, uniqueFileName);

            using var stream = new FileStream(fullPath, FileMode.OpenOrCreate);
            await image.CopyToAsync(stream, cancellationToken); // will put uploaded file in this path in wwwroot
            stream.Close();

            return $"assets/images/{uniqueFileName}";
        }

        public async Task RemoveImage(string oldPath)
        {
            if (string.IsNullOrWhiteSpace(oldPath))
            {
                return;
            }

            // 🔹 Ensure the correct absolute path
            string imagePath = oldPath.StartsWith(_basePath)
                ? oldPath
                : Path.Combine(_basePath, oldPath.TrimStart('/').Replace("/", "\\"));


            // 🔹 Check if the file exists before deleting
            if (File.Exists(imagePath))
            {
                try
                {
                    // 🔥 Ensure the file is not locked before deleting
                    File.SetAttributes(imagePath, FileAttributes.Normal);

                    // 🔥 Delete the file
                    File.Delete(imagePath);
                    _logger.LogInformation($"✅ Successfully deleted: {imagePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"❌ Error deleting file: {ex.Message}");
                    throw new Exception($"Failed to delete file: {imagePath}", ex);
                }
            }
            else
            {
                _logger.LogInformation($"⚠️ File not found: {imagePath}");
            }
        }


    }
}
