using System.ComponentModel.DataAnnotations;

namespace TegWallet.Application.Features.Core.DocumentAttachment.Dto;

public class DeleteDocumentRequestDto
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public Guid DeletedBy { get; set; } // ClientId as Guid (required)
}