using MediatR;
using Microsoft.Extensions.Logging;
using TegWallet.Application.Features.Core.DocumentAttachment.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;

namespace TegWallet.Application.Features.Core.DocumentAttachment.Query;

public record GetDocumentByIdQuery : IRequest<Result<DocumentAttachmentDto>>
{
    public Guid EntityId { get; init; }
    public string EntityType { get; init; } = string.Empty; // "Ledger" or "Reservation"
    public Guid AttachmentId { get; init; }
    public Guid ClientId { get; init; } // Client making the request
}

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, Result<DocumentAttachmentDto>>
{
    private readonly IDocumentAttachmentRepository _documentAttachmentRepository;
    private readonly ILedgerRepository _ledgerRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<GetDocumentByIdQueryHandler> _logger;

    public GetDocumentByIdQueryHandler(
        IDocumentAttachmentRepository documentAttachmentRepository,
        ILedgerRepository ledgerRepository,
        IReservationRepository reservationRepository,
        IWalletRepository walletRepository,
        IClientRepository clientRepository,
        ILogger<GetDocumentByIdQueryHandler> logger)
    {
        _documentAttachmentRepository = documentAttachmentRepository;
        _ledgerRepository = ledgerRepository;
        _reservationRepository = reservationRepository;
        _walletRepository = walletRepository;
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task<Result<DocumentAttachmentDto>> Handle(
        GetDocumentByIdQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug(
                "Getting document by ID. EntityType: {EntityType}, EntityId: {EntityId}, AttachmentId: {AttachmentId}, ClientId: {ClientId}",
                query.EntityType, query.EntityId, query.AttachmentId, query.ClientId);

            // 1. Validate client exists and is active
            var client = await _clientRepository.GetAsync(query.ClientId);
            if (client == null)
            {
                _logger.LogWarning("Client not found for document query. ClientId: {ClientId}", query.ClientId);
                return Result<DocumentAttachmentDto>.Failed("Client not found");
            }

            if (client.Status != ClientStatus.Active)
            {
                _logger.LogWarning(
                    "Client account is not active for document query. ClientId: {ClientId}, Status: {Status}",
                    query.ClientId, client.Status);
                return Result<DocumentAttachmentDto>.Failed("Your account is not active");
            }

            // 2. Get the attachment
            Domain.Entity.Core.DocumentAttachment? attachment = null;

            if (query.EntityType == nameof(Ledger))
            {
                attachment = await HandleLedgerDocumentQuery(query, client, cancellationToken);
            }
            else if (query.EntityType == nameof(Reservation))
            {
                attachment = await HandleReservationDocumentQuery(query, client, cancellationToken);
            }
            else
            {
                _logger.LogWarning("Invalid entity type for document query: {EntityType}", query.EntityType);
                return Result<DocumentAttachmentDto>.Failed($"Invalid entity type. Must be '{nameof(Ledger)}' or '{nameof(Reservation)}'");
            }

            if (attachment == null)
            {
                _logger.LogWarning(
                    "Document not found or permission denied. EntityType: {EntityType}, EntityId: {EntityId}, AttachmentId: {AttachmentId}, ClientId: {ClientId}",
                    query.EntityType, query.EntityId, query.AttachmentId, query.ClientId);
                return Result<DocumentAttachmentDto>.Failed("Document not found or you don't have permission to view it");
            }

            // 3. Map to DTO
            var dto = MapToDto(attachment);

            // 4. Log successful query
            _logger.LogInformation(
                "Successfully retrieved document. AttachmentId: {AttachmentId}, EntityType: {EntityType}, EntityId: {EntityId} for client {ClientId}",
                attachment.Id, query.EntityType, query.EntityId, query.ClientId);

            return Result<DocumentAttachmentDto>.Succeeded(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting document by ID. EntityType: {EntityType}, EntityId: {EntityId}, AttachmentId: {AttachmentId}, ClientId: {ClientId}",
                query.EntityType, query.EntityId, query.AttachmentId, query.ClientId);

            return Result<DocumentAttachmentDto>.Failed(
                "An error occurred while retrieving the document. Please try again.");
        }
    }

    private async Task<Domain.Entity.Core.DocumentAttachment?> HandleLedgerDocumentQuery(
        GetDocumentByIdQuery query,
        Client client,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get the attachment
            var attachment = await _documentAttachmentRepository.GetLedgerAttachmentByIdAsync(
                query.EntityId, query.AttachmentId, cancellationToken);

            if (attachment == null)
            {
                _logger.LogDebug("Ledger document not found. LedgerId: {LedgerId}, AttachmentId: {AttachmentId}",
                    query.EntityId, query.AttachmentId);
                return null;
            }

            // 2. Validate ledger exists
            var ledger = await _ledgerRepository.GetAsync(query.EntityId);
            if (ledger == null)
            {
                _logger.LogWarning("Ledger not found for document query. LedgerId: {LedgerId}", query.EntityId);
                return null;
            }

            // 3. Check if client has permission to view this document
            var hasPermission = await ClientHasPermissionToViewLedgerDocumentAsync(
                query.ClientId, ledger, attachment, cancellationToken);

            if (!hasPermission)
            {
                _logger.LogWarning(
                    "Client does not have permission to view ledger document. ClientId: {ClientId}, LedgerId: {LedgerId}, AttachmentId: {AttachmentId}",
                    query.ClientId, query.EntityId, query.AttachmentId);
                return null;
            }

            return attachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling ledger document query. LedgerId: {LedgerId}, ClientId: {ClientId}",
                query.EntityId, query.ClientId);
            throw;
        }
    }

    private async Task<Domain.Entity.Core.DocumentAttachment?> HandleReservationDocumentQuery(
        GetDocumentByIdQuery query,
        Client client,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get the attachment
            var attachment = await _documentAttachmentRepository.GetReservationAttachmentByIdAsync(
                query.EntityId, query.AttachmentId, cancellationToken);

            if (attachment == null)
            {
                _logger.LogDebug("Reservation document not found. ReservationId: {ReservationId}, AttachmentId: {AttachmentId}",
                    query.EntityId, query.AttachmentId);
                return null;
            }

            // 2. Validate reservation exists
            var reservation = await _reservationRepository.GetAsync(query.EntityId);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation not found for document query. ReservationId: {ReservationId}", query.EntityId);
                return null;
            }

            // 3. Check if client has permission to view this document
            var hasPermission = await ClientHasPermissionToViewReservationDocumentAsync(
                query.ClientId, reservation, attachment, cancellationToken);

            if (!hasPermission)
            {
                _logger.LogWarning(
                    "Client does not have permission to view reservation document. ClientId: {ClientId}, ReservationId: {ReservationId}, AttachmentId: {AttachmentId}",
                    query.ClientId, query.EntityId, query.AttachmentId);
                return null;
            }

            return attachment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling reservation document query. ReservationId: {ReservationId}, ClientId: {ClientId}",
                query.EntityId, query.ClientId);
            throw;
        }
    }

    private async Task<bool> ClientHasPermissionToViewLedgerDocumentAsync(
        Guid clientId,
        Ledger ledger,
        Domain.Entity.Core.DocumentAttachment attachment,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get the wallet that contains this ledger
            var wallet = await _walletRepository.GetByLedgerIdAsync(ledger.Id);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found for ledger. LedgerId: {LedgerId}", ledger.Id);
                return false;
            }

            // 2. Check if the wallet belongs to the client
            if (wallet.ClientId != clientId)
            {
                _logger.LogWarning(
                    "Client does not own the wallet containing this ledger. ClientId: {ClientId}, WalletClientId: {WalletClientId}",
                    clientId, wallet.ClientId);
                return false;
            }

            // 3. Additional checks:
            // - Check if document is deleted and should be hidden
            // - Check if ledger type has special viewing rules

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking client permission to view ledger document. ClientId: {ClientId}, LedgerId: {LedgerId}",
                clientId, ledger.Id);
            return false;
        }
    }

    private async Task<bool> ClientHasPermissionToViewReservationDocumentAsync(
        Guid clientId,
        Reservation reservation,
        Domain.Entity.Core.DocumentAttachment attachment,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Check if the client owns this reservation
            if (reservation.ClientId != clientId)
            {
                _logger.LogWarning(
                    "Client does not own this reservation. ClientId: {ClientId}, ReservationClientId: {ReservationClientId}",
                    clientId, reservation.ClientId);
                return false;
            }

            // 2. Additional checks:
            // - Check if document is deleted and should be hidden
            // - Check if reservation has special viewing rules
            // - Check if document type is restricted

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking client permission to view reservation document. ClientId: {ClientId}, ReservationId: {ReservationId}",
                clientId, reservation.Id);
            return false;
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

    private string FormatFileSize(long bytes)
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