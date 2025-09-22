using Agrovet.Application.Features.Inventory.ItemMovement.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ItemMovement.Queries;

public record ItemMovementQuery : IRequest<ItemMovementResponse>
{
    public required string Id { get; set; }
}

public class ItemMovementQueryHandler(IItemMovementRepository itemMovementRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ItemMovementQuery, ItemMovementResponse>
{

    public async Task<ItemMovementResponse> Handle(ItemMovementQuery request, CancellationToken cancellationToken)
    {
        var itemMovement = await itemMovementRepository.GetAsync(request.Id);
        return mapper.Map<ItemMovementResponse>(itemMovement);
    }

    protected override void DisposeCore()
    {
        itemMovementRepository.Dispose();
    }
}