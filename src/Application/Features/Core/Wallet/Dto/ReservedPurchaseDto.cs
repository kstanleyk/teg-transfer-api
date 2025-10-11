namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class ReservedPurchaseDto
{
    public Guid ReservationId { get; set; }
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
}