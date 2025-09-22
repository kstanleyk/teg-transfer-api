using Agrovet.Application.Features.Inventory.ItemCategory.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.ItemCategory;

public class ItemCategoryProfile : Profile
{
    public ItemCategoryProfile()
    {
        CreateMap<Domain.Entity.Inventory.ItemCategory, ItemCategoryResponse>();

        CreateMap<Domain.Entity.Inventory.ItemCategory, ItemCategoryUpdatedResponse>();

        CreateMap<Domain.Entity.Inventory.ItemCategory, ItemCategoryCreatedResponse>();
    }
}