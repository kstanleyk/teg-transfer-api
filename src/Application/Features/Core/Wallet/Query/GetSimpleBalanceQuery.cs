using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Query;

public record GetSimpleBalanceQuery(Guid ClientId) : IRequest<Result<SimpleBalanceDto>>;

public class GetSimpleBalanceQueryHandler(
    IWalletRepository walletRepository,
    IMapper mapper)
    : IRequestHandler<GetSimpleBalanceQuery, Result<SimpleBalanceDto>>
{
    private readonly IWalletRepository _walletRepository = walletRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<SimpleBalanceDto>> Handle(GetSimpleBalanceQuery query, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByClientIdAsync(query.ClientId);

        return Result<SimpleBalanceDto>.Succeeded(_mapper.Map<SimpleBalanceDto>(wallet));
    }
}