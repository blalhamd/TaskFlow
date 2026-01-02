using FluentValidation;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Shared.Common;

namespace TaskFlow.Core.Models.Validators
{
    public class UpdateDeveloperRequestValidator : ImageValidator<UpdateDeveloperRequest>
    {
        public UpdateDeveloperRequestValidator()
        {
            RuleFor(x => x.Id)
              .NotEmpty().WithMessage("Email is required");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .Length(min: 3, max: 50).WithMessage("length of full name must be between 3 and 50 characters");

            RuleFor(x => x.JobTitle)
                .NotEmpty().WithMessage("Job title is required")
                .Length(min: 3, max: 50).WithMessage("Length of job title must be between 3 and 50 characters");

            RuleFor(x => x.Age)
                .GreaterThan(0).WithMessage("Age can't less than or equal zero");

            RuleFor(x => x.YearOfExperience)
               .GreaterThanOrEqualTo(0).WithMessage("Year of experience can't less than zero");

            RuleFor(x => x.JobLevel)
                .IsInEnum().WithMessage("Must select job level from suggested data");

            // Apply rules only if ImagePath is not null
            RuleFor(x => x.ImagePath)
                .NotNull().WithMessage("Please select an image file.")
                .When(x => x.ImagePath != null);

            When(x => x.ImagePath != null, () =>
            {
                RuleFor(x => x.ImagePath)
                    .Must(IsValidLength!)
                    .WithMessage($"Image must be {ApplicationConstants.MaxFileSize / (1024 * 1024)} MB or less.")
                    .Must(IsValidExtension!)
                    .WithMessage($"Only {string.Join(", ", ApplicationConstants.AllowedExtensions)} files are allowed.");
            });


        }
    }
}
