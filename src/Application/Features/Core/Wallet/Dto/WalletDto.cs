using System.Text.Json.Serialization;

namespace TegWallet.Application.Features.Core.Wallet.Dto;

public class WalletDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public decimal Balance { get; set; }
    public decimal AvailableBalance { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public WalletStatus Status { get; set; }
    public List<TransactionDto> RecentTransactions { get; set; } = [];
    public List<PurchaseReservationDto> ActiveReservations { get; set; } = [];
}

public enum WalletStatus
{
    Active = 1,
    LowBalance = 2,
    NoActivity = 3
}