using FluentValidation;
using TegWallet.Application.Features.Kyc.Command;

namespace TegWallet.Application.Features.Kyc.Validator;

public class SubmitDocumentForVerificationCommandValidator : AbstractValidator<SubmitDocumentForVerificationCommand>
{
    public SubmitDocumentForVerificationCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Client ID cannot be empty");

        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("Document ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Document ID cannot be empty");
    }
}