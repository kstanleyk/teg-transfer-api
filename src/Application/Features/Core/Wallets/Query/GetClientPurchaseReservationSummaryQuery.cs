using MediatR;
using Microsoft.AspNetCore.Identity;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.Wallets.Query;

// Query to get purchase reservation summary statistics
public record GetClientPurchaseReservationSummaryQuery(Guid ClientId)
    : IRequest<Result<PurchaseReservationSummaryDto>>;

public class GetClientPurchaseReservationSummaryQueryHandler(
    IReservationRepository reservationRepository,
    UserManager<Domain.Entity.Core.Client> clientRepository)
    : IRequestHandler<GetClientPurchaseReservationSummaryQuery, Result<PurchaseReservationSummaryDto>>
{

    public async Task<Result<PurchaseReservationSummaryDto>> Handle(
        GetClientPurchaseReservationSummaryQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await clientRepository.FindByIdAsync(query.ClientId.ToString());
            if (client == null)
                return Result<PurchaseReservationSummaryDto>.Failed($"Client not found: {query.ClientId}");

            var reservations = await reservationRepository.GetReservationsByClientIdAsync(query.ClientId);

            var summary = new PurchaseReservationSummaryDto
            {
                TotalReservations = reservations.Count,
                PendingCount = reservations.Count(r => r.Status == ReservationStatus.Pending),
                CompletedCount = reservations.Count(r => r.Status == ReservationStatus.Completed),
                CancelledCount = reservations.Count(r => r.Status == ReservationStatus.Cancelled),
                TotalPurchaseAmount = reservations.Sum(r => r.PurchaseAmount.Amount),
                TotalServiceFeeAmount = reservations.Sum(r => r.ServiceFeeAmount.Amount),
                TotalAmount = reservations.Sum(r => r.TotalAmount.Amount),
                CurrencyCode = reservations.FirstOrDefault()?.PurchaseAmount.Currency.Code ?? "XOF"
            };

            return Result<PurchaseReservationSummaryDto>.Succeeded(summary);
        }
        catch (Exception ex)
        {
            return Result<PurchaseReservationSummaryDto>.Failed($"An error occurred while retrieving reservation summary: {ex.Message}");
        }
    }
}