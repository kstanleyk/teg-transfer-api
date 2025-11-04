using FluentValidation;
using TegWallet.Application.Features.Core.Wallets.Command;

namespace TegWallet.Application.Features.Core.Wallets.Validators;

public class CancelPurchaseCommandValidator : AbstractValidator<CancelPurchaseCommand>
{
    public CancelPurchaseCommandValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CancelledBy).NotEmpty().MaximumLength(100);
    }
}