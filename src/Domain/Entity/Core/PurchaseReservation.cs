using TegWallet.Domain.Abstractions;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Domain.Entity.Core;

public class PurchaseReservation : Entity<Guid>
{
    public Guid ClientId { get; private init; }
    public Guid WalletId { get; private init; }
    public LedgerId PurchaseLedgerId { get; private init; }
    public LedgerId ServiceFeeLedgerId { get; private init; }
    public Money PurchaseAmount { get; private init; }
    public Money ServiceFeeAmount { get; private init; }
    public Money TotalAmount { get; private init; }
    public string Description { get; private init; }
    public string SupplierDetails { get; private init; }
    public string PaymentMethod { get; private init; }
    public PurchaseReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? ProcessedBy { get; private set; }

    // Private constructor for EF Core
    private PurchaseReservation() { }

    public static PurchaseReservation Create(
        Guid clientId,
        Guid walletId,
        LedgerId purchaseLedgerId,
        LedgerId serviceFeeLedgerId,
        Money purchaseAmount,
        Money serviceFeeAmount,
        string description,
        string supplierDetails,
        string paymentMethod)
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

        var reservation = new PurchaseReservation
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
            Status = PurchaseReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        return reservation;
    }

    public void Complete(string processedBy = "ADMIN")
    {
        if (Status != PurchaseReservationStatus.Pending)
            throw new DomainException("Only pending reservations can be completed");

        Status = PurchaseReservationStatus.Completed;
        ProcessedBy = processedBy;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason, string cancelledBy = "ADMIN")
    {
        if (Status != PurchaseReservationStatus.Pending)
            throw new DomainException("Only pending reservations can be cancelled");

        Status = PurchaseReservationStatus.Cancelled;
        CancellationReason = reason.Trim();
        ProcessedBy = cancelledBy;
        CancelledAt = DateTime.UtcNow;
    }

    public bool CanBeCompleted => Status == PurchaseReservationStatus.Pending;
    public bool CanBeCancelled => Status == PurchaseReservationStatus.Pending;
    public bool IsPending => Status == PurchaseReservationStatus.Pending;
    public bool IsCompleted => Status == PurchaseReservationStatus.Completed;
    public bool IsCancelled => Status == PurchaseReservationStatus.Cancelled;
}

public enum PurchaseReservationStatus
{
    Pending = 1,
    Completed = 2,
    Cancelled = 3
}