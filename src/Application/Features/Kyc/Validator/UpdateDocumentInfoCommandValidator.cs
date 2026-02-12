using FluentValidation;
using TegWallet.Application.Features.Kyc.Command;

namespace TegWallet.Application.Features.Kyc.Validator;

public class UpdateDocumentInfoCommandValidator : AbstractValidator<UpdateDocumentInfoCommand>
{
    public UpdateDocumentInfoCommandValidator()
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

        RuleFor(x => x.DocumentNumber)
            .NotEmpty()
            .WithMessage("Document number is required")
            .MaximumLength(50)
            .WithMessage("Document number cannot exceed 50 characters");

        RuleFor(x => x.IssueDate)
            .NotEmpty()
            .WithMessage("Issue date is required")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Issue date cannot be in the future");

        RuleFor(x => x.ExpiryDate)
            .NotEmpty()
            .WithMessage("Expiry date is required")
            .GreaterThan(x => x.IssueDate)
            .WithMessage("Expiry date must be after issue date")
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Document is already expired");

        RuleFor(x => x.FullName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.FullName))
            .WithMessage("Full name cannot exceed 200 characters");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth cannot be in the future");

        RuleFor(x => x.Nationality)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Nationality))
            .WithMessage("Nationality cannot exceed 50 characters");

        RuleFor(x => x.IssuingAuthority)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.IssuingAuthority))
            .WithMessage("Issuing authority cannot exceed 100 characters");
    }
}