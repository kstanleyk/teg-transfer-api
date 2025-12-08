using System.Text.RegularExpressions;

namespace TegWallet.CoreApi.Controllers;

public class KebabCaseRouteTransformer : IOutboundParameterTransformer
{
    public string TransformOutbound(object? value)
        => value == null
            ? null
            : Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1-$2").ToLower();
}