using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TegWallet.Application.Interfaces.Photos;

namespace TegWallet.Infrastructure.Photos;

public class DocumentService : IDocumentService
{
    private readonly Cloudinary _cloudinary;
    private readonly IOptions<CloudinarySettings> _config;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IOptions<CloudinarySettings> config,
        ILogger<DocumentService> logger)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );
        _config = config;
        _cloudinary = new Cloudinary(account);
        _logger = logger;
    }

    public async Task<DeletionResult> DeleteDocument(string publicId)
    {
        try
        {
            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document from Cloudinary. PublicId: {PublicId}", publicId);
            throw;
        }
    }

    public async Task<UploadResult?> UploadDocument(IFormFile file)
    {
        if (file.Length <= 0) return null;

        await using var stream = file.OpenReadStream();

        // Get the appropriate upload method based on file type
        var contentType = file.ContentType.ToLower();

        try
        {
            if (IsVideoFile(contentType))
            {
                return await UploadVideoAsync(file, stream);
            }

            if (IsPdfFile(contentType))
            {
                return await UploadPdfAsync(file, stream);
            }

            if (IsImageFile(contentType))
            {
                return await UploadImageAsync(file, stream);
            }

            _logger.LogWarning("Unsupported file type: {ContentType}", contentType);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Cloudinary. FileName: {FileName}, ContentType: {ContentType}",
                file.FileName, contentType);
            throw;
        }
    }

    private async Task<UploadResult> UploadVideoAsync(IFormFile file, Stream stream)
    {
        _logger.LogInformation("Uploading video file: {FileName}, Size: {Size} bytes",
            file.FileName, file.Length);

        var uploadParams = new VideoUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = _config.Value.Folder,
            // ResourceType is automatically set to "video" for VideoUploadParams
            // Optional: Add video-specific parameters
            EagerTransforms =
            [
                new Transformation().Width(854).Height(480).Crop("scale").Quality("auto"),
                new Transformation().Width(640).Height(360).Crop("scale").Quality("auto")
            ],
            EagerAsync = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        _logger.LogInformation("Video uploaded successfully. PublicId: {PublicId}, Format: {Format}, Duration: {Duration}s",
            result.PublicId, result.Format, result.Duration);

        return result;
    }

    private async Task<UploadResult> UploadPdfAsync(IFormFile file, Stream stream)
    {
        _logger.LogInformation("Uploading PDF file: {FileName}, Size: {Size} bytes",
            file.FileName, file.Length);

        // For PDFs, we use RawUploadParams
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = _config.Value.Folder,
            // Optional: Add PDF-specific parameters
            //ResourceType = ResourceType.Raw
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        _logger.LogInformation("PDF uploaded successfully. PublicId: {PublicId}, Format: {Format}",
            result.PublicId, result.Format);

        return result;
    }

    private async Task<UploadResult> UploadImageAsync(IFormFile file, Stream stream)
    {
        _logger.LogInformation("Uploading image file: {FileName}, Size: {Size} bytes",
            file.FileName, file.Length);

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = _config.Value.Folder,
            // Optional: Add image optimization parameters
            Transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto"),
            EagerTransforms =
            [
                new Transformation().Width(800).Height(600).Crop("limit"),
                new Transformation().Width(400).Height(300).Crop("limit")
            ]
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        _logger.LogInformation("Image uploaded successfully. PublicId: {PublicId}, Format: {Format}, Width: {Width}, Height: {Height}",
            result.PublicId, result.Format, result.Width, result.Height);

        return result;
    }

    // Helper methods to determine file type
    private static bool IsVideoFile(string contentType)
    {
        var videoTypes = new[]
        {
            "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo",
            "video/x-ms-wmv", "video/webm", "video/3gpp", "video/x-matroska"
        };
        return videoTypes.Contains(contentType);
    }

    private static bool IsPdfFile(string contentType) => contentType == "application/pdf";

    private static bool IsImageFile(string contentType)
    {
        var imageTypes = new[]
        {
            "image/jpeg", "image/png", "image/gif", "image/bmp",
            "image/webp", "image/tiff", "image/svg+xml"
        };
        return imageTypes.Contains(contentType);
    }

    // Optional: Get optimized URL for different file types
    public string GetOptimizedUrl(string publicId, string contentType)
    {
        if (IsVideoFile(contentType))
        {
            // For videos, you might want to return a thumbnail or optimized version
            var transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("jpg")
                .Height(300)
                .Width(400)
                .Crop("fill");

            return _cloudinary.Api.UrlVideoUp.Transform(transformation)
                .BuildUrl(publicId);
        }

        if (IsImageFile(contentType))
        {
            // Optimized image URL
            var transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto")
                .Width(1200)
                .Crop("limit");

            return _cloudinary.Api.UrlImgUp.Transform(transformation)
                .BuildUrl(publicId);
        }

        // For PDFs and other files, return the original URL
        return _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
    }

    // Optional: Get video thumbnail
    public string GetVideoThumbnailUrl(string publicId, int width = 400, int height = 300)
    {
        var transformation = new Transformation()
            .Quality("auto")
            .FetchFormat("jpg")
            .Height(height)
            .Width(width)
            .Crop("fill")
            .Gravity("auto:faces"); // Auto-detect faces for better thumbnails

        return _cloudinary.Api.UrlVideoUp.Transform(transformation)
            .BuildUrl(publicId);
    }
}

