using FluentValidation;
using TegWallet.Application.Features.Core.ExchangeRate.Command;

namespace TegWallet.Application.Features.Core.ExchangeRate.Validator;

public class UpdateExchangeRateCommandValidator : AbstractValidator<UpdateExchangeRateCommand>
{
    public UpdateExchangeRateCommandValidator()
    {
        RuleFor(x => x.ExchangeRateId)
            .NotEmpty()
            .WithMessage("Exchange rate ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Exchange rate ID must be a valid GUID");

        RuleFor(x => x.NewBaseCurrencyValue).ValidateCurrencyValue();
        RuleFor(x => x.NewTargetCurrencyValue).ValidateCurrencyValue();
        RuleFor(x => x.NewMargin).ValidateMargin();

        RuleFor(x => x.UpdatedBy)
            .NotEmpty()
            .WithMessage("Updated by is required")
            .MaximumLength(100)
            .WithMessage("Updated by cannot exceed 100 characters");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}