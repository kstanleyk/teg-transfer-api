using MediatR;
using Microsoft.Extensions.Logging;
using TegWallet.Application.Features.Core.DocumentAttachment.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;

namespace TegWallet.Application.Features.Core.DocumentAttachment.Query;

public record GetLedgerAttachmentsQuery : IRequest<Result<IReadOnlyList<DocumentAttachmentDto>>>
{
    public Guid LedgerId { get; init; }
    public bool IncludeDeleted { get; init; } = false;
    public Guid ClientId { get; init; } // Client making the request
}

public class GetLedgerAttachmentsQueryHandler(
    IDocumentAttachmentRepository documentAttachmentRepository,
    ILedgerRepository ledgerRepository,
    IWalletRepository walletRepository,
    IClientRepository clientRepository,
    ILogger<GetLedgerAttachmentsQueryHandler> logger)
    : IRequestHandler<GetLedgerAttachmentsQuery, Result<IReadOnlyList<DocumentAttachmentDto>>>
{
    public async Task<Result<IReadOnlyList<DocumentAttachmentDto>>> Handle(
        GetLedgerAttachmentsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug(
                "Getting ledger attachments. LedgerId: {LedgerId}, ClientId: {ClientId}, IncludeDeleted: {IncludeDeleted}",
                query.LedgerId, query.ClientId, query.IncludeDeleted);

            // 1. Validate client exists and is active
            var client = await clientRepository.GetAsync(query.ClientId);
            if (client == null)
            {
                logger.LogWarning("Client not found for ledger attachments query. ClientId: {ClientId}", query.ClientId);
                return Result<IReadOnlyList<DocumentAttachmentDto>>.Failed("Client not found");
            }

            if (client.Status != ClientStatus.Active)
            {
                logger.LogWarning(
                    "Client account is not active for ledger attachments query. ClientId: {ClientId}, Status: {Status}",
                    query.ClientId, client.Status);
                return Result<IReadOnlyList<DocumentAttachmentDto>>.Failed("Your account is not active");
            }

            // 2. Validate ledger exists
            var ledger = await ledgerRepository.GetAsync(query.LedgerId);
            if (ledger == null)
            {
                logger.LogWarning("Ledger not found for attachments query. LedgerId: {LedgerId}", query.LedgerId);
                return Result<IReadOnlyList<DocumentAttachmentDto>>.Failed("Ledger not found");
            }

            // 3. Check if client has permission to view this ledger's attachments
            var hasPermission = await ClientHasPermissionToViewLedgerAttachmentsAsync(
                query.ClientId, ledger);

            if (!hasPermission)
            {
                logger.LogWarning(
                    "Client does not have permission to view ledger attachments. ClientId: {ClientId}, LedgerId: {LedgerId}",
                    query.ClientId, query.LedgerId);
                return Result<IReadOnlyList<DocumentAttachmentDto>>.Failed(
                    "You don't have permission to view documents for this transaction");
            }

            // 4. Get attachments based on IncludeDeleted flag
            IReadOnlyList<Domain.Entity.Core.DocumentAttachment> attachments;

            if (query.IncludeDeleted)
            {
                attachments = await documentAttachmentRepository.GetLedgerAttachmentsAsync(
                    query.LedgerId, cancellationToken);
            }
            else
            {
                attachments = await documentAttachmentRepository.GetActiveLedgerAttachmentsAsync(
                    query.LedgerId, cancellationToken);
            }

            logger.LogDebug(
                "Found {Count} attachments for ledger {LedgerId} (IncludeDeleted: {IncludeDeleted})",
                attachments.Count, query.LedgerId, query.IncludeDeleted);

            // 5. Map to DTOs
            var attachmentDtos = attachments.Select(MapToDto).ToList();

            // 6. Log successful query
            logger.LogInformation(
                "Successfully retrieved {Count} attachments for ledger {LedgerId} for client {ClientId}",
                attachmentDtos.Count, query.LedgerId, query.ClientId);

            return Result<IReadOnlyList<DocumentAttachmentDto>>.Succeeded(attachmentDtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error getting ledger attachments. LedgerId: {LedgerId}, ClientId: {ClientId}",
                query.LedgerId, query.ClientId);

            return Result<IReadOnlyList<DocumentAttachmentDto>>.Failed(
                "An error occurred while retrieving documents. Please try again.");
        }
    }

    private async Task<bool> ClientHasPermissionToViewLedgerAttachmentsAsync(
        Guid clientId,
        Ledger ledger)
    {
        try
        {
            logger.LogDebug(
                "Checking client permission to view ledger attachments. ClientId: {ClientId}, LedgerId: {LedgerId}",
                clientId, ledger.Id);

            // 1. Get the wallet that contains this ledger
            var wallet = await walletRepository.GetByLedgerIdAsync(ledger.Id);
            if (wallet == null)
            {
                logger.LogWarning("Wallet not found for ledger. LedgerId: {LedgerId}", ledger.Id);
                return false;
            }

            // 2. Check if the wallet belongs to the client
            if (wallet.ClientId != clientId)
            {
                logger.LogWarning(
                    "Client does not own the wallet containing this ledger. ClientId: {ClientId}, WalletClientId: {WalletClientId}",
                    clientId, wallet.ClientId);
                return false;
            }

            // 3. Additional checks if needed:
            // - Check if ledger type allows viewing attachments
            // - Check if client has specific view permissions

            logger.LogDebug("Client permission granted to view ledger attachments. ClientId: {ClientId}, LedgerId: {LedgerId}",
                clientId, ledger.Id);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error checking client permission to view ledger attachments. ClientId: {ClientId}, LedgerId: {LedgerId}",
                clientId, ledger.Id);
            return false;
        }
    }

    private static DocumentAttachmentDto MapToDto(Domain.Entity.Core.DocumentAttachment attachment)
    {
        return new DocumentAttachmentDto
        {
            Id = attachment.Id,
            EntityId = attachment.EntityId,
            EntityType = attachment.EntityType,
            FileName = attachment.FileName,
            FileUrl = attachment.FileUrl,
            CloudinaryPublicId = attachment.PublicId,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            FileCategory = attachment.FileCategory.ToString(),
            DocumentType = attachment.DocumentType,
            Description = attachment.Description,
            UploadedBy = attachment.UploadedBy,
            UploadedAt = attachment.UploadedAt,
            IsDeleted = attachment.IsDeleted,
            DeletedAt = attachment.DeletedAt,
            DeletedBy = attachment.DeletedBy,
            FileExtension = Path.GetExtension(attachment.FileName),
            FileSizeFormatted = FormatFileSize(attachment.FileSize),
            UploadedAtFormatted = attachment.UploadedAt.ToString("yyyy-MM-dd HH:mm")
        };
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bytes;
        var order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}