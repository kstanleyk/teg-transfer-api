using MediatR;
using Microsoft.Extensions.Logging;
using TegWallet.Application.Features.Core.DocumentAttachment.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;

namespace TegWallet.Application.Features.Core.DocumentAttachment.Query;

public record GetReservationAttachmentsQuery : IRequest<Result<IReadOnlyList<DocumentAttachmentDto>>>
{
    public Guid ReservationId { get; init; }
    public bool IncludeDeleted { get; init; } = false;
    public Guid ClientId { get; init; } 
}

public class GetReservationAttachmentsQueryHandler(
    IDocumentAttachmentRepository documentAttachmentRepository,
    IReservationRepository reservationRepository,
    IClientRepository clientRepository,
    ILogger<GetReservationAttachmentsQueryHandler> logger)
    : IRequestHandler<GetReservationAttachmentsQuery, Result<IReadOnlyList<DocumentAttachmentDto>>>
{
    public async Task<Result<IReadOnlyList<DocumentAttachmentDto>>> Handle(
        GetReservationAttachmentsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug(
                "Getting reservation attachments. ReservationId: {ReservationId}, ClientId: {ClientId}, IncludeDeleted: {IncludeDeleted}",
                query.ReservationId, query.ClientId, query.IncludeDeleted);

            // 1. Validate client exists and is active
            var client = await clientRepository.GetAsync(query.ClientId);
            if (client == null)
            {
                logger.LogWarning("Client not found for reservation attachments query. ClientId: {ClientId}", query.ClientId);
                return Result<IReadOnlyList<DocumentAttachmentDto>>.Failed("Client not found");
            }

            if (client.Status != ClientStatus.Active)
            {
                logger.LogWarning(
                    "Client account is not active for reservation attachments query. ClientId: {ClientId}, Status: {Status}",
                    query.ClientId, client.Status);
                return Result<IReadOnlyList<DocumentAttachmentDto>>.Failed("Your account is not active");
            }

            // 2. Validate reservation exists
            var reservation = await reservationRepository.GetAsync(query.ReservationId);
            if (reservation == null)
            {
                logger.LogWarning("Reservation not found for attachments query. ReservationId: {ReservationId}", query.ReservationId);
                return Result<IReadOnlyList<DocumentAttachmentDto>>.Failed("Reservation not found");
            }

            // 3. Check if client has permission to view this reservation's attachments
            var hasPermission = await ClientHasPermissionToViewReservationAttachmentsAsync(
                query.ClientId, reservation);

            if (!hasPermission)
            {
                logger.LogWarning(
                    "Client does not have permission to view reservation attachments. ClientId: {ClientId}, ReservationId: {ReservationId}",
                    query.ClientId, query.ReservationId);
                return Result<IReadOnlyList<DocumentAttachmentDto>>.Failed(
                    "You don't have permission to view documents for this reservation");
            }

            // 4. Get attachments based on IncludeDeleted flag
            IReadOnlyList<Domain.Entity.Core.DocumentAttachment> attachments;

            if (query.IncludeDeleted)
            {
                attachments = await documentAttachmentRepository.GetReservationAttachmentsAsync(
                    query.ReservationId, cancellationToken);
            }
            else
            {
                attachments = await documentAttachmentRepository.GetActiveReservationAttachmentsAsync(
                    query.ReservationId, cancellationToken);
            }

            logger.LogDebug(
                "Found {Count} attachments for reservation {ReservationId} (IncludeDeleted: {IncludeDeleted})",
                attachments.Count, query.ReservationId, query.IncludeDeleted);

            // 5. Map to DTOs
            var attachmentDtos = attachments.Select(MapToDto).ToList();

            // 6. Log successful query
            logger.LogInformation(
                "Successfully retrieved {Count} attachments for reservation {ReservationId} for client {ClientId}",
                attachmentDtos.Count, query.ReservationId, query.ClientId);

            return Result<IReadOnlyList<DocumentAttachmentDto>>.Succeeded(attachmentDtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error getting reservation attachments. ReservationId: {ReservationId}, ClientId: {ClientId}",
                query.ReservationId, query.ClientId);

            return Result<IReadOnlyList<DocumentAttachmentDto>>.Failed(
                "An error occurred while retrieving documents. Please try again.");
        }
    }

    private Task<bool> ClientHasPermissionToViewReservationAttachmentsAsync(
        Guid clientId,
        Reservation reservation)
    {
        try
        {
            logger.LogDebug(
                "Checking client permission to view reservation attachments. ClientId: {ClientId}, ReservationId: {ReservationId}",
                clientId, reservation.Id);

            // 1. Check if the client owns this reservation
            if (reservation.ClientId != clientId)
            {
                logger.LogWarning(
                    "Client does not own this reservation. ClientId: {ClientId}, ReservationClientId: {ReservationClientId}",
                    clientId, reservation.ClientId);
                return Task.FromResult(false);
            }

            // 2. Additional checks if needed:
            // - Check if reservation status allows viewing attachments
            // - Check if reservation type has special viewing rules

            logger.LogDebug("Client permission granted to view reservation attachments. ClientId: {ClientId}, ReservationId: {ReservationId}",
                clientId, reservation.Id);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error checking client permission to view reservation attachments. ClientId: {ClientId}, ReservationId: {ReservationId}",
                clientId, reservation.Id);
            return Task.FromResult(false);
        }
    }

    private DocumentAttachmentDto MapToDto(Domain.Entity.Core.DocumentAttachment attachment)
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
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}