using FluentValidation;
using TegWallet.Application.Features.Core.DocumentAttachment.Command;

namespace TegWallet.Application.Features.Core.DocumentAttachment.Validator;

public class AttachDocumentToReservationCommandValidator : AbstractValidator<AttachDocumentToReservationCommand>
{
    public AttachDocumentToReservationCommandValidator()
    {
        RuleFor(x => x.ReservationId)
            .NotEmpty().WithMessage("Reservation ID is required")
            .NotEqual(Guid.Empty).WithMessage("Reservation ID must be a valid GUID");

        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required")
            .Must(file => file.Length > 0).WithMessage("File cannot be empty");

        RuleFor(x => x.File.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("Document type is required")
            .Must(BeValidDocumentType).WithMessage("Invalid document type");

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("UploadedBy (Client ID) is required")
            .NotEqual(Guid.Empty).WithMessage("UploadedBy must be a valid GUID");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }

    private static bool BeValidDocumentType(string documentType)
    {
        var allowedTypes = Domain.Entity.Core.DocumentAttachment.GetAllowedDocumentTypes();
        return allowedTypes.Contains(documentType);
    }
}