using FluentValidation;
using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Command;

namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Validators;

public class CreateMinimumAmountConfigurationCommandValidator : AbstractValidator<CreateMinimumAmountConfigurationCommand>
{
    public CreateMinimumAmountConfigurationCommandValidator()
    {
        RuleFor(x => x.BaseCurrencyCode)
            .NotEmpty()
            .WithMessage("Base currency code is required")
            .Length(3)
            .WithMessage("Base currency code must be 3 characters")
            .Matches(@"^[A-Z]{3}$")
            .WithMessage("Base currency code must be 3 uppercase letters");

        RuleFor(x => x.TargetCurrencyCode)
            .NotEmpty()
            .WithMessage("Target currency code is required")
            .Length(3)
            .WithMessage("Target currency code must be 3 characters")
            .Matches(@"^[A-Z]{3}$")
            .WithMessage("Target currency code must be 3 uppercase letters");

        RuleFor(x => x.MinimumAmount)
            .GreaterThan(0)
            .WithMessage("Minimum amount must be greater than 0")
            .LessThan(1000000000) // 1 billion
            .WithMessage("Minimum amount is too large");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty()
            .WithMessage("Effective from date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Effective from date cannot be in the past");

        RuleFor(x => x.EffectiveTo)
            .Must((command, effectiveTo) => effectiveTo == null || effectiveTo > command.EffectiveFrom)
            .WithMessage("Effective to date must be after effective from date");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("Created by is required")
            .MaximumLength(100)
            .WithMessage("Created by cannot exceed 100 characters");
    }
}