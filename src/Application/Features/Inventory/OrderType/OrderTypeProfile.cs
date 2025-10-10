using AutoMapper;
using Transfer.Application.Features.Inventory.OrderType.Dtos;

namespace Transfer.Application.Features.Inventory.OrderType;

public class OrderTypeProfile : Profile
{
    public OrderTypeProfile()
    {
        CreateMap<Transfer.Domain.Entity.Inventory.OrderType, OrderTypeResponse>();

        CreateMap<Transfer.Domain.Entity.Inventory.OrderType, OrderTypeUpdatedResponse>();

        CreateMap<Transfer.Domain.Entity.Inventory.OrderType, OrderTypeCreatedResponse>();
    }
}