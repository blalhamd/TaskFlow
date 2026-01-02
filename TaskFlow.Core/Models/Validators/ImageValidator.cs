using FluentValidation;
using Microsoft.AspNetCore.Http;
using TaskFlow.Shared.Common;

namespace TaskFlow.Core.Models.Validators
{
    public abstract class ImageValidator<T> : AbstractValidator<T>
    {
        public bool IsValidLength(IFormFile file)
        {
           return file.Length <= ApplicationConstants.MaxFileSize;
        }

        public bool IsValidExtension(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);

            return !string.IsNullOrEmpty(extension) && ApplicationConstants.AllowedExtensions.Contains(extension);
        }
    }
}
