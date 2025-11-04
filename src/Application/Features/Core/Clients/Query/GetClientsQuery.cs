using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TegWallet.Application.Features.Core.Clients.Dto;
using TegWallet.Application.Helpers;

namespace TegWallet.Application.Features.Core.Clients.Query;

public record GetClientsQuery : IRequest<Result<ClientDto[]>>;

public class GetClientsQueryHandler(
    UserManager<Domain.Entity.Core.Client> userManager,
    IMapper mapper)
    : IRequestHandler<GetClientsQuery, Result<ClientDto[]>>
{
    private readonly UserManager<Domain.Entity.Core.Client> _userManager = userManager;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ClientDto[]>> Handle(GetClientsQuery query, CancellationToken cancellationToken)
    {
        var clients = await _userManager.Users.ToArrayAsync(cancellationToken: cancellationToken);

        return Result<ClientDto[]>.Succeeded(_mapper.Map<ClientDto[]>(clients),"Clients retrieved successfully.");
    }
}