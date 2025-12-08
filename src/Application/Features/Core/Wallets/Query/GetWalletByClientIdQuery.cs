using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallets.Query;

public record GetWalletByClientIdQuery(Guid ClientId) : IRequest<Result<WalletDto>>;

public class GetWalletByClientIdQueryHandler(
    IWalletRepository walletRepository,
    IMapper mapper)
    : IRequestHandler<GetWalletByClientIdQuery, Result<WalletDto>>
{
    private readonly IMapper _mapper = mapper;

    public async Task<Result<WalletDto>> Handle(GetWalletByClientIdQuery query, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByClientIdWithDetailsAsync(query.ClientId);

        return Result<WalletDto>.Succeeded(_mapper.Map<WalletDto>(wallet));
    }
}