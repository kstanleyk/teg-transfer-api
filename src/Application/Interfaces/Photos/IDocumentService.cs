using Microsoft.AspNetCore.Http;
using CloudinaryDotNet.Actions;

namespace TegWallet.Application.Interfaces.Photos;

public interface IDocumentService
{
    Task<UploadResult?> UploadDocument(IFormFile file);
    Task<DeletionResult> DeleteDocument(string publicId);
}