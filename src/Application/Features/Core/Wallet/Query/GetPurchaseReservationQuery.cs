using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Query;

// Query to get specific purchase reservation
public record GetPurchaseReservationQuery(Guid ClientId, Guid ReservationId) : IRequest<Result<PurchaseReservationDto>>;

public class GetPurchaseReservationQueryHandler(IWalletRepository walletRepository, IMapper mapper)
    : IRequestHandler<GetPurchaseReservationQuery, Result<PurchaseReservationDto>>
{
    public async Task<Result<PurchaseReservationDto>> Handle(GetPurchaseReservationQuery query, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByClientIdAsync(query.ClientId);
        if (wallet == null)
            return Result<PurchaseReservationDto>.Failed("Wallet not found");

        var reservation = wallet.GetPurchaseReservation(query.ReservationId);
        if (reservation == null)
            return Result<PurchaseReservationDto>.Failed("Purchase reservation not found");

        var dto = mapper.Map<PurchaseReservationDto>(reservation);
        return Result<PurchaseReservationDto>.Succeeded(dto);
    }
}