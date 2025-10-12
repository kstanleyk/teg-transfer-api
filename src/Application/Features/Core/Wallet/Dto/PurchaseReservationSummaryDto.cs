namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class PurchaseReservationSummaryDto
{
    public int TotalReservations { get; set; }
    public int PendingCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public decimal TotalServiceFeeAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = null!;
}

public class PurchaseReservationDto1
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid WalletId { get; set; }
    public Guid PurchaseLedgerId { get; set; }
    public Guid ServiceFeeLedgerId { get; set; }
    public decimal PurchaseAmount { get; set; }
    public decimal ServiceFeeAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string SupplierDetails { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public string? ProcessedBy { get; set; }

    // Additional calculated properties
    public int? DaysPending { get; set; }
    public bool CanBeApproved { get; set; }
    public bool CanBeCancelled { get; set; }
}