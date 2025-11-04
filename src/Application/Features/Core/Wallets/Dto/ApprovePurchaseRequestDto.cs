namespace TegWallet.Application.Features.Core.Wallets.Dto;

public class ApprovePurchaseRequestDto
{
    public Guid ReservationId { get; set; }
    public string ProcessedBy { get; set; } = "ADMIN";
}