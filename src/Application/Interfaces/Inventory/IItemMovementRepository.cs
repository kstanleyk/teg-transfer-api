using Agrovet.Domain.Entity.Inventory;

namespace Agrovet.Application.Interfaces.Inventory;

public interface IItemMovementRepository : IRepository<ItemMovement, string> { }