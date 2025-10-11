using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TegWallet.Application.Features.Core.Wallet.Dto;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.Wallet;

public class PurchaseProfile : Profile
{
    public PurchaseProfile()
    {
        CreateMap<PurchaseReservation, PurchaseReservationDto>();
        CreateMap<PurchaseReservation, ReservedPurchaseDto>()
            .ForMember(dest => dest.PurchaseLedgerId, opt => opt.MapFrom(src => src.PurchaseLedgerId.Value))
            .ForMember(dest => dest.ServiceFeeLedgerId, opt => opt.MapFrom(src => src.ServiceFeeLedgerId.Value));
    }
}