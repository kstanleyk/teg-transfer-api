using AutoMapper;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.Purchases;

public class PurchaseProfile : Profile
{
    public PurchaseProfile()
    {
        CreateMap<Reservation, ReservationDto>();
        CreateMap<Reservation, ReservedPurchaseDto>()
            .ForMember(dest => dest.PurchaseLedgerId, opt => opt.MapFrom(src => src.PurchaseLedgerId))
            .ForMember(dest => dest.ServiceFeeLedgerId, opt => opt.MapFrom(src => src.ServiceFeeLedgerId));
    }
}