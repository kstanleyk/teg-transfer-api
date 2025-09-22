using Agrovet.Application.Features.Inventory.ItemCategory.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.ItemCategory;

public class ItemCategoryProfile : Profile
{
    public ItemCategoryProfile()
    {
        CreateMap<Domain.Entity.Inventory.ItemCategory, ItemCategoryResponse>()
            .ForMember(dest => dest.PublicId, opt => opt.MapFrom(src => src.PublicId!.Value));
    }
}