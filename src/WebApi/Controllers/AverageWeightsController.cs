using Agrovet.Application.Authorization;
using Agrovet.Application.Features.Core.AverageWeight.Commands;
using Agrovet.Application.Features.Core.AverageWeight.Dtos;
using Agrovet.Application.Features.Core.AverageWeight.Queries;
using Agrovet.WebApi.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers;

public class AverageWeightsController(IMediator mediator) : ApiControllerBase<AverageWeightsController>
{
    public IMediator Mediator { get; } = mediator;

    [HttpGet]
    [MustHavePermission(AppFeature.AverageWeight, AppAction.Read)]
    public async Task<IActionResult> Get() =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new AverageWeightsQuery())));

    [HttpGet("{departmentId}")]
    [MustHavePermission(AppFeature.AverageWeight, AppAction.Read)]
    public async Task<IActionResult> Get(string departmentId) =>
        await GetActionResult(async () => Ok(await MediatorSender.Send(new AverageWeightQuery { Id = departmentId })));

    [HttpPost]
    [MustHavePermission(AppFeature.AverageWeight, AppAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateAverageWeightRequest dto) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new CreateAverageWeightCommand { AverageWeight = dto })));

    [HttpPut]
    [MustHavePermission(AppFeature.AverageWeight, AppAction.Update)]
    public async Task<IActionResult> Update([FromBody] EditAverageWeightRequest dto) => await GetActionResult(async () =>
        Ok(await MediatorSender.Send(new EditAverageWeightCommand { AverageWeight = dto })));
}