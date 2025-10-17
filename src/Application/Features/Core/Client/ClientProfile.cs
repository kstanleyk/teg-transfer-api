using AutoMapper;
using TegWallet.Application.Features.Core.Client.Commands;
using TegWallet.Application.Features.Core.Client.Dto;

namespace TegWallet.Application.Features.Core.Client;

public class ClientProfile : Profile
{
    public ClientProfile()
    {
        // RegisterClientDto to RegisterClientCommand
        CreateMap<RegisterClientDto, RegisterClientCommand>();
        CreateMap<Domain.Entity.Core.Client, ClientDto>();

        // Client to ClientRegisteredDto
        CreateMap<Domain.Entity.Core.Client, ClientRegisteredDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

        // Wallet to WalletCreatedDto
        CreateMap<Domain.Entity.Core.Wallet, WalletCreatedDto>()
            .ForMember(dest => dest.Balance,
                opt => opt.MapFrom(src => src.Balance.Amount))
            .ForMember(dest => dest.AvailableBalance,
                opt => opt.MapFrom(src => src.AvailableBalance.Amount))
            .ForMember(dest => dest.BaseCurrencyCode,
                opt => opt.MapFrom(src => src.BaseCurrency.Code));
    }
}