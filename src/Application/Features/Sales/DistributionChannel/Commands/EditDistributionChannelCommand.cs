using Agrovet.Application.Features.Sales.DistributionChannel.Dtos;
using Agrovet.Application.Helpers;
using Agrovet.Application.Helpers.Exceptions;
using Agrovet.Application.Interfaces.Sales;
using AutoMapper;
using MediatR;

namespace Agrovet.Application.Features.Sales.DistributionChannel.Commands;

public class EditDistributionChannelCommandResponse : BaseResponse
{
    public DistributionChannelUpdatedResponse Data { get; set; } = null!;
}

public class EditDistributionChannelCommand : IRequest<EditDistributionChannelCommandResponse>
{
    public required EditDistributionChannelRequest DistributionChannel { get; set; }
}

public class EditDistributionChannelCommandHandler(IDistributionChannelRepository distributionChannelRepository, IMapper mapper) 
    : RequestHandlerBase, IRequestHandler<EditDistributionChannelCommand, EditDistributionChannelCommandResponse>
{
    public async Task<EditDistributionChannelCommandResponse> Handle(EditDistributionChannelCommand request, 
        CancellationToken cancellationToken)
    {
        var response = new EditDistributionChannelCommandResponse();

        var validator = new EditDistributionChannelCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        var icr = request.DistributionChannel;

        var distributionChannel = Domain.Entity.Sales.DistributionChannel.Create(icr.Name);
        distributionChannel.SetId(icr.Id);
        distributionChannel.SetPublicId(icr.PublicId);

        var result = await distributionChannelRepository.UpdateAsyncAsync(icr.PublicId, distributionChannel);

        if (result.Status != RepositoryActionStatus.Updated && 
            result.Status != RepositoryActionStatus.NothingModified)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<DistributionChannelUpdatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        distributionChannelRepository.Dispose();
    }
}