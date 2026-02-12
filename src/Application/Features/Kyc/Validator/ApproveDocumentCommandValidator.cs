using FluentValidation;
using TegWallet.Application.Features.Kyc.Command;

namespace TegWallet.Application.Features.Kyc.Validator;

public class ApproveDocumentCommandValidator : AbstractValidator<ApproveDocumentCommand>
{
    public ApproveDocumentCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Client ID cannot be empty");

        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("Document ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Document ID cannot be empty");

        RuleFor(x => x.ApprovedBy)
            .NotEmpty()
            .WithMessage("Approved by is required")
            .MaximumLength(100)
            .WithMessage("Approved by cannot exceed 100 characters")
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.ApprovedBy) && x.ApprovedBy.Contains("@"))
            .WithMessage("Approved by must be a valid email address when using email");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => x.Notes != null)
            .WithMessage("Notes cannot exceed 500 characters");

        // Custom validation for admin email format
        RuleFor(x => x.ApprovedBy)
            .Must(BeValidAdminIdentifier)
            .WithMessage("Approved by must be a valid email address or admin username")
            .When(x => !string.IsNullOrEmpty(x.ApprovedBy));
    }

    private bool BeValidAdminIdentifier(string identifier)
    {
        // Check if it's an email
        if (identifier.Contains("@"))
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(identifier);
                return mailAddress.Address == identifier;
            }
            catch
            {
                return false;
            }
        }

        // Check if it's a valid admin username (alphanumeric with dots and underscores)
        return System.Text.RegularExpressions.Regex.IsMatch(identifier, @"^[a-zA-Z0-9._-]+$");
    }
}