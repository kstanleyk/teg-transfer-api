﻿using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Wallet.Query;

// Query to get purchase reservation by ID with details
public record GetPurchaseReservationByIdQuery(Guid ReservationId)
    : IRequest<PurchaseReservationDetailDto>;

public class GetPurchaseReservationByIdQueryHandler(
    IWalletRepository walletRepository,
    IMapper mapper)
    : IRequestHandler<GetPurchaseReservationByIdQuery, PurchaseReservationDetailDto>
{
    private readonly IMapper _mapper = mapper;

    public async Task<PurchaseReservationDetailDto> Handle(
        GetPurchaseReservationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByReservationIdAsync(query.ReservationId);
        if (wallet == null)
            throw new InvalidOperationException($"Wallet not found for reservation: {query.ReservationId}");

        var reservation = wallet.GetPurchaseReservation(query.ReservationId);
        if (reservation == null)
            throw new InvalidOperationException($"Purchase reservation not found: {query.ReservationId}");

        var purchaseLedger = wallet.LedgerEntries.FirstOrDefault(l => l.Id == reservation.PurchaseLedgerId);
        var serviceFeeLedger = wallet.LedgerEntries.FirstOrDefault(l => l.Id == reservation.ServiceFeeLedgerId);

        var detailDto = _mapper.Map<PurchaseReservationDetailDto>(reservation);
        detailDto.PurchaseLedger = _mapper.Map<TransactionDto>(purchaseLedger);
        detailDto.ServiceFeeLedger = _mapper.Map<TransactionDto>(serviceFeeLedger);
        detailDto.ClientName = "";//$"{wallet.Client?.FirstName} {wallet.Client?.LastName}";

        return detailDto;
    }
}