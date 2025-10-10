using Transfer.Application.Helpers;
using Transfer.Domain.Entity.Inventory;

namespace Transfer.Application.Interfaces.Inventory;

public interface IProductMovementRepository : IRepository<ProductMovement, string>
{
    Task<RepositoryActionResult<IEnumerable<ProductMovement>>> UpdateItemMovementsAsync(string id,
        ProductMovement[] itemMovements);
}