using MediatR;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.Wallets.Query;

// Query to search purchase reservations with advanced filters

public record SearchPurchaseReservationsQuery(
    Guid? ClientId = null,
    ReservationStatus? Status = null,
    string? PaymentMethod = null,
    string? SupplierSearch = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "CreatedAt",
    bool SortDescending = true)
    : IRequest<Result<PagedResponse<ReservationDto>>>;