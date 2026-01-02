using FluentValidation;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Shared.Common;

namespace TaskFlow.Core.Models.Validators
{
    public class CreateTaskEntityValidator : ImageValidator<CreateTaskEntity>
    {

        public CreateTaskEntityValidator()
        {
            When(x => !string.IsNullOrWhiteSpace(x.Content), () =>
            {
                RuleFor(x => x.Content)
               .NotEmpty().WithMessage("Task content is required.")
               .MaximumLength(1000).WithMessage("Task content cannot exceed 1000 characters.");
            });

            When(x => x.Document != null, () =>
            {
                RuleFor(x => x.Document)
                    .Must(IsValidLength!)
                    .WithMessage($"Document must be {ApplicationConstants.MaxFileSize / (1024 * 1024)} MB or less.")
                    .Must(IsValidExtension!)
                    .WithMessage($"Only {string.Join(", ", ApplicationConstants.AllowedExtensions)} files are allowed.");
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
        }

        private static bool BeAFutureDate(DateTimeOffset endAt)
            => endAt >= DateTimeOffset.UtcNow;

    }
}

