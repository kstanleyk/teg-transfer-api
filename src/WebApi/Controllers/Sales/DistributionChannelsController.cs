using Agrovet.Application.Authorization;
using Agrovet.Application.Features.Sales.DistributionChannel.Commands;
using Agrovet.Application.Features.Sales.DistributionChannel.Dtos;
using Agrovet.Application.Features.Sales.DistributionChannel.Queries;
using Agrovet.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers.Sales;

public class DistributionChannelsController(IMediator mediator) : ApiControllerBase<DistributionChannelsController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.DistributionChannel, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new DistributionChannelsQuery())));

    [HttpGet("{orderTypeId:guid}")]
    [MustHavePermission(AppFeature.DistributionChannel, AppAction.Read)]
    public async Task<IActionResult> Get(Guid orderTypeId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new DistributionChannelQuery { PublicId = orderTypeId })));

    [HttpPost]
    [MustHavePermission(AppFeature.DistributionChannel, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateDistributionChannelRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateDistributionChannelCommand { DistributionChannel = request })));

    [HttpPut]
    [MustHavePermission(AppFeature.DistributionChannel, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditDistributionChannelRequest request) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditDistributionChannelCommand { DistributionChannel = request })));
}