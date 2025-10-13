using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Entity.Enum;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class Wallet : Aggregate<Guid>
{
    public Guid ClientId { get; private init; }
    public Money Balance { get; private set; }
    public Money AvailableBalance { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Currency BaseCurrency { get; private init; }

    private readonly List<Ledger> _ledgers = [];
    public IReadOnlyList<Ledger> Ledgers => _ledgers.AsReadOnly();

    private readonly List<Reservation> _reservations = [];
    public IReadOnlyList<Reservation> Reservations => _reservations.AsReadOnly();

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

    public Ledger RequestDeposit(Money amount, string? reference = null, string? description = null)
    {
        DomainGuards.AgainstNull(amount, nameof(amount));

        if (amount.Amount <= 0)
            throw new DomainException("RequestDeposit amount must be positive");

        if (amount.Currency != BaseCurrency)
            throw new DomainException($"RequestDeposit must be in base currency: {BaseCurrency.Code}");

        var ledger = Ledger.Create(walletId: Id, type: TransactionType.Deposit, amount: amount,
            status: TransactionStatus.Pending, reference: reference,
            description: description ?? $"RequestDeposit of {amount.Amount} {amount.Currency.Code}");

        _ledgers.Add(ledger);
        Balance = Balance + amount;
        UpdatedAt = DateTime.UtcNow;

        return ledger;
    }

    public void ApproveDeposit(Guid ledgerId, string approvedBy = "SYSTEM")
    {
        DomainGuards.AgainstNull(ledgerId, nameof(ledgerId));
        DomainGuards.AgainstNullOrWhiteSpace(approvedBy, nameof(approvedBy));

        var ledger = _ledgers.FirstOrDefault(t => t.Id == ledgerId);
        if (ledger is null)
            throw new DomainException($"Ledger not found: {ledgerId}");

        if (ledger.Type != TransactionType.Deposit)
            throw new DomainException("Only deposit transactions can be approved");

        if (!ledger.IsPending)
            throw new DomainException("Only pending deposits can be approved");

        // Update the ledger status
        ledger.MarkAsCompleted(CompletionTypes.Approval, approvedBy);

        // Update wallet balances
        AvailableBalance = AvailableBalance + ledger.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RejectDeposit(Guid ledgerId, string reason, string rejectedBy = "SYSTEM")
    {
        DomainGuards.AgainstNull(ledgerId, nameof(ledgerId));
        DomainGuards.AgainstNullOrWhiteSpace(reason, nameof(reason));
        DomainGuards.AgainstNullOrWhiteSpace(rejectedBy, nameof(rejectedBy));

        var ledger = _ledgers.FirstOrDefault(t => t.Id == ledgerId);
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

    public Ledger RequestWithdrawal(Money amount, string? description = null)
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
            status: TransactionStatus.Pending,
            description: description ?? $"Withdrawal of {amount.Amount} {amount.Currency.Code}"
        );

        _ledgers.Add(transaction);
        AvailableBalance = AvailableBalance - amount;
        UpdatedAt = DateTime.UtcNow;

        return transaction;
    }

    public void ApproveWithdrawal(Guid ledgerId, string approvedBy = "SYSTEM")
    {
        DomainGuards.AgainstNull(ledgerId, nameof(ledgerId));
        DomainGuards.AgainstNullOrWhiteSpace(approvedBy, nameof(approvedBy));

        var ledger = _ledgers.FirstOrDefault(t => t.Id == ledgerId);
        if (ledger is null)
            throw new DomainException($"Ledger not found: {ledgerId}");

        if (ledger.Type != TransactionType.Withdrawal)
            throw new DomainException("Only withdrawal transactions can be approved");

        if (!ledger.IsPending)
            throw new DomainException("Only pending withdrawals can be approved");

        // Update the ledger status
        ledger.MarkAsCompleted(CompletionTypes.Approval, approvedBy);

        // Update wallet balances
        Balance = Balance - ledger.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RejectWithdrawal(Guid ledgerId, string reason, string rejectedBy = "SYSTEM")
    {
        DomainGuards.AgainstNull(ledgerId, nameof(ledgerId));
        DomainGuards.AgainstNullOrWhiteSpace(reason, nameof(reason));
        DomainGuards.AgainstNullOrWhiteSpace(rejectedBy, nameof(rejectedBy));

        var ledger = _ledgers.FirstOrDefault(t => t.Id == ledgerId);
        if (ledger is null)
            throw new DomainException($"Ledger not found: {ledgerId}");

        if (ledger.Type != TransactionType.Withdrawal)
            throw new DomainException("Only withdrawal transactions can be rejected");

        if (!ledger.IsPending)
            throw new DomainException("Only pending withdrawals can be rejected");

        // Update the ledger status with rejection details
        ledger.MarkAsFailed(reason, rejectedBy);

        // Reverse the balance (since withdrawal request subtracted from the balance when created)
        AvailableBalance = AvailableBalance + ledger.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public (Reservation reservation, Ledger purchaseLedger, Ledger serviceFeeLedger)
            ReserveForPurchase(Money purchaseAmount, Money serviceFee, string description,
                string supplierDetails, string paymentMethod)
    {
        DomainGuards.AgainstNull(purchaseAmount, nameof(purchaseAmount));
        DomainGuards.AgainstNull(serviceFee, nameof(serviceFee));
        DomainGuards.AgainstNullOrWhiteSpace(description, nameof(description));
        DomainGuards.AgainstNullOrWhiteSpace(supplierDetails, nameof(supplierDetails));
        DomainGuards.AgainstNullOrWhiteSpace(paymentMethod, nameof(paymentMethod));

        if (purchaseAmount.Amount <= 0)
            throw new DomainException("Purchase amount must be positive");

        if (serviceFee.Amount < 0)
            throw new DomainException("Service fee amount cannot be negative");

        var totalAmount = purchaseAmount + serviceFee;

        if (AvailableBalance.Amount < totalAmount.Amount)
            throw new DomainException($"Insufficient available balance. Available: {AvailableBalance.Amount}, Required: {totalAmount.Amount}");

        // Generate ledger IDs first
        var purchaseLedgerId = SequentialId.CreateUnique().Value;
        var serviceFeeLedgerId = SequentialId.CreateUnique().Value;

        // Create purchase reservation
        var reservation = Reservation.Create(
            clientId: ClientId,
            walletId: Id,
            purchaseLedgerId: purchaseLedgerId,
            serviceFeeLedgerId: serviceFeeLedgerId,
            purchaseAmount: purchaseAmount,
            serviceFeeAmount: serviceFee,
            description: description,
            supplierDetails: supplierDetails,
            paymentMethod: paymentMethod);

        // Create purchase ledger with reservation reference
        var purchaseLedger = Ledger.Create(
            walletId: Id,
            type: TransactionType.Purchase,
            amount: purchaseAmount,
            status: TransactionStatus.Pending,
            description: $"{description} - {supplierDetails} - Payment: {paymentMethod}",
            reference: $"PAYMENT_METHOD:{paymentMethod}",
            purchaseReservationId: reservation.Id);

        purchaseLedger.SetId(purchaseLedgerId);

        // Create service fee ledger with reservation reference
        var serviceFeeLedger = Ledger.Create(
            walletId: Id,
            type: TransactionType.ServiceFee,
            amount: serviceFee,
            status: TransactionStatus.Pending,
            description: $"Service fee for {description} - Payment: {paymentMethod}",
            purchaseReservationId: reservation.Id);

        serviceFeeLedger.SetId(serviceFeeLedgerId);

        _ledgers.Add(purchaseLedger);
        _ledgers.Add(serviceFeeLedger);
        _reservations.Add(reservation);

        // Reserve funds by deducting from available balance
        AvailableBalance = AvailableBalance - totalAmount;
        UpdatedAt = DateTime.UtcNow;

        return (reservation, purchaseLedger, serviceFeeLedger);
    }

    public void CompletePurchase(Guid reservationId, string processedBy = "ADMIN")
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation == null)
            throw new DomainException($"Purchase reservation not found: {reservationId}");

        var purchaseLedger = _ledgers.FirstOrDefault(t => t.Id == reservation.PurchaseLedgerId);
        var serviceFeeLedger = _ledgers.FirstOrDefault(t => t.Id == reservation.ServiceFeeLedgerId);

        if (purchaseLedger == null || serviceFeeLedger == null)
            throw new DomainException("One or both ledger entries not found for reservation");

        if (!purchaseLedger.IsPending || !serviceFeeLedger.IsPending)
            throw new DomainException("Can only complete pending transactions");

        if (!reservation.CanBeCompleted)
            throw new DomainException("Reservation cannot be completed in its current state");

        // Mark both transactions as completed
        purchaseLedger.MarkAsCompleted(CompletionTypes.Processed, processedBy);
        serviceFeeLedger.MarkAsCompleted(CompletionTypes.Processed, processedBy);

        // Complete the reservation
        reservation.Complete(processedBy);

        // Deduct from main balance (funds were already reserved from available balance)
        Balance = Balance - purchaseLedger.Amount - serviceFeeLedger.Amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CancelPurchase(Guid reservationId, string reason, string cancelledBy = "ADMIN")
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation == null)
            throw new DomainException($"Purchase reservation not found: {reservationId}");

        var purchaseLedger = _ledgers.FirstOrDefault(t => t.Id == reservation.PurchaseLedgerId);
        var serviceFeeLedger = _ledgers.FirstOrDefault(t => t.Id == reservation.ServiceFeeLedgerId);

        if (purchaseLedger == null || serviceFeeLedger == null)
            throw new DomainException("One or both ledger entries not found for reservation");

        if (!purchaseLedger.IsPending || !serviceFeeLedger.IsPending)
            throw new DomainException("Can only cancel pending transactions");

        if (!reservation.CanBeCancelled)
            throw new DomainException("Reservation cannot be cancelled in its current state");

        // Mark both transactions as failed
        purchaseLedger.MarkAsFailed(reason, cancelledBy);
        serviceFeeLedger.MarkAsFailed(reason, cancelledBy);

        // Cancel the reservation
        reservation.Cancel(reason, cancelledBy);

        // Return reserved funds to available balance
        var totalAmount = purchaseLedger.Amount + serviceFeeLedger.Amount;
        AvailableBalance = AvailableBalance + totalAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    public Reservation? GetPurchaseReservation(Guid reservationId)
    {
        return _reservations.FirstOrDefault(r => r.Id == reservationId);
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

        _ledgers.Add(transaction);
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
}

//public static class DepositRejectionReasons
//{
//    public const string BankTransferNotVerified = "Bank transfer could not be verified";
//    public const string InsufficientFunds = "Insufficient funds in source account";
//    public const string SuspiciousActivity = "Suspicious activity detected";
//    public const string InvalidReference = "Invalid or missing reference number";
//    public const string AccountFrozen = "Source account is frozen or restricted";
//    public const string DocumentationRequired = "Additional documentation required";
//    public const string AmountMismatch = "Amount does not match expected value";
//    public const string DuplicateTransaction = "Duplicate transaction detected";

//    public static readonly string[] AllReasons =
//    [
//        BankTransferNotVerified,
//        InsufficientFunds,
//        SuspiciousActivity,
//        InvalidReference,
//        AccountFrozen,
//        DocumentationRequired,
//        AmountMismatch,
//        DuplicateTransaction
//    ];
//}