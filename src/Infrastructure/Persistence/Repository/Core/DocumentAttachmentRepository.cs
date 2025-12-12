using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class DocumentAttachmentRepository(IDatabaseFactory databaseFactory)
    : DataRepository<DocumentAttachment, Guid>(databaseFactory), IDocumentAttachmentRepository
{
    // ===== Ledger Document Methods =====

    public async Task<DocumentAttachment?> GetLedgerAttachmentByIdAsync(Guid ledgerId, Guid attachmentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.Id == ledgerId
                && a.EntityType == nameof(Ledger)
                && a.Id == attachmentId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentAttachment>> GetLedgerAttachmentsAsync(Guid ledgerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.Id == ledgerId
                && a.EntityType == nameof(Ledger))
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentAttachment>> GetActiveLedgerAttachmentsAsync(Guid ledgerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.Id == ledgerId
                && a.EntityType == nameof(Ledger)
                && !a.IsDeleted)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentAttachment> AddLedgerAttachmentAsync(Guid ledgerId, DocumentAttachment attachment, CancellationToken cancellationToken = default)
    {
        // Verify ledger exists and is pending
        var isLedgerPending = await IsLedgerPendingAsync(ledgerId, cancellationToken);
        if (!isLedgerPending)
            throw new DomainException($"Cannot add attachment to non-pending ledger: {ledgerId}");

        // Ensure attachment belongs to this ledger
        if (attachment.Id != ledgerId || attachment.EntityType != nameof(Ledger))
            throw new DomainException($"Attachment does not belong to ledger: {ledgerId}");

        DbSet.Add(attachment);
        await Context.SaveChangesAsync(cancellationToken);

        return attachment;
    }

    public async Task RemoveLedgerAttachmentAsync(Guid ledgerId, Guid attachmentId, string deletedBy, string reason, CancellationToken cancellationToken = default)
    {
        var attachment = await GetLedgerAttachmentByIdAsync(ledgerId, attachmentId, cancellationToken);
        if (attachment == null)
            throw new DomainException($"Attachment not found: {attachmentId} for ledger: {ledgerId}");

        var isLedgerPending = await IsLedgerPendingAsync(ledgerId, cancellationToken);
        if (!isLedgerPending)
            throw new DomainException($"Cannot remove attachment from non-pending ledger: {ledgerId}");

        attachment.MarkAsDeleted(deletedBy, reason);

        DbSet.Update(attachment);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> LedgerAttachmentExistsAsync(Guid ledgerId, Guid attachmentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(a => a.Id == ledgerId
                && a.EntityType == nameof(Ledger)
                && a.Id == attachmentId
                && !a.IsDeleted, cancellationToken);
    }

    public async Task<int> GetLedgerAttachmentCountAsync(Guid ledgerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .CountAsync(a => a.Id == ledgerId
                && a.EntityType == nameof(Ledger)
                && !a.IsDeleted, cancellationToken);
    }


    // ===== Reservation Document Methods =====

    public async Task<DocumentAttachment?> GetReservationAttachmentByIdAsync(Guid reservationId, Guid attachmentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.Id == reservationId
                && a.EntityType == nameof(Reservation)
                && a.Id == attachmentId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentAttachment>> GetReservationAttachmentsAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.Id == reservationId
                && a.EntityType == nameof(Reservation))
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentAttachment>> GetActiveReservationAttachmentsAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.Id == reservationId
                && a.EntityType == nameof(Reservation)
                && !a.IsDeleted)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentAttachment> AddReservationAttachmentAsync(Guid reservationId, DocumentAttachment attachment, CancellationToken cancellationToken = default)
    {
        // Verify reservation exists and is pending
        var isReservationPending = await IsReservationPendingAsync(reservationId, cancellationToken);
        if (!isReservationPending)
            throw new DomainException($"Cannot add attachment to non-pending reservation: {reservationId}");

        // Ensure attachment belongs to this reservation
        if (attachment.Id != reservationId || attachment.EntityType != nameof(Reservation))
            throw new DomainException($"Attachment does not belong to reservation: {reservationId}");

        DbSet.Add(attachment);
        await Context.SaveChangesAsync(cancellationToken);

        return attachment;
    }

    public async Task RemoveReservationAttachmentAsync(Guid reservationId, Guid attachmentId, string deletedBy, string reason, CancellationToken cancellationToken = default)
    {
        var attachment = await GetReservationAttachmentByIdAsync(reservationId, attachmentId, cancellationToken);
        if (attachment == null)
            throw new DomainException($"Attachment not found: {attachmentId} for reservation: {reservationId}");

        var isReservationPending = await IsReservationPendingAsync(reservationId, cancellationToken);
        if (!isReservationPending)
            throw new DomainException($"Cannot remove attachment from non-pending reservation: {reservationId}");

        attachment.MarkAsDeleted(deletedBy, reason);

        DbSet.Update(attachment);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ReservationAttachmentExistsAsync(Guid reservationId, Guid attachmentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(a => a.Id == reservationId
                && a.EntityType == nameof(Reservation)
                && a.Id == attachmentId
                && !a.IsDeleted, cancellationToken);
    }

    public async Task<int> GetReservationAttachmentCountAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .CountAsync(a => a.Id == reservationId
                && a.EntityType == nameof(Reservation)
                && !a.IsDeleted, cancellationToken);
    }

    // ===== Batch Operations =====

    public async Task<IReadOnlyList<DocumentAttachment>> GetAttachmentsForMultipleLedgersAsync(IEnumerable<Guid> ledgerIds, CancellationToken cancellationToken = default)
    {
        var ledgerIdList = ledgerIds.ToList();
        if (!ledgerIdList.Any())
            return new List<DocumentAttachment>();

        return await DbSet
            .Where(a => ledgerIdList.Contains(a.Id)
                && a.EntityType == nameof(Ledger)
                && !a.IsDeleted)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentAttachment>> GetAttachmentsForMultipleReservationsAsync(IEnumerable<Guid> reservationIds, CancellationToken cancellationToken = default)
    {
        var reservationIdList = reservationIds.ToList();
        if (!reservationIdList.Any())
            return new List<DocumentAttachment>();

        return await DbSet
            .Where(a => reservationIdList.Contains(a.Id)
                && a.EntityType == nameof(Reservation)
                && !a.IsDeleted)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    // ===== Utility Methods =====

    public async Task<bool> CanAddAttachmentToLedgerAsync(Guid ledgerId, CancellationToken cancellationToken = default)
    {
        return await IsLedgerPendingAsync(ledgerId, cancellationToken);
    }

    public async Task<bool> CanAddAttachmentToReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await IsReservationPendingAsync(reservationId, cancellationToken);
    }

    public async Task<bool> IsLedgerPendingAsync(Guid ledgerId, CancellationToken cancellationToken = default)
    {
        var ledger = await Context.LedgerSet
            .Where(l => l.Id == ledgerId)
            .Select(l => new { l.Status })
            .FirstOrDefaultAsync(cancellationToken);

        return ledger?.Status == TransactionStatus.Pending;
    }

    public async Task<bool> IsReservationPendingAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await Context.ReservationSet
            .Where(r => r.Id == reservationId)
            .Select(r => new { r.Status })
            .FirstOrDefaultAsync(cancellationToken);

        return reservation?.Status == ReservationStatus.Pending;
    }

    public async Task<int> CountClientUploadsTodayAsync(
        string clientId,
        DateTime todayStart,
        DateTime tomorrowStart,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await DbSet
                .Where(a => a.UploadedBy == clientId
                            && a.UploadedAt >= todayStart
                            && a.UploadedAt < tomorrowStart
                            && !a.IsDeleted)
                .CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex,
            //    "Error counting client uploads today. ClientId: {ClientId}, Date: {Date}",
            //    clientId, todayStart);
            throw;
        }
    }

    public async Task<int> CountClientReservationUploadsTodayAsync(
        string clientId,
        DateTime todayStart,
        DateTime tomorrowStart,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Join DocumentAttachments with Reservations to count only reservation uploads
            return await Context.DocumentAttachmentSet
                .Where(a => a.EntityType == nameof(Reservation)
                            && a.UploadedBy == clientId
                            && a.UploadedAt >= todayStart
                            && a.UploadedAt < tomorrowStart
                            && !a.IsDeleted)
                .Join(Context.ReservationSet,
                    attachment => attachment.EntityId,
                    reservation => reservation.Id,
                    (attachment, reservation) => new { attachment, reservation })
                .Where(x => x.reservation.ClientId.ToString() == clientId) // Double-check client owns reservation
                .CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex,
            //    "Error counting client reservation uploads today. ClientId: {ClientId}, Date: {Date}",
            //    clientId, todayStart);
            throw;
        }
    }
}