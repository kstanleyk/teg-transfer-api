using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Infrastructure.Persistence.Repository.Core;

public class ReservationRepository(IDatabaseFactory databaseFactory)
    : DataRepository<Reservation, Guid>(databaseFactory), IReservationRepository
{
    public async Task<IReadOnlyList<Reservation>> GetReservationsByClientIdAsync(Guid clientId,
        ReservationStatus? status = null)
    {
        var query = DbSet
            .Where(pr => pr.ClientId == clientId);

        if (status.HasValue)
        {
            query = query.Where(pr => pr.Status == status.Value);
        }

        var reservations = await query
            .OrderByDescending(pr => pr.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return reservations.AsReadOnly();
    }

    public async Task<PagedResult<Reservation>> GetPagedReservationsByClientIdAsync(
        Guid clientId,
        ReservationStatus? status = null,
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = true)
    {
        // Validate pagination
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Limit page size

        var query = DbSet
            .Where(pr => pr.ClientId == clientId);

        if (status.HasValue)
        {
            query = query.Where(pr => pr.Status == status.Value);
        }

        // Get total count for pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortDescending);

        // Apply pagination
        var reservations = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<Reservation>
        {
            Items = reservations,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static IQueryable<Reservation> ApplySorting(
        IQueryable<Reservation> query,
        string? sortBy,
        bool sortDescending)
    {
        return (sortBy?.ToLower(), sortDescending) switch
        {
            ("amount" or "purchaseamount", true) => query.OrderByDescending(pr => pr.PurchaseAmount.Amount),
            ("amount" or "purchaseamount", false) => query.OrderBy(pr => pr.PurchaseAmount.Amount),
            ("totalamount", true) => query.OrderByDescending(pr => pr.TotalAmount.Amount),
            ("totalamount", false) => query.OrderBy(pr => pr.TotalAmount.Amount),
            ("status", true) => query.OrderByDescending(pr => pr.Status),
            ("status", false) => query.OrderBy(pr => pr.Status),
            ("paymentmethod", true) => query.OrderByDescending(pr => pr.PaymentMethod),
            ("paymentmethod", false) => query.OrderBy(pr => pr.PaymentMethod),
            ("createdat" or null, false) => query.OrderBy(pr => pr.CreatedAt),
            _ => query.OrderByDescending(pr => pr.CreatedAt) // Default sort
        };
    }
}