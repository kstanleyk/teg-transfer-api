using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Query;

public record GetSimpleBalanceQuery(Guid ClientId) : IRequest<SimpleBalanceDto>;

public class GetSimpleBalanceQueryHandler(
    IWalletRepository walletRepository,
    IMapper mapper)
    : IRequestHandler<GetSimpleBalanceQuery, SimpleBalanceDto>
{
    private readonly IWalletRepository _walletRepository = walletRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<SimpleBalanceDto> Handle(GetSimpleBalanceQuery query, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByClientIdAsync(query.ClientId);

        return _mapper.Map<SimpleBalanceDto>(wallet);
    }
}