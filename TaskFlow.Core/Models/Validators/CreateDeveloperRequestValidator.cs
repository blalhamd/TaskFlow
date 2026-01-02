using FluentValidation;
using TaskFlow.Core.Models.Dtos.V1;
using TaskFlow.Shared.Common;

namespace TaskFlow.Core.Models.Validators
{
    public class CreateDeveloperRequestValidator : ImageValidator<CreateDeveloperRequest>
    {
        public CreateDeveloperRequestValidator()
        {

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
               .NotEmpty().WithMessage("Password is required")
               .MinimumLength(5).WithMessage("Password must be at least 5 characters.")
               .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
               .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

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

            RuleFor(x => x.ImagePath)
                    .NotNull().WithMessage("Please select an image file.")
                    .Must(IsValidLength!)
                    .WithMessage($"Document must be {ApplicationConstants.MaxFileSize / (1024 * 1024)} MB or less.")
                    .Must(IsValidExtension!)
                    .WithMessage($"Only {string.Join(", ", ApplicationConstants.AllowedExtensions)} files are allowed.")
                    .When(x => x.ImagePath != null);
        }
    }
}
