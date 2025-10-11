using FluentValidation;
using TegWallet.Application.Features.Core.Wallet.Command;

namespace TegWallet.Application.Features.Core.Wallet.Validators;

public class CancelPurchaseCommandValidator : AbstractValidator<CancelPurchaseCommand>
{
    public CancelPurchaseCommandValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CancelledBy).NotEmpty().MaximumLength(100);
    }
}