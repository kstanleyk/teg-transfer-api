using FluentValidation;
using Microsoft.AspNetCore.Http;
using TegWallet.Application.Features.Kyc.Command;

namespace TegWallet.Application.Features.Kyc.Validator;

public class AddDocumentFrontImageCommandValidator : AbstractValidator<AddDocumentFrontImageCommand>
{
    public AddDocumentFrontImageCommandValidator()
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

        RuleFor(x => x.FrontImage)
            .NotNull()
            .WithMessage("Front image is required")
            .Must(BeValidImageFile)
            .WithMessage("Front image must be a valid image file (JPEG, PNG, GIF, BMP, WebP)")
            .Must(BeValidFileSize)
            .WithMessage("Front image size must be between 10KB and 10MB");
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