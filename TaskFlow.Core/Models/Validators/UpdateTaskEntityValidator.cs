using FluentValidation;
using Microsoft.AspNetCore.Http;
using TaskFlow.Core.Models.Dtos.V1;

namespace TaskFlow.Core.Models.Validators
{
    public class UpdateTaskEntityValidator : AbstractValidator<UpdateTaskEntity>
    {
        private readonly string[] _allowedExtensions = { ".jpg", ".png", ".jpeg", ".pdf", ".docx" };
        private const long MaxBytes = 2L * 1024 * 1024; // 2 MB

        public UpdateTaskEntityValidator()
        {
            RuleFor(x => x.Id)
               .NotEmpty().WithMessage("Id is required.");

            When(x => !string.IsNullOrWhiteSpace(x.Content), () =>
            {
                RuleFor(x => x.Content)
               .NotEmpty().WithMessage("Task content is required.")
               .MaximumLength(1000).WithMessage("Task content cannot exceed 1000 characters.");
            });

            When(x => x.Document != null, () =>
            {
                RuleFor(x => x.Document)
                    .Must(FileSizeIsValid!)
                    .WithMessage($"Document must be {MaxBytes / (1024 * 1024)} MB or less.")
                    .Must(HasAllowedExtension!)
                    .WithMessage($"Only {string.Join(", ", _allowedExtensions)} files are allowed.");
            });

            RuleFor(x => x.AssignedToDeveloperId)
                .NotEmpty().WithMessage("Assigned developer ID is required.");

            RuleFor(x => x.StartAt)
                .NotNull().WithMessage("Start time of Task is required.")
                .Must(BeAFutureDate).WithMessage("Start time of Task cannot be in the past.")
                .Must((model, startAt) =>
                       startAt < model.EndAt
                   ).WithMessage("Start time must be earlier than the end time.");

            RuleFor(x => x.EndAt)
                .NotNull().WithMessage("Task deadline is required.")
                .Must(BeAFutureDate).WithMessage("Task deadline cannot be in the past.");

            RuleFor(x => x.Progress)
                .IsInEnum().WithMessage("Task progress invalid");
        }

        private static bool BeAFutureDate(DateTimeOffset endAt)
            => endAt >= DateTimeOffset.UtcNow;

        private bool FileSizeIsValid(IFormFile file)
         => file.Length <= MaxBytes;

        private bool HasAllowedExtension(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(ext) && _allowedExtensions.Contains(ext);
        }
    }
}
