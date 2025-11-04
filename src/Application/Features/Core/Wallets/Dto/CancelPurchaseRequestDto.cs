namespace TegWallet.Application.Features.Core.Wallets.Dto;

public class CancelPurchaseRequestDto
{
    public Guid ReservationId { get; set; }
    public string Reason { get; set; } = null!;
    public string CancelledBy { get; set; } = "ADMIN";
}