using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.Wallet.Query;

// Query to get purchase reservation summary statistics
public record GetClientPurchaseReservationSummaryQuery(Guid ClientId)
    : IRequest<Result<PurchaseReservationSummaryDto>>;

public class GetClientPurchaseReservationSummaryQueryHandler(
    IPurchaseReservationRepository purchaseReservationRepository,
    IClientRepository clientRepository)
    : IRequestHandler<GetClientPurchaseReservationSummaryQuery, Result<PurchaseReservationSummaryDto>>
{

    public async Task<Result<PurchaseReservationSummaryDto>> Handle(
        GetClientPurchaseReservationSummaryQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await clientRepository.GetAsync(query.ClientId);
            if (client == null)
                return Result<PurchaseReservationSummaryDto>.Failed($"Client not found: {query.ClientId}");

            var reservations = await purchaseReservationRepository.GetReservationsByClientIdAsync(query.ClientId);

            var summary = new PurchaseReservationSummaryDto
            {
                TotalReservations = reservations.Count,
                PendingCount = reservations.Count(r => r.Status == PurchaseReservationStatus.Pending),
                CompletedCount = reservations.Count(r => r.Status == PurchaseReservationStatus.Completed),
                CancelledCount = reservations.Count(r => r.Status == PurchaseReservationStatus.Cancelled),
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