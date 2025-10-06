using Agrovet.Application.Features.Inventory.OrderStatus.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.OrderStatus;

public class OrderStatusProfile : Profile
{
    public OrderStatusProfile()
    {
        CreateMap<Domain.Entity.Inventory.OrderStatus, OrderStatusResponse>();

        CreateMap<Domain.Entity.Inventory.OrderStatus, OrderStatusUpdatedResponse>();

        CreateMap<Domain.Entity.Inventory.OrderStatus, OrderStatusCreatedResponse>();
    }
}