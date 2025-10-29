using FluentValidation;
using TaskFlow.Core.Models.Dtos.V1;

namespace TaskFlow.Core.Models.Validators
{
    public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
    {
        public CreateCommentRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotNull()
                .WithMessage("content can't be null");
        }
        
    }
}
