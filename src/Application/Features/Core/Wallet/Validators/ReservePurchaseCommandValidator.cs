using FluentValidation;
using TegWallet.Application.Features.Core.Wallet.Command;

namespace TegWallet.Application.Features.Core.Wallet.Validators;

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