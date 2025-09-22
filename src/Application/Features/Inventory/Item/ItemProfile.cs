using Agrovet.Application.Features.Inventory.Item.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.Item;

public class ItemProfile : Profile
{
    public ItemProfile()
    {
        CreateMap<Domain.Entity.Inventory.Item, ItemResponse>();
        CreateMap<Domain.Entity.Inventory.Item, ItemCreatedResponse>();
        CreateMap<Domain.Entity.Inventory.Item, ItemUpdatedResponse>();
    }
}