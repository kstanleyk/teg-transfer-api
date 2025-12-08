using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.Clients.Dto;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.Clients.Query;

public record GetClientsQuery : IRequest<Result<ClientDto[]>>;

public class GetClientsQueryHandler(
    IClientRepository clientRepository,
    IMapper mapper)
    :RequestHandlerBase, IRequestHandler<GetClientsQuery, Result<ClientDto[]>>
{
    public async Task<Result<ClientDto[]>> Handle(GetClientsQuery query, CancellationToken cancellationToken)
    {
        var clients = await clientRepository.GetAllAsync();

        return Result<ClientDto[]>.Succeeded(mapper.Map<ClientDto[]>(clients),"Clients retrieved successfully.");
    }

    protected override void DisposeCore()
    {
        clientRepository.Dispose();
    }
}