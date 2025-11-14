using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.ClientGroups.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.ClientGroups.Queries;


// Get ClientGroupId with Clients (includes client count and details)
public record GetClientGroupWithClientsQuery(Guid ClientGroupId) : IRequest<Result<ClientGroupWithClientsDto>>;

public class GetClientGroupWithClientsQueryHandler(
    IClientGroupRepository clientGroupRepository,
    IMapper mapper)
    : IRequestHandler<GetClientGroupWithClientsQuery, Result<ClientGroupWithClientsDto>>
{
    private readonly IClientGroupRepository _clientGroupRepository = clientGroupRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ClientGroupWithClientsDto>> Handle(GetClientGroupWithClientsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var clientGroup = await _clientGroupRepository.GetByIdWithClientsAsync(query.ClientGroupId);
            if (clientGroup == null)
                return Result<ClientGroupWithClientsDto>.Failed("Client group not found");

            var clientGroupDto = _mapper.Map<ClientGroupWithClientsDto>(clientGroup);
            return Result<ClientGroupWithClientsDto>.Succeeded(clientGroupDto);
        }
        catch (Exception ex)
        {
            return Result<ClientGroupWithClientsDto>.Failed($"An error occurred while retrieving client group with clients: {ex.Message}");
        }
    }
}
