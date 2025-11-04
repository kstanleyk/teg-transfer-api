namespace TegWallet.Application.Features.Core.Wallets.Dto;

public class ReservePurchaseRequestDto
{
    public decimal PurchaseAmount { get; set; }
    public decimal ServiceFeeAmount { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string SupplierDetails { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
}