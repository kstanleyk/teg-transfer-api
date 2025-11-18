using FluentValidation;
using TegWallet.Application.Features.Core.ExchangeRates.Command;

namespace TegWallet.Application.Features.Core.ExchangeRates.Validator;

public class ExchangeRateTierRequestValidator : AbstractValidator<ExchangeRateTierRequest>
{
    public ExchangeRateTierRequestValidator()
    {
        RuleFor(x => x.MinAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Min amount must be greater than or equal to 0")
            .LessThan(x => x.MaxAmount)
            .WithMessage("Min amount must be less than max amount");

        RuleFor(x => x.MaxAmount)
            .GreaterThan(x => x.MinAmount)
            .WithMessage("Max amount must be greater than min amount")
            .LessThan(1000000000)
            .WithMessage("Max amount is too large");

        RuleFor(x => x.Rate)
            .GreaterThan(0)
            .WithMessage("Rate must be greater than 0")
            .LessThan(10000)
            .WithMessage("Rate is too large");

        RuleFor(x => x.Margin)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Margin must be greater than or equal to 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Margin cannot exceed 100%");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("Created by is required")
            .MaximumLength(100)
            .WithMessage("Created by cannot exceed 100 characters");
    }
}