using Agrovet.Application.Features.Inventory.Order.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.Order;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Domain.Entity.Inventory.Order, OrderResponse>()
            .ForMember(dest => dest.PublicId, opt => opt.MapFrom(src => src.PublicId!.Value));
    }
}