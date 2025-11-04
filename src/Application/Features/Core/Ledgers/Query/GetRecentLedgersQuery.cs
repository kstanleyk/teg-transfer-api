using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Ledgers.Query;

public record GetRecentLedgersQuery(Guid ClientId, int Limit = 10) : IRequest<Result<LedgerDto[]>>;

public class GetRecentLedgersQueryHandler(
    ILedgerRepository ledgerRepository,
    IWalletRepository walletRepository,
    IMapper mapper)
    : GetLedgersQueryHandlerBase(walletRepository,mapper), IRequestHandler<GetRecentLedgersQuery, Result<LedgerDto[]>>
{
    private readonly ILedgerRepository _ledgerRepository = ledgerRepository;
    private readonly IWalletRepository _walletRepository = walletRepository;

    public async Task<Result<LedgerDto[]>> Handle(GetRecentLedgersQuery query, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByClientIdAsync(query.ClientId);

        if (wallet == null)
            return Result<LedgerDto[]>.Succeeded([]);

        var ledgers = await _ledgerRepository.GetLastTopLedgersForWalletAsync(wallet.Id, query.Limit);

        var walletIds = ledgers.Select(x => x.WalletId).ToArray();
        var ledgerWithClientIds = await UpdateClientIds(walletIds, ledgers);

        return Result<LedgerDto[]>.Succeeded(ledgerWithClientIds);
    }
}