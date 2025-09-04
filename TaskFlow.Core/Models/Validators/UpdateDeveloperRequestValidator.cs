using FluentValidation;
using TaskFlow.Core.Models.Dtos.V1;

namespace TaskFlow.Core.Models.Validators
{
    public class UpdateDeveloperRequestValidator : AbstractValidator<UpdateDeveloperRequest>
    {
        private const int maxFileSize = 2 * 1024 * 1024;
        public UpdateDeveloperRequestValidator()
        {
            RuleFor(x => x.Id)
              .NotEmpty().WithMessage("Email is required");

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

            // Apply rules only if ImagePath is not null
            RuleFor(x => x.ImagePath)
                .NotNull().WithMessage("Please select an image file.")
                .When(x => x.ImagePath != null);

            // Rule for file size: must not exceed 2MB
            RuleFor(x => x.ImagePath!.Length)
                .LessThanOrEqualTo(maxFileSize)
                .WithMessage($"The image size must not exceed {maxFileSize / (1024 * 1024)} MB.")
                .When(x => x.ImagePath != null);

            // Rule for file extension: must be .jpg or .png
            RuleFor(x => x.ImagePath!.ContentType)
                .Must(contentType => contentType.Equals("image/jpeg") || contentType.Equals("image/png"))
                .WithMessage("Only .jpg and .png image types are allowed.")
                .When(x => x.ImagePath != null);


        }
    }
}
