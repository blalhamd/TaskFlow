using FluentValidation;
using TaskFlow.Core.Models.Dtos.V1;

namespace TaskFlow.Core.Models.Validators
{
    public class UpdateTaskEntityValidator : AbstractValidator<UpdateTaskEntity>
    {
        public UpdateTaskEntityValidator()
        {
            RuleFor(x => x.Id)
               .NotEmpty().WithMessage("Id is required.");

            RuleFor(x => x.Content)
               .NotEmpty().WithMessage("Task content is required.")
               .MaximumLength(1000).WithMessage("Task content cannot exceed 1000 characters.");

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
    }
}
