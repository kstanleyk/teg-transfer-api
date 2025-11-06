using AutoMapper;
using MediatR;
using TegWallet.Application.Features.Core.ClientGroups.Dtos;
using TegWallet.Application.Helpers;
using TegWallet.Application.Interfaces.Core;

namespace TegWallet.Application.Features.Core.ClientGroups.Queries;

// Get all ClientGroups (with optional filtering)
public record GetAllClientGroupsQuery(
    bool? IsActive = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedResponse<ClientGroupDto>>>;

public class GetAllClientGroupsQueryHandler(
    IClientGroupRepository clientGroupRepository,
    IMapper mapper)
    : IRequestHandler<GetAllClientGroupsQuery, Result<PagedResponse<ClientGroupDto>>>
{
    private readonly IClientGroupRepository _clientGroupRepository = clientGroupRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PagedResponse<ClientGroupDto>>> Handle(GetAllClientGroupsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _clientGroupRepository.GetAllAsync(
                query.IsActive,
                query.SearchTerm,
                query.PageNumber,
                query.PageSize);

            var clientGroupDtos = _mapper.Map<List<ClientGroupDto>>(result.Items);

            var response = new PagedResponse<ClientGroupDto>
            {
                Items = clientGroupDtos,
                Page = result.Page,           // Updated property name
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
                // TotalPages, HasPrevious, HasNext are computed properties
            };

            return Result<PagedResponse<ClientGroupDto>>.Succeeded(response);
        }
        catch (Exception ex)
        {
            return Result<PagedResponse<ClientGroupDto>>.Failed($"An error occurred while retrieving client groups: {ex.Message}");
        }
    }
}