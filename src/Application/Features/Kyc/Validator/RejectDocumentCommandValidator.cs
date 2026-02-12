using FluentValidation;
using TegWallet.Application.Features.Kyc.Command;

namespace TegWallet.Application.Features.Kyc.Validator;

public class RejectDocumentCommandValidator : AbstractValidator<RejectDocumentCommand>
{
    public RejectDocumentCommandValidator()
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

        RuleFor(x => x.RejectedBy)
            .NotEmpty()
            .WithMessage("Rejected by is required")
            .MaximumLength(100)
            .WithMessage("Rejected by cannot exceed 100 characters")
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.RejectedBy) && x.RejectedBy.Contains("@"))
            .WithMessage("Rejected by must be a valid email address when using email");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Rejection reason is required")
            .MinimumLength(10)
            .WithMessage("Rejection reason must be at least 10 characters")
            .MaximumLength(1000)
            .WithMessage("Rejection reason cannot exceed 1000 characters")
            .Must(reason => !reason.All(char.IsWhiteSpace))
            .WithMessage("Rejection reason cannot be only whitespace")
            .Must(reason => !reason.Contains("TODO") && !reason.Contains("FIXME") && !reason.Contains("XXX"))
            .WithMessage("Please provide a specific rejection reason");

        // Custom validation for admin identifier
        RuleFor(x => x.RejectedBy)
            .Must(BeValidAdminIdentifier)
            .WithMessage("Rejected by must be a valid email address or admin username")
            .When(x => !string.IsNullOrEmpty(x.RejectedBy));

        // Custom validation for meaningful rejection reasons
        RuleFor(x => x.Reason)
            .Must(BeMeaningfulRejectionReason)
            .WithMessage("Please provide a specific and meaningful rejection reason")
            .When(x => !string.IsNullOrEmpty(x.Reason));
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

        // Check if it's a valid admin username
        return System.Text.RegularExpressions.Regex.IsMatch(identifier, @"^[a-zA-Z0-9._-]+$");
    }

    private bool BeMeaningfulRejectionReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return false;

        var trimmedReason = reason.Trim();

        // Check for common non-meaningful patterns
        var meaninglessPatterns = new[]
        {
            "rejected",
            "not accepted",
            "invalid",
            "wrong",
            "bad",
            "poor quality",
            "unclear"
        };

        // If the reason is just one of these words without additional context
        if (meaninglessPatterns.Any(p => trimmedReason.Equals(p, StringComparison.OrdinalIgnoreCase)))
            return false;

        // Reason should contain more than just a single word
        var words = trimmedReason.Split(new[] { ' ', ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries);
        return words.Length >= 2;
    }
}