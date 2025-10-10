using AutoMapper;
using MediatR;
using Transfer.Application.Features.Sales.DistributionChannel.Dtos;
using Transfer.Application.Helpers;
using Transfer.Application.Helpers.Exceptions;
using Transfer.Application.Interfaces.Sales;
using Transfer.Domain.ValueObjects;

namespace Transfer.Application.Features.Sales.DistributionChannel.Commands;

public class CreateDistributionChannelCommandResponse : BaseResponse
{
    public DistributionChannelCreatedResponse Data { get; set; } = null!;
}

public class CreateDistributionChannelCommand : IRequest<CreateDistributionChannelCommandResponse>
{
    public required CreateDistributionChannelRequest DistributionChannel { get; set; }
}

public class CreateDistributionChannelCommandHandler(IDistributionChannelRepository distributionChannelRepository, IMapper mapper)
    :
        RequestHandlerBase, IRequestHandler<CreateDistributionChannelCommand, CreateDistributionChannelCommandResponse>
{
    public async Task<CreateDistributionChannelCommandResponse> Handle(CreateDistributionChannelCommand request,
        CancellationToken cancellationToken)
    {
        var response = new CreateDistributionChannelCommandResponse();

        var validator = new CreateDistributionChannelCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToList();

            throw new ValidationException(response.ValidationErrors);
        }

        if (request.DistributionChannel == null)
            throw new ArgumentNullException(nameof(request.DistributionChannel));

        var icr = request.DistributionChannel;

        var distributionChannel = Transfer.Domain.Entity.Sales.DistributionChannel.Create(icr.Name);

        distributionChannel.SetPublicId(PublicId.CreateUnique().Value);

        var result = await distributionChannelRepository.AddAsync(distributionChannel);

        if (result.Status != RepositoryActionStatus.Created)
        {
            response.Success = false;
            return response;
        }

        response.Data = mapper.Map<DistributionChannelCreatedResponse>(result.Entity);

        return response;
    }

    protected override void DisposeCore()
    {
        distributionChannelRepository.Dispose();
    }
}