using FluentValidation;
using TegWallet.Application.Features.Core.Wallets.Command;

namespace TegWallet.Application.Features.Core.Wallets.Validators;

public class ApprovePurchaseCommandValidator : AbstractValidator<ApprovePurchaseCommand>
{
    public ApprovePurchaseCommandValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.ProcessedBy).NotEmpty().MaximumLength(100);
    }
}