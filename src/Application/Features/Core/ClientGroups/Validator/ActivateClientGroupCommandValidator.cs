using FluentValidation;
using TegWallet.Application.Features.Core.ClientGroups.Command;

namespace TegWallet.Application.Features.Core.ClientGroups.Validator;

public class ActivateClientGroupCommandValidator : AbstractValidator<ActivateClientGroupCommand>
{
    public ActivateClientGroupCommandValidator()
    {
        RuleFor(x => x.ClientGroupId)
            .NotEmpty()
            .WithMessage("Client group ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Client group ID must be a valid GUID");

        RuleFor(x => x.ActivatedBy)
            .NotEmpty()
            .WithMessage("Activated by is required")
            .MaximumLength(100)
            .WithMessage("Activated by cannot exceed 100 characters");
    }
}