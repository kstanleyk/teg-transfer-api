using Agrovet.Application.Features.Inventory.Item.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Item.Queries;

public record ItemsQuery : IRequest<ItemResponse[]>;

public class ItemsQueryHandler(IItemRepository itemRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ItemsQuery, ItemResponse[]>
{
    public async Task<ItemResponse[]> Handle(ItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await itemRepository.GetAllAsync();
        return mapper.Map<ItemResponse[]>(items);
    }

    protected override void DisposeCore()
    {
        itemRepository.Dispose();
    }
}