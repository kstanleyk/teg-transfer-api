using Agrovet.Application.Features.Inventory.Item.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.Item;

public class ItemProfile : Profile
{
    public ItemProfile()
    {
        CreateMap<Domain.Entity.Inventory.Item, ItemResponse>()
            .ForMember(dest => dest.PublicId, opt => opt.MapFrom(src => src.PublicId!.Value));
    }
}