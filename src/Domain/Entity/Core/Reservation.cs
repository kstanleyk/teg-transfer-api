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

    // Private constructor for EF Core
    protected Reservation()
    {
        PurchaseAmount  = new Money(0, Currency.XOF);
        ServiceFeeAmount = new Money(0, Currency.XOF);
        TotalAmount = new Money(0, Currency.XOF);
        Description = string.Empty;
        SupplierDetails = string.Empty;
        PaymentMethod = string.Empty;
    }

    public static Reservation Create(Guid clientId, Guid walletId, Guid purchaseLedgerId, Guid serviceFeeLedgerId,
        Money purchaseAmount, Money serviceFeeAmount, string description, string supplierDetails, string paymentMethod)
    {
        DomainGuards.AgainstDefault(clientId, nameof(clientId));
        DomainGuards.AgainstDefault(walletId, nameof(walletId));
        DomainGuards.AgainstNull(purchaseLedgerId, nameof(purchaseLedgerId));
        DomainGuards.AgainstNull(serviceFeeLedgerId, nameof(serviceFeeLedgerId));
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
        if (Status != ReservationStatus.Pending)
            throw new DomainException("Only pending reservations can be cancelled");

        Status = ReservationStatus.Cancelled;
        CancellationReason = reason.Trim();
        ProcessedBy = cancelledBy;
        CancelledAt = DateTime.UtcNow;
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