using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.Wallets.Query;

public record GetClientPurchaseReservationsQuery(
    Guid ClientId,
    ReservationStatus? Status = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "CreatedAt",
    bool SortDescending = true)
    : IRequest<Result<PagedResponse<ReservationDto>>>;

public class GetClientPurchaseReservationsQueryHandler(
    IReservationRepository reservationRepository,
    IClientRepository clientRepository,
    IMapper mapper)
    : IRequestHandler<GetClientPurchaseReservationsQuery, Result<PagedResponse<ReservationDto>>>
{
    public async Task<Result<PagedResponse<ReservationDto>>> Handle(
        GetClientPurchaseReservationsQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate client exists
            var client = await clientRepository.GetAsync(query.ClientId);
            if (client == null)
                return Result<PagedResponse<ReservationDto>>.Failed($"Client not found: {query.ClientId}");

            // Get paged reservations
            var pagedResult = await reservationRepository.GetPagedReservationsByClientIdAsync(
                query.ClientId,
                query.Status,
                query.Page,
                query.PageSize,
                query.SortBy,
                query.SortDescending);

            // Map to DTOs
            var reservationDtos = mapper.Map<List<ReservationDto>>(pagedResult.Items);

            var response = new PagedResponse<ReservationDto>
            {
                Items = reservationDtos,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount
                // TotalPages, HasPrevious, HasNext are computed properties
            };

            return Result<PagedResponse<ReservationDto>>.Succeeded(response);
        }
        catch (Exception ex)
        {
            return Result<PagedResponse<ReservationDto>>.Failed($"An error occurred while retrieving purchase reservations: {ex.Message}");
        }
    }
}