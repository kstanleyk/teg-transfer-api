using Agrovet.Application.Features.Inventory.ItemCategory.Dtos;
using Agrovet.Application.Interfaces.Inventory;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Inventory.ItemCategory.Queries;

public record ItemCategoriesQuery : IRequest<ItemCategoryResponse[]>;

public class ItemCategoriesQueryHandler(IItemCategoryRepository itemCategoryRepository, IMapper mapper)
    : RequestHandlerBase, IRequestHandler<ItemCategoriesQuery, ItemCategoryResponse[]>
{

    public async Task<ItemCategoryResponse[]> Handle(ItemCategoriesQuery request, CancellationToken cancellationToken)
    {
        var itemCategories = await itemCategoryRepository.GetAllAsync();
        return mapper.Map<ItemCategoryResponse[]>(itemCategories);
    }

    protected override void DisposeCore()
    {
        itemCategoryRepository.Dispose();
    }
}