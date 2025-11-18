using FluentValidation;
using TegWallet.Application.Features.Core.MinimumAmountConfigurations.Command;

namespace TegWallet.Application.Features.Core.MinimumAmountConfigurations.Validators;

public class DeactivateMinimumAmountConfigurationCommandValidator : AbstractValidator<DeactivateMinimumAmountConfigurationCommand>
{
    public DeactivateMinimumAmountConfigurationCommandValidator()
    {
        RuleFor(x => x.ConfigurationId)
            .NotEmpty()
            .WithMessage("Configuration ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters");

        RuleFor(x => x.DeactivatedBy)
            .NotEmpty()
            .WithMessage("Deactivated by is required")
            .MaximumLength(100)
            .WithMessage("Deactivated by cannot exceed 100 characters");
    }
}