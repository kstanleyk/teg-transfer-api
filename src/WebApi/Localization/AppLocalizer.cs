using Microsoft.Extensions.Localization;
using System.Reflection;
using TegWallet.Application.Interfaces.Localization;

namespace TegWallet.WebApi.Localization;

public class AppLocalizer : IAppLocalizer
{
    private readonly IStringLocalizer _localizer;

    public AppLocalizer(IStringLocalizerFactory factory)
    {
        // SharedResource is just a marker class
        //var type = typeof(SharedResource);
        //_localizer = factory.Create(type);
        _localizer = factory.Create("SharedResource", Assembly.GetExecutingAssembly().GetName().Name);
    }

    public string this[string key] => _localizer[key];

    public string GetString(string key, params object[] arguments)
    {
        var value = _localizer[key];
        return arguments.Length == 0 ? value.Value : string.Format(value.Value, arguments);
    }
}

public class SharedResource { }