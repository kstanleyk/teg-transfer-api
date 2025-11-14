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
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ClientDto[]>> Handle(GetClientsQuery query, CancellationToken cancellationToken)
    {
        var clients = await _clientRepository.GetAllAsync();

        return Result<ClientDto[]>.Succeeded(_mapper.Map<ClientDto[]>(clients),"Clients retrieved successfully.");
    }

    protected override void DisposeCore()
    {
        _clientRepository.Dispose();
    }
}