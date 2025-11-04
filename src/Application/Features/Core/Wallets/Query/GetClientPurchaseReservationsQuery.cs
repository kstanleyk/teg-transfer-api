using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
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
        UserManager<Domain.Entity.Core.Client> clientRepository,
        IMapper mapper)
        : IRequestHandler<GetClientPurchaseReservationsQuery, Result<PagedResponse<ReservationDto>>>
{
    public async Task<Result<PagedResponse<ReservationDto>>> Handle(
        GetClientPurchaseReservationsQuery query,
        CancellationToken cancellationToken)
    {
        // Validate client exists
        var client = await clientRepository.FindByIdAsync(query.ClientId.ToString());
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
        var reservationDtos = mapper.Map<List<ReservationDto>>(pagedResult.Items);

        var response = new PagedResponse<ReservationDto>
        {
            Items = reservationDtos,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages,
            HasPrevious = pagedResult.HasPrevious,
            HasNext = pagedResult.HasNext
        };

        return Result<PagedResponse<ReservationDto>>.Succeeded(response);
    }
}