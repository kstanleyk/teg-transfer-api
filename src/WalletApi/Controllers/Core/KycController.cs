using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Features.Kyc.Command;
using TegWallet.Application.Helpers;
using TegWallet.WalletApi.Attributes;


namespace TegWallet.WalletApi.Controllers.Core;

[ApiVersion("1.0")]
public class KycController(IMediator mediator) : ApiControllerBase<KycController>
{
    public IMediator Mediator { get; } = mediator;

    [MapToApiVersion("1.0")]
    [HttpPost("{clientId:guid}/start")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [MustMatchClient]
    public async Task<IActionResult> StartKycProcess(Guid clientId)
    {
        var command = new StartKycProcessCommand(clientId);
        var result = await Mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

}