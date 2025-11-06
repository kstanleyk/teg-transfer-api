using FluentValidation;
using TegWallet.Application.Features.Core.ClientGroups.Command;

namespace TegWallet.Application.Features.Core.ClientGroups.Validator;

public class UpdateClientGroupCommandValidator : AbstractValidator<UpdateClientGroupCommand>
{
    public UpdateClientGroupCommandValidator()
    {
        RuleFor(x => x.ClientGroupId)
            .NotEmpty()
            .WithMessage("Client group ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Client group ID must be a valid GUID");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Client group name is required")
            .Length(2, 50)
            .WithMessage("Client group name must be between 2 and 50 characters")
            .Matches(@"^[a-zA-Z0-9\s_-]+$")
            .WithMessage("Client group name can only contain letters, numbers, spaces, hyphens, and underscores");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty()
            .WithMessage("Updated by is required")
            .MaximumLength(100)
            .WithMessage("Updated by cannot exceed 100 characters");
    }
}