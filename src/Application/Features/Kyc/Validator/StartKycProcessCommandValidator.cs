using FluentValidation;
using TegWallet.Application.Features.Kyc.Command;

namespace TegWallet.Application.Features.Kyc.Validator;

public class StartKycProcessCommandValidator : AbstractValidator<StartKycProcessCommand>
{
    public StartKycProcessCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Client ID cannot be empty");
    }
}