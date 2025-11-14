using FluentValidation;
using TegWallet.Application.Features.Core.ExchangeRates.Command;

namespace TegWallet.Application.Features.Core.ExchangeRates.Validator;

public class CreateGeneralExchangeRateCommandValidator : AbstractValidator<CreateGeneralExchangeRateCommand>
{
    public CreateGeneralExchangeRateCommandValidator()
    {
        RuleFor(x => x.BaseCurrency).ValidateCurrency();

        RuleFor(x => x.TargetCurrency)
            .ValidateCurrency()
            .NotEqual(x => x.BaseCurrency)
            .WithMessage("Base currency and target currency cannot be the same");

        RuleFor(x => x.BaseCurrencyValue).ValidateCurrencyValue();
        RuleFor(x => x.TargetCurrencyValue).ValidateCurrencyValue();
        RuleFor(x => x.Margin).ValidateMargin();

        //RuleFor(x => x.EffectiveFrom)
        //    .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5))
        //    .WithMessage("Effective date cannot be more than 5 minutes in the past");

        RuleFor(x => x.EffectiveTo)
            .Must((command, effectiveTo) => effectiveTo == null || effectiveTo > command.EffectiveFrom)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("Created by is required")
            .MaximumLength(100)
            .WithMessage("Created by cannot exceed 100 characters");

        RuleFor(x => x.Source)
            .NotEmpty()
            .WithMessage("Source is required")
            .MaximumLength(50)
            .WithMessage("Source cannot exceed 50 characters");
    }
}