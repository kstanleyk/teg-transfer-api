using AutoMapper;
using TegWallet.Application.Features.Core.ClientGroups.Dtos;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.ClientGroups;

public class ClientGroupProfile : Profile
{
    public ClientGroupProfile()
    {
        // ClientGroup to ClientGroupDto
        CreateMap<ClientGroup, ClientGroupDto>()
            .ForMember(dest => dest.ClientCount, opt => opt.MapFrom(src => src.Clients.Count));

        // ClientGroup to ClientGroupWithClientsDto
        CreateMap<ClientGroup, ClientGroupWithClientsDto>()
            .ForMember(dest => dest.Clients, opt => opt.MapFrom(src => src.Clients));

        // Client to ClientGroupClientDto
        CreateMap<Client, ClientGroupClientDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}