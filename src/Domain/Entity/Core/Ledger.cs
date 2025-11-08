using System.Runtime.CompilerServices;
using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class Ledger : TegWallet.Domain.Abstractions.Entity<Guid>
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
        DomainGuards.AgainstDefault<Guid>(walletId, nameof(walletId));
        DomainGuards.AgainstNull<Money>(amount, nameof(amount));
        Ledger.ValidateAmountForType(type, amount);
        Ledger.ValidateInitialStatus(type, status);
        Ledger ledger = new Ledger();
        ledger.Id = SequentialId.CreateUnique().Value;
        ledger.WalletId = walletId;
        ledger.Type = type;
        ledger.Amount = amount;
        ledger.Status = status;
        ledger.Timestamp = timestamp ?? DateTime.UtcNow;
        ledger.Reference = reference?.Trim() ?? string.Empty;
        ledger.Description = description?.Trim() ?? Ledger.GenerateDefaultDescription(type, amount);
        ledger.ReservationId = purchaseReservationId;
        return ledger;
    }

    public static Ledger Hydrate(
      Guid walletId,
      TransactionType type,
      Money amount,
      TransactionStatus status,
      string failureReason,
      string completionType,
      string completedBy,
      DateTime? completedAt,
      string? reference = null,
      string? description = null,
      DateTime? timestamp = null,
      Guid? purchaseReservationId = null)
    {
        DomainGuards.AgainstDefault<Guid>(walletId, nameof(walletId));
        DomainGuards.AgainstNull<Money>(amount, nameof(amount));
        Ledger ledger = new Ledger();
        ledger.Id = SequentialId.CreateUnique().Value;
        ledger.WalletId = walletId;
        ledger.Type = type;
        ledger.Amount = amount;
        ledger.Status = status;
        ledger.Timestamp = timestamp ?? DateTime.UtcNow;
        ledger.Reference = reference?.Trim() ?? string.Empty;
        ledger.Description = description?.Trim() ?? Ledger.GenerateDefaultDescription(type, amount);
        ledger.ReservationId = purchaseReservationId;
        ledger.FailureReason = failureReason;
        ledger.CompletionType = completionType;
        ledger.CompletedBy = completedBy;
        ledger.CompletedAt = completedAt;
        return ledger;
    }

    public void MarkAsCompleted(string completionType, string completedBy = "SYSTEM")
    {
        if (this.Status == TransactionStatus.Completed)
            return;
        if (this.Status != TransactionStatus.Pending)
        {
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 1);
            interpolatedStringHandler.AppendLiteral("Only pending transactions can be completed. Current status: ");
            interpolatedStringHandler.AppendFormatted<TransactionStatus>(this.Status);
            throw new DomainException(interpolatedStringHandler.ToStringAndClear());
        }
        this.Status = TransactionStatus.Completed;
        this.CompletionType = completionType;
        this.CompletedBy = completedBy;
        this.CompletedAt = new DateTime?(DateTime.UtcNow);
    }

    public void MarkAsFailed(string reason, string rejectedBy = "SYSTEM")
    {
        DomainGuards.AgainstNullOrWhiteSpace(reason, nameof(reason));
        DomainGuards.AgainstNullOrWhiteSpace(rejectedBy, nameof(rejectedBy));
        if (this.Status == TransactionStatus.Failed)
            return;
        this.Status = this.Status != TransactionStatus.Completed ? TransactionStatus.Failed : throw new DomainException("Cannot mark a completed transaction as failed");
        this.FailureReason = reason.Trim();
        this.CompletionType = CompletionTypes.Rejection;
        this.CompletedBy = rejectedBy;
        this.CompletedAt = new DateTime?(DateTime.UtcNow);
    }

    public void UpdateReference(string? reference)
    {
        if (this.Status != TransactionStatus.Pending)
            throw new DomainException("Can only update reference for pending transactions");
        this.Reference = reference?.Trim() ?? string.Empty;
    }

    public void UpdateDescription(string? description)
    {
        if (this.Status != TransactionStatus.Pending)
            throw new DomainException("Can only update description for pending transactions");
        this.Description = description?.Trim() ?? string.Empty;
    }

    public void HydrateFields(
      string? failureReason,
      string? completionType,
      string? completedBy,
      DateTime? completedAt)
    {
        if (!string.IsNullOrEmpty(failureReason))
            this.FailureReason = failureReason;
        if (!string.IsNullOrEmpty(completionType))
            this.CompletionType = completionType;
        if (!string.IsNullOrEmpty(completedBy))
            this.CompletedBy = completedBy;
        if (!completedAt.HasValue)
            return;
        this.CompletedAt = completedAt;
    }

    public bool CanTransitionTo(TransactionStatus newStatus)
    {
        bool flag;
        if (this.Status == TransactionStatus.Pending)
        {
            switch (newStatus)
            {
                case TransactionStatus.Pending:
                    flag = true;
                    goto label_6;
                case TransactionStatus.Completed:
                    flag = true;
                    goto label_6;
                case TransactionStatus.Failed:
                    flag = true;
                    goto label_6;
            }
        }
        flag = false;
    label_6:
        return flag;
    }

    public bool IsPending => this.Status == TransactionStatus.Pending;

    public bool IsCompleted => this.Status == TransactionStatus.Completed;

    public bool IsFailed => this.Status == TransactionStatus.Failed;

    private static void ValidateAmountForType(TransactionType type, Money amount)
    {
        if (amount.Amount <= 0M)
            throw new DomainException("Ledger amount must be positive");
        Console.WriteLine(amount.Amount);
        switch (type)
        {
            case TransactionType.Deposit:
            case TransactionType.Withdrawal:
            case TransactionType.Purchase:
            case TransactionType.ServiceFee:
                break;
            default:
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
                interpolatedStringHandler.AppendLiteral("Unsupported transaction type: ");
                interpolatedStringHandler.AppendFormatted<TransactionType>(type);
                throw new DomainException(interpolatedStringHandler.ToStringAndClear());
        }
    }

    private static void ValidateInitialStatus(TransactionType type, TransactionStatus status)
    {
        switch (type)
        {
            case TransactionType.Deposit:
                if (status == TransactionStatus.Pending || status == TransactionStatus.Completed)
                    break;
                throw new DomainException("RequestDeposit transactions must start as Pending or Completed");
            case TransactionType.Withdrawal:
            case TransactionType.Purchase:
            case TransactionType.ServiceFee:
                if (status == TransactionStatus.Completed || status == TransactionStatus.Pending)
                    break;
                DefaultInterpolatedStringHandler interpolatedStringHandler1 = new DefaultInterpolatedStringHandler(48, 1);
                interpolatedStringHandler1.AppendFormatted<TransactionType>(type);
                interpolatedStringHandler1.AppendLiteral(" transactions must start as Pending or Completed");
                throw new DomainException(interpolatedStringHandler1.ToStringAndClear());
            default:
                DefaultInterpolatedStringHandler interpolatedStringHandler2 = new DefaultInterpolatedStringHandler(30, 1);
                interpolatedStringHandler2.AppendLiteral("Unsupported transaction type: ");
                interpolatedStringHandler2.AppendFormatted<TransactionType>(type);
                throw new DomainException(interpolatedStringHandler2.ToStringAndClear());
        }
    }

    private static string GenerateDefaultDescription(TransactionType type, Money amount)
    {
        string stringAndClear;
        switch (type)
        {
            case TransactionType.Deposit:
                DefaultInterpolatedStringHandler interpolatedStringHandler1 = new DefaultInterpolatedStringHandler(19, 2);
                interpolatedStringHandler1.AppendLiteral("RequestDeposit of ");
                interpolatedStringHandler1.AppendFormatted<Decimal>(amount.Amount);
                interpolatedStringHandler1.AppendLiteral(" ");
                interpolatedStringHandler1.AppendFormatted(amount.Currency.Code);
                stringAndClear = interpolatedStringHandler1.ToStringAndClear();
                break;
            case TransactionType.Withdrawal:
                DefaultInterpolatedStringHandler interpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 2);
                interpolatedStringHandler2.AppendLiteral("Withdrawal of ");
                interpolatedStringHandler2.AppendFormatted<Decimal>(amount.Amount);
                interpolatedStringHandler2.AppendLiteral(" ");
                interpolatedStringHandler2.AppendFormatted(amount.Currency.Code);
                stringAndClear = interpolatedStringHandler2.ToStringAndClear();
                break;
            case TransactionType.Purchase:
                DefaultInterpolatedStringHandler interpolatedStringHandler3 = new DefaultInterpolatedStringHandler(14, 2);
                interpolatedStringHandler3.AppendLiteral("Purchase for ");
                interpolatedStringHandler3.AppendFormatted<Decimal>(amount.Amount);
                interpolatedStringHandler3.AppendLiteral(" ");
                interpolatedStringHandler3.AppendFormatted(amount.Currency.Code);
                stringAndClear = interpolatedStringHandler3.ToStringAndClear();
                break;
            case TransactionType.ServiceFee:
                DefaultInterpolatedStringHandler interpolatedStringHandler4 = new DefaultInterpolatedStringHandler(16, 2);
                interpolatedStringHandler4.AppendLiteral("Service fee of ");
                interpolatedStringHandler4.AppendFormatted<Decimal>(amount.Amount);
                interpolatedStringHandler4.AppendLiteral(" ");
                interpolatedStringHandler4.AppendFormatted(amount.Currency.Code);
                stringAndClear = interpolatedStringHandler4.ToStringAndClear();
                break;
            default:
                DefaultInterpolatedStringHandler interpolatedStringHandler5 = new DefaultInterpolatedStringHandler(11, 2);
                interpolatedStringHandler5.AppendLiteral("Ledger of ");
                interpolatedStringHandler5.AppendFormatted<Decimal>(amount.Amount);
                interpolatedStringHandler5.AppendLiteral(" ");
                interpolatedStringHandler5.AppendFormatted(amount.Currency.Code);
                stringAndClear = interpolatedStringHandler5.ToStringAndClear();
                break;
        }
        return stringAndClear;
    }
}
