using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallets.Query;

public record GetWalletsQuery : IRequest<Result<WalletDto[]>>;

public class GetWalletsQueryHandler(
    IWalletRepository walletRepository,
    IMapper mapper)
    : IRequestHandler<GetWalletsQuery, Result<WalletDto[]>>
{
    private readonly IWalletRepository _walletRepository = walletRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<WalletDto[]>> Handle(GetWalletsQuery query, CancellationToken cancellationToken)
    {
        var wallets = await _walletRepository.GetWalletsAsync();

        return Result<WalletDto[]>.Succeeded(_mapper.Map<WalletDto[]>(wallets));
    }
}