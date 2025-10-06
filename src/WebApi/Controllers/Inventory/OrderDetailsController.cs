using Agrovet.Application.Features.Inventory.OrderDetail.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Agrovet.WebApi.Controllers.Inventory;

public class OrderDetailsController(IMediator mediator) : ApiControllerBase<OrderDetailsController>
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return await GetActionResult(async () =>
        {
            var query = new OrderDetailsQuery();
            var orderDetails = await mediator.Send(query);
            return Ok(orderDetails);
        });
    }

    [HttpGet]
    [Route("{code:guid}")]
    public async Task<IActionResult> Get(Guid code)
    {
        return await GetActionResult(async () =>
        {
            var query = new OrderDetailQuery
            {
                PublicId = code
            };
            var orderDetail = await mediator.Send(query);
            return Ok(orderDetail);
        });
    }
}
