using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TegWallet.Application.Features.Core.Currency.Query;

namespace TegWallet.WalletApi.Controllers.Core;

[ApiVersion("1.0")]
public class CurrenciesController : ApiControllerBase<CurrenciesController>
{
    [MapToApiVersion("1.0")]
    [HttpGet()]
    public async Task<IActionResult> GetCurrenciesV1()
    {
        var query = new CurrenciesQuery();
        var result = await MediatorSender.Send(query);
        return Ok(result);
    }
}
