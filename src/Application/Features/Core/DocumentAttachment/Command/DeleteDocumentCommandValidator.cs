using FluentValidation;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.DocumentAttachment.Command;

public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required")
            .NotEqual(Guid.Empty).WithMessage("Entity ID must be a valid GUID");

        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required")
            .Must(BeValidEntityType).WithMessage("Entity type must be either 'Ledger' or 'Reservation'");

        RuleFor(x => x.AttachmentId)
            .NotEmpty().WithMessage("Attachment ID is required")
            .NotEqual(Guid.Empty).WithMessage("Attachment ID must be a valid GUID");

        RuleFor(x => x.DeletedBy)
            .NotEmpty().WithMessage("DeletedBy (Client ID) is required")
            .NotEqual(Guid.Empty).WithMessage("DeletedBy must be a valid GUID");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

        // Business rules
        RuleFor(x => x)
            .MustAsync(async (command, cancellation) => await EntityIsPending(command.EntityId, command.EntityType, cancellation))
                .WithMessage("Cannot delete document from an entity that is not in pending status");

        RuleFor(x => x)
            .MustAsync(async (command, cancellation) => await ClientHasPermissionToDelete(command.EntityId, command.EntityType, command.DeletedBy, cancellation))
                .WithMessage("Client does not have permission to delete documents from this entity");
    }

    private static bool BeValidEntityType(string entityType)
    {
        return entityType == nameof(Ledger) || entityType == nameof(Reservation);
    }

    private async Task<bool> EntityIsPending(Guid entityId, string entityType, CancellationToken cancellationToken)
    {
        // In a real implementation, you would inject repositories and check the entity status
        // For now, this is a placeholder
        return await Task.FromResult(true);
    }

    private async Task<bool> ClientHasPermissionToDelete(Guid entityId, string entityType, Guid deletedBy, CancellationToken cancellationToken)
    {
        // In a real implementation:
        // 1. Check if the client (deletedBy) owns the entity (ledger/reservation)
        // 2. Or check if the client has permission to delete documents from this entity
        // For now, this is a placeholder
        return await Task.FromResult(true);
    }
}