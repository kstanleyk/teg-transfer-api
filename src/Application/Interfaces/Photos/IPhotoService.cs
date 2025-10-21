using Microsoft.AspNetCore.Http;
using CloudinaryDotNet.Actions;

namespace TegWallet.Application.Interfaces.Photos;

public interface IPhotoService
{
    Task<UploadResult?> UploadPhoto(IFormFile file);
    Task<DeletionResult> DeletePhoto(string publicId);
}