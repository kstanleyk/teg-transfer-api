using Agrovet.Application.Features.Inventory.ItemCategory.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ItemCategory.Queries;

public record ItemCategoryQuery : IRequest<ItemCategoryResponse>
{
    public required Guid PublicId { get; set; }
}

public class ItemCategoryQueryHandler(IItemCategoryRepository itemCategoryRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ItemCategoryQuery, ItemCategoryResponse>
{

    public async Task<ItemCategoryResponse> Handle(ItemCategoryQuery request, CancellationToken cancellationToken)
    {
        var itemCategory = await itemCategoryRepository.GetByPublicIdAsync(request.PublicId);
        return mapper.Map<ItemCategoryResponse>(itemCategory);
    }

    protected override void DisposeCore()
    {
        itemCategoryRepository.Dispose();
    }
}