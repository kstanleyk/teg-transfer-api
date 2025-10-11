using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Query;

public record GetWalletBalanceQuery(Guid ClientId) : IRequest<WalletBalanceDto>;

public class GetWalletBalanceQueryHandler(
    IWalletRepository walletRepository,
    IMapper mapper)
    : IRequestHandler<GetWalletBalanceQuery, WalletBalanceDto>
{
    private readonly IWalletRepository _walletRepository = walletRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<WalletBalanceDto> Handle(GetWalletBalanceQuery query, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByClientIdAsync(query.ClientId);

        return _mapper.Map<WalletBalanceDto>(wallet);
    }
}