using AutoMapper;
using Transfer.Application.Features.Inventory.Order.Dtos;

namespace Transfer.Application.Features.Inventory.Order;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Transfer.Domain.Entity.Inventory.Order, OrderResponse>();
        CreateMap<Transfer.Domain.Entity.Inventory.Order, OrderCreatedResponse>();
        CreateMap<Transfer.Domain.Entity.Inventory.Order, OrderUpdatedResponse>();
    }
}