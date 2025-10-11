using FluentValidation;
using TegWallet.Application.Features.Core.Wallet.Command;

namespace TegWallet.Application.Features.Core.Wallet.Validators;

public class RejectDepositCommandValidator : AbstractValidator<RejectDepositCommand>
{
    public RejectDepositCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required")
            .NotEqual(Guid.Empty).WithMessage("Client ID must be a valid GUID");

        RuleFor(x => x.LedgerId)
            .NotEmpty().WithMessage("Ledger ID is required")
            .NotEqual(Guid.Empty).WithMessage("Ledger ID must be a valid GUID");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters");

        RuleFor(x => x.RejectedBy)
            .NotEmpty().WithMessage("Rejector name is required")
            .MaximumLength(100).WithMessage("Rejector name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.RejectedBy));
    }
}