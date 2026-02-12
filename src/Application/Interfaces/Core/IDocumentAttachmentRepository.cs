using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Interfaces.Core;

public interface IDocumentAttachmentRepository : IRepository<DocumentAttachment, Guid>
{
    Task<DocumentAttachment?> GetLedgerAttachmentByIdAsync(Guid ledgerId, Guid attachmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentAttachment>> GetLedgerAttachmentsAsync(Guid ledgerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentAttachment>> GetActiveLedgerAttachmentsAsync(Guid ledgerId, CancellationToken cancellationToken = default);
    Task<DocumentAttachment> AddLedgerAttachmentAsync(Guid ledgerId, DocumentAttachment attachment, CancellationToken cancellationToken = default);
    Task RemoveLedgerAttachmentAsync(Guid ledgerId, Guid attachmentId, string deletedBy, string reason, CancellationToken cancellationToken = default);
    Task<bool> LedgerAttachmentExistsAsync(Guid ledgerId, Guid attachmentId, CancellationToken cancellationToken = default);
    Task<int> GetLedgerAttachmentCountAsync(Guid ledgerId, CancellationToken cancellationToken = default);
    Task<DocumentAttachment?> GetReservationAttachmentByIdAsync(Guid reservationId, Guid attachmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentAttachment>> GetReservationAttachmentsAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentAttachment>> GetActiveReservationAttachmentsAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<DocumentAttachment> AddReservationAttachmentAsync(Guid reservationId, DocumentAttachment attachment, CancellationToken cancellationToken = default);
    Task RemoveReservationAttachmentAsync(Guid reservationId, Guid attachmentId, string deletedBy, string reason, CancellationToken cancellationToken = default);
    Task<bool> ReservationAttachmentExistsAsync(Guid reservationId, Guid attachmentId, CancellationToken cancellationToken = default);
    Task<int> GetReservationAttachmentCountAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentAttachment>> GetAttachmentsForMultipleLedgersAsync(IEnumerable<Guid> ledgerIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentAttachment>> GetAttachmentsForMultipleReservationsAsync(IEnumerable<Guid> reservationIds, CancellationToken cancellationToken = default);
    Task<bool> CanAddAttachmentToLedgerAsync(Guid ledgerId, CancellationToken cancellationToken = default);
    Task<bool> CanAddAttachmentToReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<bool> IsLedgerPendingAsync(Guid ledgerId, CancellationToken cancellationToken = default);
    Task<bool> IsReservationPendingAsync(Guid reservationId, CancellationToken cancellationToken = default);

    Task<int> CountClientUploadsTodayAsync(
        string clientId,
        DateTime todayStart,
        DateTime tomorrowStart,
        CancellationToken cancellationToken = default);

    Task<int> CountClientReservationUploadsTodayAsync(
        string clientId,
        DateTime todayStart,
        DateTime tomorrowStart,
        CancellationToken cancellationToken = default);
}