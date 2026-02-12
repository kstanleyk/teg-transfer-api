using FluentValidation;
using Microsoft.AspNetCore.Http;
using TegWallet.Application.Features.Kyc.Command;

namespace TegWallet.Application.Features.Kyc.Validator;

public class AddSelfieImageCommandValidator : AbstractValidator<AddSelfieImageCommand>
{
    public AddSelfieImageCommandValidator()
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

        RuleFor(x => x.SelfieImage)
            .NotNull()
            .WithMessage("Selfie image is required")
            .Must(BeValidImageFile)
            .WithMessage("Selfie must be a valid image file (JPEG, PNG)")
            .Must(BeValidFileSize)
            .WithMessage("Selfie image size must be between 10KB and 5MB");
    }

    private static bool BeValidImageFile(IFormFile file)
    {
        if (file == null) return false;

        var allowedContentTypes = new[]
        {
            "image/jpeg", "image/png" // Selfies typically JPEG or PNG
        };

        return allowedContentTypes.Contains(file.ContentType.ToLower());
    }

    private static bool BeValidFileSize(IFormFile file)
    {
        if (file == null) return false;
        return file.Length is > 10 * 1024 and < 5 * 1024 * 1024; // 10KB to 5MB (smaller limit for selfies)
    }
}