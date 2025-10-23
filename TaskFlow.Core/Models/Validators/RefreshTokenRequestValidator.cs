using FluentValidation;
using TaskFlow.Core.Models.Dtos.V1;

namespace TaskFlow.Core.Models.Validators
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh Token is required");
        }
    }
}
