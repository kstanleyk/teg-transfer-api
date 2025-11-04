namespace TegWallet.Application.Features.Core.Wallets.Dto;

public class ReservationDetailDto : ReservationDto
{
    public LedgerDto? PurchaseLedger { get; set; }
    public LedgerDto? ServiceFeeLedger { get; set; }
    public string? ClientName { get; set; }
    public string? ClientEmail { get; set; }
    public string? ClientPhone { get; set; }
}