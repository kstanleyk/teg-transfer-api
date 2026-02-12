using FluentValidation;
using TegWallet.Application.Features.Kyc.Command;

namespace TegWallet.Application.Features.Kyc.Validator;

public class ApproveKycLevel3CommandValidator : AbstractValidator<ApproveKycLevel3Command>
{
    public ApproveKycLevel3CommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Client ID cannot be empty");

        RuleFor(x => x.ApprovedBy)
            .NotEmpty()
            .WithMessage("Approved by is required")
            .MaximumLength(100)
            .WithMessage("Approved by cannot exceed 100 characters")
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.ApprovedBy) && x.ApprovedBy.Contains("@"))
            .WithMessage("Approved by must be a valid email address when using email");

        RuleFor(x => x.ExpiresAt)
            .NotEmpty()
            .WithMessage("Expiration date is required")
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Expiration date must be in the future")
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1)) // Level 3 has stricter expiry
            .WithMessage("Level 3 verification cannot exceed 1 year");

        RuleFor(x => x.Notes)
            .MaximumLength(2000) // Level 3 can have more detailed notes
            .When(x => x.Notes != null)
            .WithMessage("Notes cannot exceed 2000 characters")
            .Must(notes => notes == null || notes.Split('.').Length <= 5)
            .WithMessage("Notes should be concise (max 5 sentences)");

        // Custom validation for senior admin identifier
        RuleFor(x => x.ApprovedBy)
            .Must(BeValidSeniorAdminIdentifier)
            .WithMessage("Level 3 approvals require senior admin credentials")
            .When(x => !string.IsNullOrEmpty(x.ApprovedBy));
    }

    private bool BeValidSeniorAdminIdentifier(string identifier)
    {
        // Level 3 approvals require senior admin approval
        // Check if it's a valid senior admin email
        if (identifier.Contains("@"))
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(identifier);
                var domain = mailAddress.Host.ToLower();

                // Senior admins typically have specific domains or patterns
                var seniorAdminDomains = new[] { "admin.", "security.", "compliance." };
                return mailAddress.Address == identifier &&
                       seniorAdminDomains.Any(d => domain.Contains(d));
            }
            catch
            {
                return false;
            }
        }

        // Check if it's a valid senior admin username
        var seniorAdminPatterns = new[] { "admin_", "security_", "compliance_", "risk_" };
        return seniorAdminPatterns.Any(pattern => identifier.StartsWith(pattern, StringComparison.OrdinalIgnoreCase));
    }
}