using Agrovet.Application.Features.Inventory.OrderType.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.OrderType;

public class OrderTypeProfile : Profile
{
    public OrderTypeProfile()
    {
        CreateMap<Domain.Entity.Inventory.OrderType, OrderTypeResponse>();

        CreateMap<Domain.Entity.Inventory.OrderType, OrderTypeUpdatedResponse>();

        CreateMap<Domain.Entity.Inventory.OrderType, OrderTypeCreatedResponse>();
    }
}