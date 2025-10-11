using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Query;

public record GetWalletByClientIdQuery(Guid ClientId) : IRequest<WalletDto>;

public class GetWalletByClientIdQueryHandler(
    IWalletRepository walletRepository,
    IMapper mapper)
    : IRequestHandler<GetWalletByClientIdQuery, WalletDto>
{
    private readonly IWalletRepository _walletRepository = walletRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<WalletDto> Handle(GetWalletByClientIdQuery query, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByClientIdWithDetailsAsync(query.ClientId);

        return _mapper.Map<WalletDto>(wallet);
    }
}