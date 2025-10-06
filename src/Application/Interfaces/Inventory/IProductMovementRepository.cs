using Agrovet.Application.Helpers;
using Agrovet.Domain.Entity.Inventory;

namespace Agrovet.Application.Interfaces.Inventory;

public interface IProductMovementRepository : IRepository<ProductMovement, string>
{
    Task<RepositoryActionResult<IEnumerable<ProductMovement>>> UpdateItemMovementsAsync(string id,
        ProductMovement[] itemMovements);
}