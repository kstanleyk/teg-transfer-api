using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.ClientGroups.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.ClientGroups.Queries;

public record GetAllClientGroupsWithoutPaginationQuery(
    bool? IsActive = null,
    string? SearchTerm = null) : IRequest<Result<IReadOnlyList<ClientGroupDto>>>;

public class GetAllClientGroupsWithoutPaginationQueryHandler(
    IClientGroupRepository clientGroupRepository,
    IMapper mapper)
    : IRequestHandler<GetAllClientGroupsWithoutPaginationQuery, Result<IReadOnlyList<ClientGroupDto>>>
{
    private readonly IClientGroupRepository _clientGroupRepository = clientGroupRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<IReadOnlyList<ClientGroupDto>>> Handle(
        GetAllClientGroupsWithoutPaginationQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var clientGroups = await _clientGroupRepository.GetAllWithoutPaginationAsync(
                query.IsActive,
                query.SearchTerm);

            var clientGroupDtos = _mapper.Map<IReadOnlyList<ClientGroupDto>>(clientGroups);

            return Result<IReadOnlyList<ClientGroupDto>>.Succeeded(clientGroupDtos);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ClientGroupDto>>.Failed($"An error occurred while retrieving client groups: {ex.Message}");
        }
    }
}