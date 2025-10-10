using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transfer.Application.Features.Client.Commands;
using Transfer.Application.Features.Client.Dto;
using Transfer.Domain.Entity.Core;

namespace Transfer.Application.Features.Client;

public class ClientProfile : Profile
{
    public ClientProfile()
    {
        // RegisterClientDto to RegisterClientCommand
        CreateMap<RegisterClientDto, RegisterClientCommand>();

        // Client to ClientRegisteredDto
        CreateMap<Domain.Entity.Core.Client, ClientRegisteredDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

        // Wallet to WalletCreatedDto
        CreateMap<Wallet, WalletCreatedDto>()
            .ForMember(dest => dest.Balance,
                opt => opt.MapFrom(src => src.Balance.Amount))
            .ForMember(dest => dest.AvailableBalance,
                opt => opt.MapFrom(src => src.AvailableBalance.Amount))
            .ForMember(dest => dest.BaseCurrencyCode,
                opt => opt.MapFrom(src => src.BaseCurrency.Code));
    }
}