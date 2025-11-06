using FluentValidation;
using TegWallet.Application.Features.Core.ExchangeRate.Command;

namespace TegWallet.Application.Features.Core.ExchangeRate.Validator;

public class CreateGeneralExchangeRateCommandValidator : AbstractValidator<CreateGeneralExchangeRateCommand>
{
    public CreateGeneralExchangeRateCommandValidator()
    {
        RuleFor(x => x.BaseCurrency)
            .NotNull()
            .WithMessage("Base currency is required");

        RuleFor(x => x.TargetCurrency)
            .NotNull()
            .WithMessage("Target currency is required")
            .NotEqual(x => x.BaseCurrency)
            .WithMessage("Base currency and target currency cannot be the same");

        RuleFor(x => x.BaseCurrencyValue)
            .GreaterThan(0)
            .WithMessage("Base currency value must be positive");

        RuleFor(x => x.TargetCurrencyValue)
            .GreaterThan(0)
            .WithMessage("Target currency value must be positive");

        RuleFor(x => x.Margin)
            .InclusiveBetween(0, 1)
            .WithMessage("Margin must be between 0 and 1");

        RuleFor(x => x.EffectiveFrom)
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("Effective date cannot be more than 5 minutes in the past");

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