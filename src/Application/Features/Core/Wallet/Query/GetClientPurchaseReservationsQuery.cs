using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.Wallet.Query;

public record GetClientPurchaseReservationsQuery(
    Guid ClientId,
    PurchaseReservationStatus? Status = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "CreatedAt",
    bool SortDescending = true)
    : IRequest<PagedResponse<PurchaseReservationDto>>;

public class GetClientPurchaseReservationsQueryHandler(
        IReservationRepository reservationRepository,
        IClientRepository clientRepository,
        IMapper mapper)
        : IRequestHandler<GetClientPurchaseReservationsQuery, PagedResponse<PurchaseReservationDto>>
{
    public async Task<PagedResponse<PurchaseReservationDto>> Handle(
        GetClientPurchaseReservationsQuery query,
        CancellationToken cancellationToken)
    {
        // Validate client exists
        var client = await clientRepository.GetAsync(query.ClientId);
        if (client == null)
            throw new InvalidOperationException($"Client not found: {query.ClientId}");

        // Get paged reservations
        var pagedResult = await reservationRepository.GetPagedReservationsByClientIdAsync(
            query.ClientId,
            query.Status,
            query.Page,
            query.PageSize,
            query.SortBy,
            query.SortDescending);

        // Map to DTOs
        var reservationDtos = mapper.Map<List<PurchaseReservationDto>>(pagedResult.Items);

        var response = new PagedResponse<PurchaseReservationDto>
        {
            Items = reservationDtos,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages,
            HasPrevious = pagedResult.HasPrevious,
            HasNext = pagedResult.HasNext
        };

        return response;
    }
}