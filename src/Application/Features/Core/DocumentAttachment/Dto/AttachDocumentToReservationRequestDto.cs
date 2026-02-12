using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TegWallet.Application.Features.Core.DocumentAttachment.Dto;

public class AttachDocumentToReservationRequestDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    public Guid UploadedBy { get; set; } // ClientId as Guid

    [StringLength(500)]
    public string? Description { get; set; }
}