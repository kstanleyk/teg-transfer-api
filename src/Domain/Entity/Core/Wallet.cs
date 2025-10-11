using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class Wallet : Entity<Guid>
{
    public Guid ClientId { get; private init; }
    public Money Balance { get; private set; }
    public Money AvailableBalance { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Currency BaseCurrency { get; private init; }

    private readonly List<Ledger> _ledgerEntries = [];
    public IReadOnlyList<Ledger> LedgerEntries => _ledgerEntries.AsReadOnly();

    // Protected constructor for EF Core - properly initialize all owned entities
    protected Wallet()
    {
        // Initialize with default values that EF Core can work with
        // These will be overwritten when EF Core hydrates the entity
        BaseCurrency = Currency.XOF;
        Balance = new Money(0, BaseCurrency);
        AvailableBalance = new Money(0, BaseCurrency);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Wallet Create(Guid clientId, Currency baseCurrency, DateTime? createdAt = null)
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

    public Ledger Deposit(Money amount, string? reference = null, string? description = null)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));

        if (amount.Amount <= 0)
            throw new DomainException("Deposit amount must be positive");

        if (amount.Currency != BaseCurrency)
            throw new DomainException($"Deposit must be in base currency: {BaseCurrency.Code}");

        var ledger = Ledger.Create(walletId: Id, type: TransactionType.Deposit, amount: amount,
            status: TransactionStatus.Pending, reference: reference,
            description: description ?? $"Deposit of {amount.Amount} {amount.Currency.Code}");

        _ledgerEntries.Add(ledger);
        Balance = Balance + amount;
        UpdatedAt = DateTime.UtcNow;

        return ledger;
    }

    public void ApproveDeposit(LedgerId ledgerId, string approvedBy = "SYSTEM")
    {
        DomainGuards.AgainstNull(ledgerId, nameof(ledgerId));
        DomainGuards.AgainstNullOrWhiteSpace(approvedBy, nameof(approvedBy));

        var ledger = _ledgerEntries.FirstOrDefault(t => t.Id == ledgerId);
        if (ledger is null)
            throw new DomainException($"Ledger not found: {ledgerId}");

        if (ledger.Type != TransactionType.Deposit)
            throw new DomainException("Only deposit transactions can be approved");

        if (!ledger.IsPending)
            throw new DomainException("Only pending deposits can be approved");

        // Update the ledger status
        ledger.MarkAsCompleted();

        // Update wallet balances
        AvailableBalance = AvailableBalance + ledger.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RejectDeposit(LedgerId ledgerId, string reason, string rejectedBy = "SYSTEM")
    {
        DomainGuards.AgainstNull(ledgerId, nameof(ledgerId));
        DomainGuards.AgainstNullOrWhiteSpace(reason, nameof(reason));
        DomainGuards.AgainstNullOrWhiteSpace(rejectedBy, nameof(rejectedBy));

        var ledger = _ledgerEntries.FirstOrDefault(t => t.Id == ledgerId);
        if (ledger is null)
            throw new DomainException($"Ledger not found: {ledgerId}");

        if (ledger.Type != TransactionType.Deposit)
            throw new DomainException("Only deposit transactions can be rejected");

        if (!ledger.IsPending)
            throw new DomainException("Only pending deposits can be rejected");

        // Update the ledger status with rejection details
        ledger.MarkAsFailed(reason, rejectedBy);

        // Reverse the balance (since deposit added to balance when created)
        Balance = Balance - ledger.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public Ledger Withdraw(Money amount, string? description = null)
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

        var transaction = Ledger.Create(
            walletId: Id,
            type: TransactionType.Withdrawal,
            amount: amount,
            status: TransactionStatus.Completed,
            description: description ?? $"Withdrawal of {amount.Amount} {amount.Currency.Code}"
        );

        _ledgerEntries.Add(transaction);
        Balance = Balance - amount;
        AvailableBalance = AvailableBalance - amount;
        UpdatedAt = DateTime.UtcNow;

        return transaction;
    }

    public Ledger Purchase(Money amount, string description, string supplierDetails)
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

        var transaction = Ledger.Create(
            walletId: Id,
            type: TransactionType.Purchase,
            amount: amount,
            status: TransactionStatus.Completed,
            description: $"{description} - {supplierDetails}"
        );

        _ledgerEntries.Add(transaction);
        Balance = Balance - amount;
        AvailableBalance = AvailableBalance - amount;
        UpdatedAt = DateTime.UtcNow;

        return transaction;
    }

    public Ledger ChargeServiceFee(Money amount, string description)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));

        if (amount.Amount <= 0)
            throw new DomainException("Service fee amount must be positive");

        if (AvailableBalance.Amount < amount.Amount)
            throw new DomainException($"Insufficient available balance for service fee");

        //var previousBalance = Balance;
        //var previousAvailableBalance = AvailableBalance;
        var transaction = Ledger.Create(walletId: Id, type: TransactionType.ServiceFee, amount: amount,
            status: TransactionStatus.Completed, description: description);

        _ledgerEntries.Add(transaction);
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

    public IReadOnlyList<Ledger> GetPendingDeposits()
    {
        return _ledgerEntries
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

