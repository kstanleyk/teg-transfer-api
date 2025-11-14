using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.ClientGroups.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.ClientGroups.Queries;

// Get ClientGroupId by ID
public record GetClientGroupByIdQuery(Guid ClientGroupId) : IRequest<Result<ClientGroupDto>>;

public class GetClientGroupByIdQueryHandler(
    IClientGroupRepository clientGroupRepository,
    IMapper mapper)
    : IRequestHandler<GetClientGroupByIdQuery, Result<ClientGroupDto>>
{
    private readonly IClientGroupRepository _clientGroupRepository = clientGroupRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ClientGroupDto>> Handle(GetClientGroupByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var clientGroup = await _clientGroupRepository.GetAsync(query.ClientGroupId);
            if (clientGroup == null)
                return Result<ClientGroupDto>.Failed("Client group not found");

            var clientGroupDto = _mapper.Map<ClientGroupDto>(clientGroup);
            return Result<ClientGroupDto>.Succeeded(clientGroupDto);
        }
        catch (Exception ex)
        {
            return Result<ClientGroupDto>.Failed($"An error occurred while retrieving client group: {ex.Message}");
        }
    }
}
