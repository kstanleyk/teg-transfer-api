using System.Runtime.CompilerServices;
using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class Ledger : Entity<Guid>
{
    public Guid WalletId { get; private set; }
    public TransactionType Type { get; private set; }
    public Money Amount { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string FailureReason { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string CompletionType { get; private set; } = string.Empty;
    public DateTime? CompletedAt { get; private set; }
    public string CompletedBy { get; private set; } = string.Empty;
    public Guid? ReservationId { get; private set; }

    // Document attachments (not directly mapped in EF, loaded separately)
    private readonly List<DocumentAttachment> _attachments = [];
    public IReadOnlyList<DocumentAttachment> Attachments => _attachments.AsReadOnly();

    protected Ledger()
    {
    }

    public static Ledger Create(
        Guid walletId,
        TransactionType type,
        Money amount,
        TransactionStatus status,
        string? reference = null,
        string? description = null,
        DateTime? timestamp = null,
        Guid? purchaseReservationId = null)
    {
        DomainGuards.AgainstDefault(walletId, nameof(walletId));
        DomainGuards.AgainstNull(amount, nameof(amount));
        ValidateAmountForType(type, amount);
        ValidateInitialStatus(type, status);

        var ledger = new Ledger
        {
            Id = SequentialId.CreateUnique().Value,
            WalletId = walletId,
            Type = type,
            Amount = amount,
            Status = status,
            Timestamp = timestamp ?? DateTime.UtcNow,
            Reference = reference?.Trim() ?? string.Empty,
            Description = description?.Trim() ?? GenerateDefaultDescription(type, amount),
            ReservationId = purchaseReservationId
        };

        return ledger;
    }

    public static Ledger Hydrate(
        Guid id,
        Guid walletId,
        TransactionType type,
        Money amount,
        TransactionStatus status,
        DateTime timestamp,
        string reference,
        string description,
        string failureReason,
        string completionType,
        string completedBy,
        DateTime? completedAt,
        Guid? reservationId,
        IEnumerable<DocumentAttachment>? attachments = null)
    {
        DomainGuards.AgainstDefault(id, nameof(id));
        DomainGuards.AgainstDefault(walletId, nameof(walletId));
        DomainGuards.AgainstNull(amount, nameof(amount));

        var ledger = new Ledger
        {
            Id = id,
            WalletId = walletId,
            Type = type,
            Amount = amount,
            Status = status,
            Timestamp = timestamp,
            Reference = reference.Trim(),
            Description = description.Trim(),
            FailureReason = failureReason.Trim(),
            CompletionType = completionType.Trim(),
            CompletedBy = completedBy.Trim(),
            CompletedAt = completedAt,
            ReservationId = reservationId
        };

        // Add attachments if provided
        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                // Validate attachment belongs to this ledger
                if (attachment.EntityId != id || attachment.EntityType != nameof(Ledger))
                    throw new DomainException($"Attachment {attachment.Id} does not belong to ledger {id}");

                ledger._attachments.Add(attachment);
            }
        }

        return ledger;
    }

    public void MarkAsCompleted(string completionType, string completedBy = "SYSTEM")
    {
        if (Status == TransactionStatus.Completed)
            return;

        if (Status != TransactionStatus.Pending)
            throw new DomainException($"Only pending transactions can be completed. Current status: {Status}");

        Status = TransactionStatus.Completed;
        CompletionType = completionType;
        CompletedBy = completedBy;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason, string rejectedBy = "SYSTEM")
    {
        DomainGuards.AgainstNullOrWhiteSpace(reason, nameof(reason));
        DomainGuards.AgainstNullOrWhiteSpace(rejectedBy, nameof(rejectedBy));

        if (Status == TransactionStatus.Failed)
            return;

        if (Status == TransactionStatus.Completed)
            throw new DomainException("Cannot mark a completed transaction as failed");

        Status = TransactionStatus.Failed;
        FailureReason = reason.Trim();

        CompletionType = CompletionTypes.Rejection;
        CompletedBy = rejectedBy;
        CompletedAt = DateTime.UtcNow;
    }

    public void UpdateReference(string? reference)
    {
        if (Status != TransactionStatus.Pending)
            throw new DomainException("Can only update reference for pending transactions");

        Reference = reference?.Trim() ?? string.Empty;
    }

    public void UpdateDescription(string? description)
    {
        if (Status != TransactionStatus.Pending)
            throw new DomainException("Can only update description for pending transactions");

        Description = description?.Trim() ?? string.Empty;
    }

    public void HydrateFields(
        string? failureReason,
        string? completionType,
        string? completedBy,
        DateTime? completedAt)
    {
        if (!string.IsNullOrEmpty(failureReason))
            FailureReason = failureReason.Trim();

        if (!string.IsNullOrEmpty(completionType))
            CompletionType = completionType.Trim();

        if (!string.IsNullOrEmpty(completedBy))
            CompletedBy = completedBy.Trim();

        if (completedAt.HasValue)
            CompletedAt = completedAt;
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

        // Can only add attachments when ledger is pending
        if (!IsPending)
            throw new DomainException("Can only add attachments to pending ledger entries");

        var attachment = DocumentAttachment.Create(
            entityId: Id,
            entityType: nameof(Ledger),
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

        // Can only remove attachments when ledger is pending
        if (!IsPending)
            throw new DomainException("Can only remove attachments from pending ledger entries");

        attachment.MarkAsDeleted(removedBy, reason);
    }

    public void AddExistingAttachment(DocumentAttachment attachment)
    {
        DomainGuards.AgainstNull(attachment, nameof(attachment));

        // Validate attachment belongs to this ledger
        if (attachment.EntityId != Id || attachment.EntityType != nameof(Ledger))
            throw new DomainException($"Attachment {attachment.Id} does not belong to ledger {Id}");

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

    public bool CanTransitionTo(TransactionStatus newStatus)
    {
        return (Status, newStatus) switch
        {
            (TransactionStatus.Pending, TransactionStatus.Completed) => true,
            (TransactionStatus.Pending, TransactionStatus.Failed) => true,
            (TransactionStatus.Pending, TransactionStatus.Pending) => true,
            _ => false
        };
    }

    public bool IsPending => Status == TransactionStatus.Pending;
    public bool IsCompleted => Status == TransactionStatus.Completed;
    public bool IsFailed => Status == TransactionStatus.Failed;

    // Private validation methods
    private static void ValidateAmountForType(TransactionType type, Money amount)
    {
        if (amount.Amount <= 0M)
            throw new DomainException("Ledger amount must be positive");

        // Additional validation based on transaction type
        switch (type)
        {
            case TransactionType.Deposit:
            case TransactionType.Withdrawal:
            case TransactionType.Purchase:
            case TransactionType.ServiceFee:
                // These types are fine with positive amounts
                break;
            default:
                throw new DomainException($"Unsupported transaction type: {type}");
        }
    }

    private static void ValidateInitialStatus(TransactionType type, TransactionStatus status)
    {
        // Validate that the initial status makes sense for the transaction type
        switch (type)
        {
            case TransactionType.Deposit:
                if (status != TransactionStatus.Pending && status != TransactionStatus.Completed)
                    throw new DomainException("Deposit transactions must start as Pending or Completed");
                break;
            case TransactionType.Withdrawal:
            case TransactionType.Purchase:
            case TransactionType.ServiceFee:
                if (status != TransactionStatus.Completed && status != TransactionStatus.Pending)
                    throw new DomainException($"{type} transactions must start as Pending or Completed");
                break;
            default:
                throw new DomainException($"Unsupported transaction type: {type}");
        }
    }

    private static string GenerateDefaultDescription(TransactionType type, Money amount)
    {
        return type switch
        {
            TransactionType.Deposit => $"Deposit of {amount.Amount} {amount.Currency.Code}",
            TransactionType.Withdrawal => $"Withdrawal of {amount.Amount} {amount.Currency.Code}",
            TransactionType.Purchase => $"Purchase for {amount.Amount} {amount.Currency.Code}",
            TransactionType.ServiceFee => $"Service fee of {amount.Amount} {amount.Currency.Code}",
            _ => $"Transaction of {amount.Amount} {amount.Currency.Code}"
        };
    }
}
