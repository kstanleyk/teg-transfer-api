using AutoMapper;
using Transfer.Application.Features.Inventory.OrderStatus.Dtos;

namespace Transfer.Application.Features.Inventory.OrderStatus;

public class OrderStatusProfile : Profile
{
    public OrderStatusProfile()
    {
        CreateMap<Transfer.Domain.Entity.Inventory.OrderStatus, OrderStatusResponse>();

        CreateMap<Transfer.Domain.Entity.Inventory.OrderStatus, OrderStatusUpdatedResponse>();

        CreateMap<Transfer.Domain.Entity.Inventory.OrderStatus, OrderStatusCreatedResponse>();
    }
}