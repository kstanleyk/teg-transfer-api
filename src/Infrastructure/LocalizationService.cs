using Microsoft.Extensions.Localization;
using TegWallet.Application.Interfaces;

namespace TegWallet.Infrastructure;

public class LocalizationService(IStringLocalizerFactory factory) : ILocalizationService
{
    private readonly IStringLocalizer _localizer = factory.Create(typeof(SharedResource));

    public string GetString(string key) => _localizer[key];

    internal class SharedResource { }
}