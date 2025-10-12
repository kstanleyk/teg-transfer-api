using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Interfaces.Core;

public interface IReservationRepository : IRepository<Reservation, Guid>
{
    Task<IReadOnlyList<Reservation>> GetReservationsByClientIdAsync(Guid clientId, PurchaseReservationStatus? status = null);

    Task<PagedResult<Reservation>> GetPagedReservationsByClientIdAsync(
        Guid clientId,
        PurchaseReservationStatus? status = null,
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = true);
}