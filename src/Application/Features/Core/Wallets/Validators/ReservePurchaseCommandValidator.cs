using FluentValidation;
using TegWallet.Application.Features.Core.Wallets.Command;

namespace TegWallet.Application.Features.Core.Wallets.Validators;

public class ReservePurchaseCommandValidator : AbstractValidator<ReservePurchaseCommand>
{
    public ReservePurchaseCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.PurchaseAmount).GreaterThan(0);
        RuleFor(x => x.ServiceFeeAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SupplierDetails).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PaymentMethod).NotEmpty().MaximumLength(100);
    }
}