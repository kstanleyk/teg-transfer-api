using TegWallet.Application.Features.Core.Currencies.Dto;
using TegWallet.Application.Features.Core.Wallets.Dto;

namespace TegWallet.Application.Features.Core.Clients.Dto;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Fullname { get; set; }
    public string Phone { get; set; }
    public string Country { get; set; }
    public CurrencyDto Currency { get; set; } 
    public MoneyDto WalletBalance { get; set; }
    public MoneyDto WalletAvailableBalance { get; set; }
    public string KycStatus { get; set; } = "pending"; // "pending", "verified", "rejected"
    public string AccountStatus { get; set; } = "active"; // "active", "suspended", "inactive"
    public DateTime CreatedAt { get; set; }
    public DateTime LastLogin { get; set; }
}