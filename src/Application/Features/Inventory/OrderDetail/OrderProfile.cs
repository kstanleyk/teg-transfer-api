using Agrovet.Application.Features.Inventory.Order.Dtos;
using AutoMapper;

namespace Agrovet.Application.Features.Inventory.OrderDetail;

public class OrderDetailProfile : Profile
{
    public OrderDetailProfile()
    {
        CreateMap<Domain.Entity.Inventory.OrderDetail, OrderDetailResponse>();
        //CreateMap<Domain.Entity.Inventory.OrderDetail, OrderDetailCreatedResponse>();
        //CreateMap<Domain.Entity.Inventory.OrderDetail, OrderDetailUpdatedResponse>();
    }
}