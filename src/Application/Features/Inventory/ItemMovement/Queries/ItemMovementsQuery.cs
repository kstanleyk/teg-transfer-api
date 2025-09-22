using Agrovet.Application.Features.Inventory.ItemMovement.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ItemMovement.Queries;

public record ItemMovementsQuery : IRequest<ItemMovementResponse[]>;

public class ItemMovementsQueryHandler(IItemMovementRepository itemMovementRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ItemMovementsQuery, ItemMovementResponse[]>
{

    public async Task<ItemMovementResponse[]> Handle(ItemMovementsQuery request, CancellationToken cancellationToken)
    {
        var itemMovements = await itemMovementRepository.GetAllAsync();
        return mapper.Map<ItemMovementResponse[]>(itemMovements);
    }

    protected override void DisposeCore()
    {
        itemMovementRepository.Dispose();
    }
}