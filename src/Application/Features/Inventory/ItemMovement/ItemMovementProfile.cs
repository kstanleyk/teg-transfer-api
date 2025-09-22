using Agrovet.Application.Features.Inventory.ItemMovement.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.ItemMovement;

public class ItemMovementProfile : Profile
{
    public ItemMovementProfile()
    {
        CreateMap<Domain.Entity.Inventory.ItemMovement, ItemMovementResponse>()
            .ForMember(dest => dest.PublicId, opt => opt.MapFrom(src => src.PublicId!.Value));
    }
}