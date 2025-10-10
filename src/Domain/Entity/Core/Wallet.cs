using Transfer.Domain.Abstractions;
using Transfer.Domain.Entity.Enum;
using Transfer.Domain.Exceptions;
using Transfer.Domain.ValueObjects;

namespace Transfer.Domain.Entity.Core;

public class Wallet : Entity<Guid>
{
    public Guid ClientId { get; private set; }
    public Money Balance { get; private set; } = null!;
    public Money AvailableBalance { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Currency BaseCurrency { get; private set; } = null!;

    private readonly List<Transaction> _transactions = [];
    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    // Protected constructor for EF Core
    protected Wallet()
    {
    }

    public static Wallet Create(
        Guid clientId,
        Currency baseCurrency,
        DateTime? createdAt = null)
    {
        DomainGuards.AgainstDefault(clientId, nameof(clientId));
        DomainGuards.AgainstNull(baseCurrency, nameof(baseCurrency));

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            BaseCurrency = baseCurrency,
            Balance = new Money(0, baseCurrency),
            AvailableBalance = new Money(0, baseCurrency),
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = createdAt ?? DateTime.UtcNow
        };

        return wallet;
    }

    public Transaction Deposit(Money amount, string? reference = null, string? description = null)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));

        if (amount.Amount <= 0)
            throw new DomainException("Deposit amount must be positive");

        if (amount.Currency != BaseCurrency)
            throw new DomainException($"Deposit must be in base currency: {BaseCurrency.Code}");

        var transaction = Transaction.Create(
            walletId: Id,
            type: TransactionType.Deposit,
            amount: amount,
            status: TransactionStatus.Pending,
            reference: reference,
            description: description ?? $"Deposit of {amount.Amount} {amount.Currency.Code}"
        );

        _transactions.Add(transaction);
        Balance = Balance + amount;
        UpdatedAt = DateTime.UtcNow;

        //AddDomainEvent(new DepositInitiatedEvent(Id, ClientId, amount, transaction.Id));

        return transaction;
    }

    public void ApproveDeposit(TransactionId transactionId)
    {
        DomainGuards.AgainstNull(transactionId, nameof(transactionId));

        var transaction = _transactions.FirstOrDefault(t => t.Id == transactionId);
        if (transaction is null)
            throw new DomainException($"Transaction not found: {transactionId}");

        if (transaction.Type != TransactionType.Deposit)
            throw new DomainException("Only deposit transactions can be approved");

        if (!transaction.IsPending)
            throw new DomainException("Only pending deposits can be approved");

        //var previousAvailableBalance = AvailableBalance;

        transaction.MarkAsCompleted();
        AvailableBalance = AvailableBalance + transaction.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RejectDeposit(TransactionId transactionId, string reason)
    {
        DomainGuards.AgainstNull(transactionId, nameof(transactionId));
        DomainGuards.AgainstNullOrWhiteSpace(reason, nameof(reason));

        var transaction = _transactions.FirstOrDefault(t => t.Id == transactionId);
        if (transaction is null)
            throw new DomainException($"Transaction not found: {transactionId}");

        if (transaction.Type != TransactionType.Deposit)
            throw new DomainException("Only deposit transactions can be rejected");

        if (!transaction.IsPending)
            throw new DomainException("Only pending deposits can be rejected");

        //var previousBalance = Balance;

        transaction.MarkAsFailed(reason);
        Balance = Balance - transaction.Amount; // Reverse the balance
        UpdatedAt = DateTime.UtcNow;
    }

    public Transaction Withdraw(Money amount, string? description = null)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));

        if (amount.Amount <= 0)
            throw new DomainException("Withdrawal amount must be positive");

        if (amount.Currency != BaseCurrency)
            throw new DomainException($"Withdrawal must be in base currency: {BaseCurrency.Code}");

        if (AvailableBalance.Amount < amount.Amount)
            throw new DomainException($"Insufficient available balance. Available: {AvailableBalance.Amount}, Requested: {amount.Amount}");

        //var previousBalance = Balance;
        //var previousAvailableBalance = AvailableBalance;

        var transaction = Transaction.Create(
            walletId: Id,
            type: TransactionType.Withdrawal,
            amount: amount,
            status: TransactionStatus.Completed,
            description: description ?? $"Withdrawal of {amount.Amount} {amount.Currency.Code}"
        );

        _transactions.Add(transaction);
        Balance = Balance - amount;
        AvailableBalance = AvailableBalance - amount;
        UpdatedAt = DateTime.UtcNow;

        return transaction;
    }

    public Transaction Purchase(Money amount, string description, string supplierDetails)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));
        DomainGuards.AgainstNullOrWhiteSpace(supplierDetails, nameof(supplierDetails));

        if (amount.Amount <= 0)
            throw new DomainException("Purchase amount must be positive");

        if (AvailableBalance.Amount < amount.Amount)
            throw new DomainException($"Insufficient available balance. Available: {AvailableBalance.Amount}, Required: {amount.Amount}");

        //var previousBalance = Balance;
        //var previousAvailableBalance = AvailableBalance;

        var transaction = Transaction.Create(
            walletId: Id,
            type: TransactionType.Purchase,
            amount: amount,
            status: TransactionStatus.Completed,
            description: $"{description} - {supplierDetails}"
        );

        _transactions.Add(transaction);
        Balance = Balance - amount;
        AvailableBalance = AvailableBalance - amount;
        UpdatedAt = DateTime.UtcNow;

        return transaction;
    }

    public Transaction ChargeServiceFee(Money amount, string description)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));

        if (amount.Amount <= 0)
            throw new DomainException("Service fee amount must be positive");

        if (AvailableBalance.Amount < amount.Amount)
            throw new DomainException($"Insufficient available balance for service fee");

        //var previousBalance = Balance;
        //var previousAvailableBalance = AvailableBalance;
        var transaction = Transaction.Create(walletId: Id, type: TransactionType.ServiceFee, amount: amount,
            status: TransactionStatus.Completed, description: description);

        _transactions.Add(transaction);
        Balance = Balance - amount;
        AvailableBalance = AvailableBalance - amount;
        UpdatedAt = DateTime.UtcNow;

        return transaction;
    }

    public bool HasSufficientBalance(Money amount)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));
        return AvailableBalance.Amount >= amount.Amount && amount.Currency == AvailableBalance.Currency;
    }

    public bool HasSufficientBalanceForPurchase(Money purchaseAmount, Money serviceFee)
    {
        DomainGuards.AgainstNull(purchaseAmount, nameof(purchaseAmount));
        DomainGuards.AgainstNull(serviceFee, nameof(serviceFee));

        if (purchaseAmount.Currency != BaseCurrency || serviceFee.Currency != BaseCurrency)
            return false;

        var totalAmount = purchaseAmount.Amount + serviceFee.Amount;
        return AvailableBalance.Amount >= totalAmount;
    }

    public decimal GetAvailableBalance()
    {
        return AvailableBalance.Amount;
    }

    public decimal GetTotalBalance()
    {
        return Balance.Amount;
    }

    public decimal GetPendingBalance()
    {
        return Balance.Amount - AvailableBalance.Amount;
    }

    public IReadOnlyList<Transaction> GetPendingDeposits()
    {
        return _transactions
            .Where(t => t.Type == TransactionType.Deposit && t.IsPending)
            .ToList()
            .AsReadOnly();
    }

    public bool HasChanges(Wallet? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return false;

        return ClientId != other.ClientId ||
               !Balance.Equals(other.Balance) ||
               !AvailableBalance.Equals(other.AvailableBalance) ||
               !BaseCurrency.Equals(other.BaseCurrency);
    }
}

