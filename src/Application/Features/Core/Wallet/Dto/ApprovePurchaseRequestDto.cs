namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class ApprovePurchaseRequestDto
{
    public Guid ReservationId { get; set; }
    public string ProcessedBy { get; set; } = "ADMIN";
}