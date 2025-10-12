using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class Ledger : Entity<Guid>
{
    public Guid WalletId { get; private init; }
    public TransactionType Type { get; private init; }
    public Money Amount { get; private init; } = null!;
    public TransactionStatus Status { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string FailureReason { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string ApprovedBy { get; private set; } = string.Empty;
    public string CompletionType { get; private set; } = string.Empty;
    public DateTime? CompletedAt { get; private set; }
    public string CompletedBy { get; private set; } = string.Empty;
    public DateTime? ApprovedAt { get; private set; }
    public string RejectedBy { get; private set; } = string.Empty;
    public DateTime? RejectedAt { get; private set; }
    public string ProcessedBy { get; private set; } = string.Empty;
    public DateTime? ProcessedAt { get; private set; }
    public Guid? ReservationId { get; private set; } 

    // Protected constructor for EF Core
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
        Guid? purchaseReservationId = null) // New optional parameter
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

    public void MarkAsCompleted(CompletionTypes completionType,  string completedBy = "SYSTEM")
    {
        if (Status == TransactionStatus.Completed)
            return;

        if (Status != TransactionStatus.Pending)
            throw new DomainException($"Only pending transactions can be completed. Current status: {Status}");

        //var previousStatus = Status;
        Status = TransactionStatus.Completed;
        ApprovedBy = completedBy;
        ApprovedAt = DateTime.UtcNow;

        CompletionType = nameof(completionType);
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
        RejectedBy = rejectedBy;
        RejectedAt = DateTime.UtcNow;

        CompletionType = nameof(CompletionTypes.Rejection);
        CompletedBy = rejectedBy;
        CompletedAt = DateTime.UtcNow;
    }

    public void UpdateReference(string? reference)
    {
        if (Status != TransactionStatus.Pending)
            throw new DomainException("Can only update reference for pending transactions");

        //var oldReference = Reference;
        Reference = reference?.Trim() ?? string.Empty;
    }

    public void UpdateDescription(string? description)
    {
        if (Status != TransactionStatus.Pending)
            throw new DomainException("Can only update description for pending transactions");

        //var oldDescription = Description;
        Description = description?.Trim() ?? string.Empty;
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
        if (amount.Amount <= 0)
            throw new DomainException("Ledger amount must be positive");

        Console.WriteLine(amount.Amount);

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
            _ => $"Ledger of {amount.Amount} {amount.Currency.Code}"
        };
    }
}
