using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallets.Query;

// Query to get purchase reservation by ID with details
public record GetPurchaseReservationByIdQuery(Guid ReservationId)
    : IRequest<Result<ReservationDetailDto>>;

public class GetPurchaseReservationByIdQueryHandler(
    IWalletRepository walletRepository,
    IMapper mapper)
    : IRequestHandler<GetPurchaseReservationByIdQuery, Result<ReservationDetailDto>>
{
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ReservationDetailDto>> Handle(
        GetPurchaseReservationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByReservationIdAsync(query.ReservationId);
        if (wallet == null)
            throw new InvalidOperationException($"Wallet not found for reservation: {query.ReservationId}");

        var reservation = wallet.GetPurchaseReservation(query.ReservationId);
        if (reservation == null)
            throw new InvalidOperationException($"Purchase reservation not found: {query.ReservationId}");

        var purchaseLedger = wallet.Ledgers.FirstOrDefault(l => l.Id == reservation.PurchaseLedgerId);
        var serviceFeeLedger = wallet.Ledgers.FirstOrDefault(l => l.Id == reservation.ServiceFeeLedgerId);

        var detailDto = _mapper.Map<ReservationDetailDto>(reservation);
        detailDto.PurchaseLedger = _mapper.Map<LedgerDto>(purchaseLedger);
        detailDto.ServiceFeeLedger = _mapper.Map<LedgerDto>(serviceFeeLedger);
        detailDto.ClientName = "";//$"{wallet.Client?.FullName} {wallet.Client?.LastName}";

        return Result<ReservationDetailDto>.Succeeded(detailDto);
    }
}