using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallets.Query;

// Query to get specific purchase reservation
public record GetPurchaseReservationQuery(Guid ClientId, Guid ReservationId) : IRequest<Result<ReservationDto>>;

public class GetPurchaseReservationQueryHandler(IWalletRepository walletRepository, IMapper mapper)
    : IRequestHandler<GetPurchaseReservationQuery, Result<ReservationDto>>
{
    public async Task<Result<ReservationDto>> Handle(GetPurchaseReservationQuery query, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByClientIdAsync(query.ClientId);
        if (wallet == null)
            return Result<ReservationDto>.Failed("Wallet not found");

        var reservation = wallet.GetPurchaseReservation(query.ReservationId);
        if (reservation == null)
            return Result<ReservationDto>.Failed("Purchase reservation not found");

        var dto = mapper.Map<ReservationDto>(reservation);
        return Result<ReservationDto>.Succeeded(dto);
    }
}