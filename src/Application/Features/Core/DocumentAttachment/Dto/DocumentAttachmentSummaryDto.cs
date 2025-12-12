namespace TegWallet.Application.Features.Core.DocumentAttachment.Dto;

public record DocumentAttachmentSummaryDto
{
    public int TotalCount { get; init; }
    public int ActiveCount { get; init; }
    public int DeletedCount { get; init; }
    public IReadOnlyList<DocumentAttachmentDto> Attachments { get; init; } = new List<DocumentAttachmentDto>();
}