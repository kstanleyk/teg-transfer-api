using AutoMapper;
using Transfer.Application.Features.Inventory.OrderDetail.Dtos;

namespace Transfer.Application.Features.Inventory.OrderDetail;

public class OrderDetailProfile : Profile
{
    public OrderDetailProfile()
    {
        CreateMap<Transfer.Domain.Entity.Inventory.OrderDetail, OrderDetailResponse>()
            .ForMember(dest => dest.PublicId, opt => opt.MapFrom(src => src.PublicId))
            .ForMember(dest => dest.PackagingType, opt => opt.MapFrom(src => src.PackagingType.Id))
            .ForMember(dest => dest.PackagingDisplayName, opt => opt.MapFrom(src => src.PackagingType.DisplayName))
            .ForMember(dest => dest.UnitsPerBox, opt => opt.MapFrom(src => src.PackagingType.UnitsPerBox))
            .ForMember(dest => dest.BottlingSizeInLiters, opt => opt.MapFrom(src => src.PackagingType.BottlingType.SizeInLiters))
            .ForMember(dest => dest.BottlingDisplayName, opt => opt.MapFrom(src => src.PackagingType.BottlingType.DisplayName));
        //CreateMap<Domain.Entity.Inventory.OrderDetail, OrderDetailCreatedResponse>();
        //CreateMap<Domain.Entity.Inventory.OrderDetail, OrderDetailUpdatedResponse>();
    }
}