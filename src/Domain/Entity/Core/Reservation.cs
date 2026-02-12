using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class Reservation : Entity<Guid>
{
    public Guid ClientId { get; private init; }
    public Guid WalletId { get; private init; }
    public Guid PurchaseLedgerId { get; private init; }
    public Guid ServiceFeeLedgerId { get; private init; }
    public Money PurchaseAmount { get; private init; }
    public Money ServiceFeeAmount { get; private init; }
    public Money TotalAmount { get; private init; }
    public string Description { get; private init; }
    public string SupplierDetails { get; private init; }
    public string PaymentMethod { get; private init; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? ProcessedBy { get; private set; }

    // Document attachments (not directly mapped in EF, loaded separately)
    private readonly List<DocumentAttachment> _attachments = [];
    public IReadOnlyList<DocumentAttachment> Attachments => _attachments.AsReadOnly();

    // Private constructor for EF Core
    protected Reservation()
    {
        PurchaseAmount = new Money(0, Currency.XAF);
        ServiceFeeAmount = new Money(0, Currency.XAF);
        TotalAmount = new Money(0, Currency.XAF);
        Description = string.Empty;
        SupplierDetails = string.Empty;
        PaymentMethod = string.Empty;
    }

    public static Reservation Create(
        Guid clientId,
        Guid walletId,
        Guid purchaseLedgerId,
        Guid serviceFeeLedgerId,
        Money purchaseAmount,
        Money serviceFeeAmount,
        string description,
        string supplierDetails,
        string paymentMethod)
    {
        DomainGuards.AgainstDefault(clientId, nameof(clientId));
        DomainGuards.AgainstDefault(walletId, nameof(walletId));
        DomainGuards.AgainstDefault(purchaseLedgerId, nameof(purchaseLedgerId));
        DomainGuards.AgainstDefault(serviceFeeLedgerId, nameof(serviceFeeLedgerId));
        DomainGuards.AgainstNull(purchaseAmount, nameof(purchaseAmount));
        DomainGuards.AgainstNull(serviceFeeAmount, nameof(serviceFeeAmount));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));
        DomainGuards.AgainstNullOrWhiteSpace(supplierDetails, nameof(supplierDetails));
        DomainGuards.AgainstNullOrWhiteSpace(paymentMethod, nameof(paymentMethod));

        if (purchaseAmount.Amount <= 0)
            throw new DomainException("Purchase amount must be positive");

        if (serviceFeeAmount.Amount < 0)
            throw new DomainException("Service fee amount cannot be negative");

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            WalletId = walletId,
            PurchaseLedgerId = purchaseLedgerId,
            ServiceFeeLedgerId = serviceFeeLedgerId,
            PurchaseAmount = purchaseAmount,
            ServiceFeeAmount = serviceFeeAmount,
            TotalAmount = purchaseAmount + serviceFeeAmount,
            Description = description.Trim(),
            SupplierDetails = supplierDetails.Trim(),
            PaymentMethod = paymentMethod.Trim(),
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        return reservation;
    }

    public static Reservation Hydrate(
        Guid id,
        Guid clientId,
        Guid walletId,
        Guid purchaseLedgerId,
        Guid serviceFeeLedgerId,
        Money purchaseAmount,
        Money serviceFeeAmount,
        Money totalAmount,
        string description,
        string supplierDetails,
        string paymentMethod,
        ReservationStatus status,
        DateTime createdAt,
        DateTime? completedAt,
        DateTime? cancelledAt,
        string? cancellationReason,
        string? processedBy,
        IEnumerable<DocumentAttachment>? attachments = null)
    {
        DomainGuards.AgainstDefault(id, nameof(id));
        DomainGuards.AgainstDefault(clientId, nameof(clientId));
        DomainGuards.AgainstDefault(walletId, nameof(walletId));
        DomainGuards.AgainstDefault(purchaseLedgerId, nameof(purchaseLedgerId));
        DomainGuards.AgainstDefault(serviceFeeLedgerId, nameof(serviceFeeLedgerId));
        DomainGuards.AgainstNull(purchaseAmount, nameof(purchaseAmount));
        DomainGuards.AgainstNull(serviceFeeAmount, nameof(serviceFeeAmount));
        DomainGuards.AgainstNull(totalAmount, nameof(totalAmount));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));
        DomainGuards.AgainstNullOrWhiteSpace(supplierDetails, nameof(supplierDetails));
        DomainGuards.AgainstNullOrWhiteSpace(paymentMethod, nameof(paymentMethod));

        var reservation = new Reservation
        {
            Id = id,
            ClientId = clientId,
            WalletId = walletId,
            PurchaseLedgerId = purchaseLedgerId,
            ServiceFeeLedgerId = serviceFeeLedgerId,
            PurchaseAmount = purchaseAmount,
            ServiceFeeAmount = serviceFeeAmount,
            TotalAmount = totalAmount,
            Description = description.Trim(),
            SupplierDetails = supplierDetails.Trim(),
            PaymentMethod = paymentMethod.Trim(),
            Status = status,
            CreatedAt = createdAt,
            CompletedAt = completedAt,
            CancelledAt = cancelledAt,
            CancellationReason = cancellationReason?.Trim(),
            ProcessedBy = processedBy?.Trim()
        };

        // Add attachments if provided
        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                // Validate attachment belongs to this reservation
                if (attachment.EntityId != id || attachment.EntityType != nameof(Reservation))
                    throw new DomainException($"Attachment {attachment.Id} does not belong to reservation {id}");

                reservation._attachments.Add(attachment);
            }
        }

        return reservation;
    }

    public void Complete(string processedBy = "ADMIN")
    {
        if (Status != ReservationStatus.Pending)
            throw new DomainException("Only pending reservations can be completed");

        Status = ReservationStatus.Completed;
        ProcessedBy = processedBy;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason, string cancelledBy = "ADMIN")
    {
        DomainGuards.AgainstNullOrWhiteSpace(reason, nameof(reason));
        DomainGuards.AgainstNullOrWhiteSpace(cancelledBy, nameof(cancelledBy));

        if (Status != ReservationStatus.Pending)
            throw new DomainException("Only pending reservations can be cancelled");

        Status = ReservationStatus.Cancelled;
        CancellationReason = reason.Trim();
        ProcessedBy = cancelledBy;
        CancelledAt = DateTime.UtcNow;
    }

    public DocumentAttachment AddAttachment(
        string fileName,
        string fileUrl,
        string cloudinaryPublicId,
        string contentType,
        long fileSize,
        string documentType,
        string description,
        string uploadedBy)
    {
        DomainGuards.AgainstNullOrWhiteSpace(uploadedBy, nameof(uploadedBy));
        DomainGuards.AgainstNullOrWhiteSpace(fileName, nameof(fileName));
        DomainGuards.AgainstNullOrWhiteSpace(fileUrl, nameof(fileUrl));
        DomainGuards.AgainstNullOrWhiteSpace(cloudinaryPublicId, nameof(cloudinaryPublicId));
        DomainGuards.AgainstNullOrWhiteSpace(contentType, nameof(contentType));
        DomainGuards.AgainstNullOrWhiteSpace(documentType, nameof(documentType));

        // Can only add attachments when reservation is pending
        if (!IsPending)
            throw new DomainException("Can only add attachments to pending reservations");

        var attachment = DocumentAttachment.Create(
            entityId: Id,
            entityType: nameof(Reservation),
            fileName: fileName,
            fileUrl: fileUrl,
            publicId: cloudinaryPublicId,
            contentType: contentType,
            fileSize: fileSize,
            documentType: documentType,
            description: description,
            uploadedBy: uploadedBy);

        _attachments.Add(attachment);
        return attachment;
    }

    public void RemoveAttachment(Guid attachmentId, string removedBy, string reason)
    {
        DomainGuards.AgainstNullOrWhiteSpace(removedBy, nameof(removedBy));
        DomainGuards.AgainstNullOrWhiteSpace(reason, nameof(reason));

        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment == null)
            throw new DomainException($"Attachment not found: {attachmentId}");

        // Can only remove attachments when reservation is pending
        if (!IsPending)
            throw new DomainException("Can only remove attachments from pending reservations");

        attachment.MarkAsDeleted(removedBy, reason);
    }

    public void AddExistingAttachment(DocumentAttachment attachment)
    {
        DomainGuards.AgainstNull(attachment, nameof(attachment));

        // Validate attachment belongs to this reservation
        if (attachment.EntityId != Id || attachment.EntityType != nameof(Reservation))
            throw new DomainException($"Attachment {attachment.Id} does not belong to reservation {Id}");

        // Don't add if already exists
        if (_attachments.Any(a => a.Id == attachment.Id))
            return;

        _attachments.Add(attachment);
    }

    public bool HasAttachment(Guid attachmentId)
    {
        return _attachments.Any(a => a.Id == attachmentId && !a.IsDeleted);
    }

    public IEnumerable<DocumentAttachment> GetActiveAttachments()
    {
        return _attachments.Where(a => !a.IsDeleted);
    }

    public bool CanBeCompleted => Status == ReservationStatus.Pending;
    public bool CanBeCancelled => Status == ReservationStatus.Pending;
    public bool IsPending => Status == ReservationStatus.Pending;
    public bool IsCompleted => Status == ReservationStatus.Completed;
    public bool IsCancelled => Status == ReservationStatus.Cancelled;
}

public enum ReservationStatus
{
    Pending = 1,
    Completed = 2,
    Cancelled = 3
}