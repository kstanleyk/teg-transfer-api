using FluentValidation;
using TegWallet.Application.Features.Kyc.Command;

namespace TegWallet.Application.Features.Kyc.Validator;

public class SubmitKycForReviewCommandValidator : AbstractValidator<SubmitKycForReviewCommand>
{
    public SubmitKycForReviewCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

        RuleFor(x => x.SubmittedBy)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Submitted by must be provided and cannot exceed 100 characters");

        RuleFor(x => x.SubmittedBy)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.SubmittedBy) && x.SubmittedBy.Contains("@"))
            .WithMessage("Submitted by must be a valid email address when using email");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes != null)
            .WithMessage("Notes cannot exceed 500 characters");
    }
}