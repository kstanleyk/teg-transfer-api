using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Purchases.Query;

public record GetReservationsQuery : IRequest<Result<ReservationDto[]>>;

public class GetWalletsQueryHandler(
    IReservationRepository reservationRepository,
    IMapper mapper)
    : IRequestHandler<GetReservationsQuery, Result<ReservationDto[]>>
{
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ReservationDto[]>> Handle(GetReservationsQuery query, CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.GetPendingReservationsAsync();

        return Result<ReservationDto[]>.Succeeded(_mapper.Map<ReservationDto[]>(reservations));
    }
}