using MediatR;
using TegWallet.Application.Features.Core.Clients.Dto;
using TegWallet.Application.Features.Core.Currencies.Dto;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Clients.Query;

public record GetUserProfileQuery(string? UserId) : IRequest<Result<UserProfileDto>>;

public class GetUserProfileQueryHandler(
    IClientRepository clientRepository,
    IWalletRepository walletRepository)
    :RequestHandlerBase, IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery query, CancellationToken cancellationToken)
    {
        if(query.UserId == null)
            return Result<UserProfileDto>.Failed("UserId cannot be null.");

        var clientId = Guid.Parse(query.UserId);

        var client = await clientRepository.GetByUserIdAsync(clientId);

        if (client == null)
            return Result<UserProfileDto>.Failed("Client not found.");

        var wallet = await walletRepository.GetByClientIdWithDetailsAsync(client.Id);

        if(wallet == null)
            return Result<UserProfileDto>.Failed("Wallet not found for the client.");

        var userProfile = new UserProfileDto
        {
            Id = client.Id,
            Email = client.Email,
            Fullname = client.FullName,
            Phone = client.PhoneNumber,
            Country = "",
            Currency = new CurrencyDto
            {
                Code = wallet.BaseCurrency.Code, Symbol = wallet.BaseCurrency.Symbol,
                DecimalPlaces = wallet.BaseCurrency.DecimalPlaces
            },
            WalletBalance = new MoneyDto(wallet.Balance.Amount,wallet.BaseCurrency.Code, wallet.BaseCurrency.Symbol),
            WalletAvailableBalance = new MoneyDto(wallet.AvailableBalance.Amount,wallet.BaseCurrency.Code, wallet.BaseCurrency.Symbol),
            KycStatus = "verified", // "pending"; // "pending", "verified", "rejected"
            AccountStatus = "active", // "active"; // "active", "suspended", "inactive"
            CreatedAt = client.CreatedAt,
            LastLogin = DateTime.Now
        };

        return Result<UserProfileDto>.Succeeded(userProfile, "user profile retrieved successfully.");
    }

    protected override void DisposeCore()
    {
        clientRepository.Dispose();
    }
}