using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Interfaces.Core;

public interface IPurchaseReservationRepository : IRepository<PurchaseReservation, Guid>
{
    Task<IReadOnlyList<PurchaseReservation>> GetReservationsByClientIdAsync(Guid clientId, PurchaseReservationStatus? status = null);

    Task<PagedResult<PurchaseReservation>> GetPagedReservationsByClientIdAsync(
        Guid clientId,
        PurchaseReservationStatus? status = null,
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = true);
}