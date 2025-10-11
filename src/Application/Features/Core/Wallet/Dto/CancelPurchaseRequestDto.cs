namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class CancelPurchaseRequestDto
{
    public Guid ReservationId { get; set; }
    public string Reason { get; set; } = null!;
    public string CancelledBy { get; set; } = "ADMIN";
}