using FluentValidation;
using TegWallet.Application.Features.Core.ExchangeRates.Command;

namespace TegWallet.Application.Features.Core.ExchangeRates.Validator;

public class DeactivateExchangeRateCommandValidator : AbstractValidator<DeactivateExchangeRateCommand>
{
    public DeactivateExchangeRateCommandValidator()
    {
        RuleFor(x => x.ExchangeRateId).ValidateNotEmptyGuid("Exchange rate ID");

        RuleFor(x => x.DeactivatedBy)
            .NotEmpty()
            .WithMessage("Deactivated by is required")
            .MaximumLength(100)
            .WithMessage("Deactivated by cannot exceed 100 characters");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}