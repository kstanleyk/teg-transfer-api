using Transfer.Domain.Abstractions;
using Transfer.Domain.Entity.Enum;
using Transfer.Domain.Exceptions;
using Transfer.Domain.ValueObjects;

namespace Transfer.Domain.Entity.Core;

public class Transaction : Entity<TransactionId>
{
    public Guid WalletId { get; private init; }
    public TransactionType Type { get; private init; }
    public Money Amount { get; private init; } = null!;
    public TransactionStatus Status { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string FailureReason { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    // Protected constructor for EF Core
    protected Transaction()
    {
    }

    public static Transaction Create(Guid walletId, TransactionType type, Money amount, TransactionStatus status,
        string? reference = null, string? description = null, DateTime? timestamp = null)
    {
        DomainGuards.AgainstDefault(walletId, nameof(walletId));
        DomainGuards.AgainstNull(amount, nameof(amount));

        // Validate amount based on transaction type
        ValidateAmountForType(type, amount);

        // Validate status transitions (basic validation)
        ValidateInitialStatus(type, status);

        var transaction = new Transaction
        {
            Id = TransactionId.New(),
            WalletId = walletId,
            Type = type,
            Amount = amount,
            Status = status,
            Timestamp = timestamp ?? DateTime.UtcNow,
            Reference = reference?.Trim() ?? string.Empty,
            Description = description?.Trim() ?? GenerateDefaultDescription(type, amount)
        };

        return transaction;
    }

    public void MarkAsCompleted()
    {
        if (Status == TransactionStatus.Completed)
            return;

        if (Status != TransactionStatus.Pending)
            throw new DomainException($"Only pending transactions can be completed. Current status: {Status}");

        //var previousStatus = Status;
        Status = TransactionStatus.Completed;
    }

    public void MarkAsFailed(string reason)
    {
        DomainGuards.AgainstNullOrWhiteSpace(reason, nameof(reason));

        if (Status == TransactionStatus.Failed)
            return;

        if (Status == TransactionStatus.Completed)
            throw new DomainException("Cannot mark a completed transaction as failed");

        //var previousStatus = Status;
        Status = TransactionStatus.Failed;
        FailureReason = reason.Trim();
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

    public bool HasChanges(Transaction? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return WalletId != other.WalletId ||
               Type != other.Type ||
               !Amount.Equals(other.Amount) ||
               Status != other.Status ||
               Reference != other.Reference ||
               FailureReason != other.FailureReason ||
               Description != other.Description;
    }

    public bool IsPending => Status == TransactionStatus.Pending;
    public bool IsCompleted => Status == TransactionStatus.Completed;
    public bool IsFailed => Status == TransactionStatus.Failed;

    // Private validation methods
    private static void ValidateAmountForType(TransactionType type, Money amount)
    {
        if (amount.Amount <= 0)
            throw new DomainException("Transaction amount must be positive");

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
            _ => $"Transaction of {amount.Amount} {amount.Currency.Code}"
        };
    }
}
