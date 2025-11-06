using FluentValidation;
using TegWallet.Application.Features.Core.ExchangeRates.Command;

namespace TegWallet.Application.Features.Core.ExchangeRates.Validator;

public class ExtendExchangeRateValidityCommandValidator : AbstractValidator<ExtendExchangeRateValidityCommand>
{
    public ExtendExchangeRateValidityCommandValidator()
    {
        RuleFor(x => x.ExchangeRateId).ValidateNotEmptyGuid("Exchange rate ID");

        RuleFor(x => x.NewEffectiveTo)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("New effective date must be in the future")
            .Must((command, newEffectiveTo) => newEffectiveTo > DateTime.UtcNow.AddYears(1) == false)
            .WithMessage("New effective date cannot be more than 1 year in the future");

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