using FluentValidation;
using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Command;

namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Validators;

public class UpdateMinimumAmountConfigurationCommandValidator : AbstractValidator<UpdateMinimumAmountConfigurationCommand>
{
    public UpdateMinimumAmountConfigurationCommandValidator()
    {
        RuleFor(x => x.ConfigurationId)
            .NotEmpty()
            .WithMessage("Configuration ID is required");

        RuleFor(x => x.MinimumAmount)
            .GreaterThan(0)
            .WithMessage("Minimum amount must be greater than 0")
            .LessThan(1000000000)
            .WithMessage("Minimum amount is too large");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty()
            .WithMessage("Effective from date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Effective from date cannot be in the past");

        RuleFor(x => x.EffectiveTo)
            .Must((command, effectiveTo) => effectiveTo == null || effectiveTo > command.EffectiveFrom)
            .WithMessage("Effective to date must be after effective from date");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty()
            .WithMessage("Updated by is required")
            .MaximumLength(100)
            .WithMessage("Updated by cannot exceed 100 characters");
    }
}