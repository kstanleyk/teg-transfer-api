using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Ledgers.Query;

public record GetLedgersQuery : IRequest<Result<LedgerDto[]>>;

public class GetLedgersQueryHandler(
    ILedgerRepository ledgerRepository,
    IWalletRepository walletRepository,
    IMapper mapper)
    : GetLedgersQueryHandlerBase(walletRepository,mapper), IRequestHandler<GetLedgersQuery, Result<LedgerDto[]>>
{
    private readonly ILedgerRepository _ledgerRepository = ledgerRepository;

    public async Task<Result<LedgerDto[]>> Handle(GetLedgersQuery query, CancellationToken cancellationToken)
    {
        var ledgers = await _ledgerRepository.GetLastTenLedgersEachForEveryActiveWalletAsync();

        var walletIds = ledgers.Select(x => x.WalletId).ToArray();
        var ledgerWithClientIds = await UpdateClientIds(walletIds, ledgers);

        return Result<LedgerDto[]>.Succeeded(ledgerWithClientIds);
    }
}