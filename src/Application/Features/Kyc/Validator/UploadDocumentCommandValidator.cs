using FluentValidation;
using Microsoft.AspNetCore.Http;
using TegWallet.Application.Features.Kyc.Command;

namespace TegWallet.Application.Features.Kyc.Validator;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Client ID cannot be empty");

        RuleFor(x => x.DocumentType)
            .IsInEnum()
            .WithMessage("Invalid document type");

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

        RuleFor(x => x.FrontImage)
            .NotNull()
            .WithMessage("Front image is required")
            .Must(BeValidImageFile)
            .WithMessage("Front image must be a valid image file (JPEG, PNG, GIF, BMP, WebP)")
            .Must(BeValidFileSize)
            .WithMessage("Front image size must be between 10KB and 10MB");

        RuleFor(x => x.BackImage)
            .Must(BeValidImageFile)
            .When(x => x.BackImage != null)
            .WithMessage("Back image must be a valid image file")
            .Must(BeValidFileSize)
            .When(x => x.BackImage != null)
            .WithMessage("Back image size must be between 10KB and 10MB");

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

    private static bool BeValidImageFile(IFormFile file)
    {
        if (file == null) return false;

        var allowedContentTypes = new[]
        {
            "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp"
        };

        return allowedContentTypes.Contains(file.ContentType.ToLower());
    }

    private static bool BeValidFileSize(IFormFile file)
    {
        if (file == null) return false;
        return file.Length is > 10 * 1024 and < 10 * 1024 * 1024; // 10KB to 10MB
    }
}