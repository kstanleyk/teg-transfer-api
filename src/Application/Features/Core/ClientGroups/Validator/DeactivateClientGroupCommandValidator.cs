using FluentValidation;
using TegWallet.Application.Features.Core.ClientGroups.Command;

namespace TegWallet.Application.Features.Core.ClientGroups.Validator;

public class DeactivateClientGroupCommandValidator : AbstractValidator<DeactivateClientGroupCommand>
{
    public DeactivateClientGroupCommandValidator()
    {
        RuleFor(x => x.ClientGroupId)
            .NotEmpty()
            .WithMessage("Client group ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Client group ID must be a valid GUID");

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