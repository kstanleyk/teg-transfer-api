using Agrovet.Application.Features.Inventory.Order.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.Order;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Domain.Entity.Inventory.Order, OrderResponse>();
        CreateMap<Domain.Entity.Inventory.Order, OrderCreatedResponse>();
        CreateMap<Domain.Entity.Inventory.Order, OrderUpdatedResponse>();
    }
}