namespace TegWallet.Application.Features.Core.Wallets.Dto;

public class ReservationDto
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
    public int DaysPending { get; set; }
    public bool CanBeApproved { get; set; }
    public bool CanBeCancelled { get; set; }
}