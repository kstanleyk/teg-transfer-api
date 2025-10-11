using FluentValidation;
using TegWallet.Application.Features.Core.Wallet.Command;

namespace TegWallet.Application.Features.Core.Wallet.Validators;

public class ApprovePurchaseCommandValidator : AbstractValidator<ApprovePurchaseCommand>
{
    public ApprovePurchaseCommandValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.ProcessedBy).NotEmpty().MaximumLength(100);
    }
}