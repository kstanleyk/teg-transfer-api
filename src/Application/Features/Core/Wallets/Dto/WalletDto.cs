using TegWallet.Application.Features.Core.Currencies.Dto;

namespace TegWallet.Application.Features.Core.Wallets.Dto;

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
    public List<LedgerDto> RecentTransactions { get; set; } = [];
    public List<ReservationDto> ActiveReservations { get; set; } = [];
}

public enum WalletStatus
{
    Active = 1,
    LowBalance = 2,
    NoActivity = 3
}