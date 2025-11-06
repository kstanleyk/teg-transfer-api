using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.ClientGroups.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.ClientGroups.Queries;

// Get ClientGroups by status
public record GetClientGroupsByStatusQuery(bool IsActive) : IRequest<Result<IReadOnlyList<ClientGroupDto>>>;

public class GetClientGroupsByStatusQueryHandler(
    IClientGroupRepository clientGroupRepository,
    IMapper mapper)
    : IRequestHandler<GetClientGroupsByStatusQuery, Result<IReadOnlyList<ClientGroupDto>>>
{
    private readonly IClientGroupRepository _clientGroupRepository = clientGroupRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<IReadOnlyList<ClientGroupDto>>> Handle(GetClientGroupsByStatusQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var clientGroups = await _clientGroupRepository.GetByStatusAsync(query.IsActive);
            var clientGroupDtos = _mapper.Map<IReadOnlyList<ClientGroupDto>>(clientGroups);

            return Result<IReadOnlyList<ClientGroupDto>>.Succeeded(clientGroupDtos);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ClientGroupDto>>.Failed($"An error occurred while retrieving client groups: {ex.Message}");
        }
    }
}