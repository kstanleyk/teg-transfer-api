using FluentValidation;
using TegWallet.Application.Features.Core.Wallet.Command;

namespace TegWallet.Application.Features.Core.Wallet.Validators;

public class ApproveDepositCommandValidator : AbstractValidator<ApproveDepositCommand>
{
    public ApproveDepositCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required")
            .NotEqual(Guid.Empty).WithMessage("Client ID must be a valid GUID");

        RuleFor(x => x.LedgerId)
            .NotEmpty().WithMessage("Ledger ID is required")
            .NotEqual(Guid.Empty).WithMessage("Ledger ID must be a valid GUID");

        RuleFor(x => x.ApprovedBy)
            .NotEmpty().WithMessage("Approver name is required")
            .MaximumLength(100).WithMessage("Approver name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ApprovedBy));
    }
}