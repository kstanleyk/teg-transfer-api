using AutoMapper;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.Wallets;

public class PurchaseReservationProfile : Profile
{
    public PurchaseReservationProfile()
    {
        // Existing mappings...

        // Reservation to ReservationDto mapping
        CreateMap<Reservation, ReservationDto>()
            .ForMember(dest => dest.PurchaseAmount, opt => opt.MapFrom(src => src.PurchaseAmount.Amount))
            .ForMember(dest => dest.ServiceFeeAmount, opt => opt.MapFrom(src => src.ServiceFeeAmount.Amount))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.PurchaseAmount.Currency.Code))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DaysPending, opt => opt.MapFrom(src =>
                src.Status == ReservationStatus.Pending
                    ? (DateTime.UtcNow - src.CreatedAt).Days
                    : (int?)null))
            .ForMember(dest => dest.CanBeApproved, opt => opt.MapFrom(src =>
                src.Status == ReservationStatus.Pending))
            .ForMember(dest => dest.CanBeCancelled, opt => opt.MapFrom(src =>
                src.Status == ReservationStatus.Pending));

        // PagedResult to PagedResponse mapping
        CreateMap<PagedResult<Reservation>, PagedResponse<ReservationDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
            .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
            .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.TotalCount))
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages))
            .ForMember(dest => dest.HasPrevious, opt => opt.MapFrom(src => src.HasPrevious))
            .ForMember(dest => dest.HasNext, opt => opt.MapFrom(src => src.HasNext));
    }
}