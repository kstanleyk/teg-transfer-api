using AutoMapper;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Application.Features.Core.Wallets.Dto;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.ExchangeRates;

public class ClientWithExchangeRateProfile : Profile
{
    public ClientWithExchangeRateProfile()
    {
        CreateMap<Client, ClientWithExchangeRateDto>()
            .ForMember(dest => dest.ClientGroupName, opt => opt.MapFrom(src => src.ClientGroup != null ? src.ClientGroup.Name : null));

        CreateMap<Wallet, WalletDto>()
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance.Amount))
            .ForMember(dest => dest.AvailableBalance, opt => opt.MapFrom(src => src.AvailableBalance.Amount));

        CreateMap<ExchangeRate, ExchangeRateDto>();
    }
}