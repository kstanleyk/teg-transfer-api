using Agrovet.Application.Features.Inventory.Item.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.Item.Queries;

public record ItemQuery : IRequest<ItemResponse>
{
    public required string Id { get; set; }
}

public class ItemQueryHandler(IItemRepository itemRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ItemQuery, ItemResponse>
{

    public async Task<ItemResponse> Handle(ItemQuery request, CancellationToken cancellationToken)
    {
        var item = await itemRepository.GetAsync(request.Id);
        return mapper.Map<ItemResponse>(item);
    }

    protected override void DisposeCore()
    {
        itemRepository.Dispose();
    }
}