using AutoMapper;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.ExchangeRates;

public class ExchangeRateProfile : Profile
{
    public ExchangeRateProfile()
    {
        CreateMap<ExchangeRate, ExchangeRateDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.BaseCurrency, opt => opt.MapFrom(src => src.BaseCurrency))
            .ForMember(dest => dest.TargetCurrency, opt => opt.MapFrom(src => src.TargetCurrency))
            .ForMember(dest => dest.MarketRate, opt => opt.MapFrom(src => src.MarketRate))
            .ForMember(dest => dest.EffectiveRate, opt => opt.MapFrom(src => src.EffectiveRate))
            .ForMember(dest => dest.Margin, opt => opt.MapFrom(src => src.Margin))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.EffectiveFrom, opt => opt.MapFrom(src => src.EffectiveFrom))
            .ForMember(dest => dest.EffectiveTo, opt => opt.MapFrom(src => src.EffectiveTo))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
            .ForMember(dest => dest.ExchangeRateDescription, opt => opt.MapFrom(src => src.GetRateDescription()))
            .ForMember(dest => dest.ExchangeRateInverseDescription, opt => opt.MapFrom(src => src.GetInverseRateDescription()))
            .ForMember(dest => dest.ExchangeRateShortDescription, opt => opt.MapFrom(src => src.GetRateShortDescription()))
            .ForMember(dest => dest.ExchangeRateInverseShortDescription, opt => opt.MapFrom(src => src.GetInverseRateShortDescription()))
            .ForMember(dest => dest.ClientGroupName, opt => opt.MapFrom(src => src.ClientGroup != null ? src.ClientGroup.Name : null))
            .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null ? src.Client.FullName : null))
            .ForMember(dest => dest.RateTypeDescription, opt => opt.MapFrom(src => GetRateTypeDescription(src.Type)));
    }

    private static string GetRateTypeDescription(RateType type)
    {
        return type switch
        {
            RateType.General => "General Rate",
            RateType.Group => "Group Rate",
            RateType.Individual => "Individual Rate",
            _ => "Unknown Rate Type"
        };
    }
}