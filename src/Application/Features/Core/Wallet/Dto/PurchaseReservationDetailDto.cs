namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class PurchaseReservationDetailDto : PurchaseReservationDto
{
    public TransactionDto? PurchaseLedger { get; set; }
    public TransactionDto? ServiceFeeLedger { get; set; }
    public string? ClientName { get; set; }
    public string? ClientEmail { get; set; }
    public string? ClientPhone { get; set; }
}