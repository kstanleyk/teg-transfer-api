using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Core.Test.ExchangeRateTests;

public class ExchangeRateTestBuilder
{
    private Currency _baseCurrency = Currency.XAF;
    private Currency _targetCurrency = Currency.USD;
    private decimal _baseCurrencyValue = 575.50m;  // 1 USD = 575.50 XOF
    private decimal _targetCurrencyValue = 1.0m;   // 1 USD = 1.00 USD
    private decimal _margin = 0.05m;               // 5% margin
    private DateTime _effectiveFrom = DateTime.UtcNow.AddHours(1);
    private DateTime? _effectiveTo;
    private string _createdBy = "system";
    private string _source = "Market";
    private Guid? _clientId;
    private Guid? _clientGroupId;

    public ExchangeRateTestBuilder WithBaseCurrency(Currency baseCurrency)
    {
        _baseCurrency = baseCurrency;
        return this;
    }

    public ExchangeRateTestBuilder WithTargetCurrency(Currency targetCurrency)
    {
        _targetCurrency = targetCurrency;
        return this;
    }

    public ExchangeRateTestBuilder WithBaseCurrencyValue(decimal baseCurrencyValue)
    {
        _baseCurrencyValue = baseCurrencyValue;
        return this;
    }

    public ExchangeRateTestBuilder WithTargetCurrencyValue(decimal targetCurrencyValue)
    {
        _targetCurrencyValue = targetCurrencyValue;
        return this;
    }

    public ExchangeRateTestBuilder WithMargin(decimal margin)
    {
        _margin = margin;
        return this;
    }

    public ExchangeRateTestBuilder WithEffectiveFrom(DateTime effectiveFrom)
    {
        _effectiveFrom = effectiveFrom;
        return this;
    }

    public ExchangeRateTestBuilder WithEffectiveTo(DateTime? effectiveTo)
    {
        _effectiveTo = effectiveTo;
        return this;
    }

    public ExchangeRateTestBuilder WithCreatedBy(string createdBy)
    {
        _createdBy = createdBy;
        return this;
    }

    public ExchangeRateTestBuilder WithSource(string source)
    {
        _source = source;
        return this;
    }

    public ExchangeRateTestBuilder WithClientId(Guid? clientId)
    {
        _clientId = clientId;
        return this;
    }

    public ExchangeRateTestBuilder WithClientGroupId(Guid? clientGroupId)
    {
        _clientGroupId = clientGroupId;
        return this;
    }

    public ExchangeRate BuildGeneralRate()
    {
        return ExchangeRate.CreateGeneralRate(
            _baseCurrency,
            _targetCurrency,
            _baseCurrencyValue,
            _targetCurrencyValue,
            _margin,
            _effectiveFrom,
            _createdBy,
            _source,
            _effectiveTo
        );
    }

    public ExchangeRate BuildGroupRate()
    {
        if (!_clientGroupId.HasValue)
            _clientGroupId = Guid.NewGuid();

        return ExchangeRate.CreateGroupRate(
            _baseCurrency,
            _targetCurrency,
            _baseCurrencyValue,
            _targetCurrencyValue,
            _margin,
            _clientGroupId.Value,
            _effectiveFrom,
            _createdBy,
            _source,
            _effectiveTo
        );
    }

    public ExchangeRate BuildIndividualRate()
    {
        if (!_clientId.HasValue)
            _clientId = Guid.NewGuid();

        return ExchangeRate.CreateIndividualRate(
            _baseCurrency,
            _targetCurrency,
            _baseCurrencyValue,
            _targetCurrencyValue,
            _margin,
            _clientId.Value,
            _effectiveFrom,
            _createdBy,
            _source,
            _effectiveTo
        );
    }

    // Predefined factory methods for common scenarios

    public static ExchangeRate CreateActiveGeneralRate() =>
        new ExchangeRateTestBuilder().BuildGeneralRate();

    public static ExchangeRate CreateInactiveGeneralRate()
    {
        var rate = new ExchangeRateTestBuilder().BuildGeneralRate();
        rate.Deactivate();
        return rate;
    }

    public static ExchangeRate CreateGroupRate(Guid clientGroupId) =>
        new ExchangeRateTestBuilder()
            .WithClientGroupId(clientGroupId)
            .BuildGroupRate();

    public static ExchangeRate CreateIndividualRate(Guid clientId) =>
        new ExchangeRateTestBuilder()
            .WithClientId(clientId)
            .BuildIndividualRate();

    public static ExchangeRate CreateRateWithHighMargin() =>
        new ExchangeRateTestBuilder()
            .WithMargin(0.25m) // 25% margin
            .BuildGeneralRate();

    public static ExchangeRate CreateRateWithNoMargin() =>
        new ExchangeRateTestBuilder()
            .WithMargin(0.0m)
            .BuildGeneralRate();

    public static ExchangeRate CreateHistoricalRate()
    {
        var rate = new ExchangeRateTestBuilder()
            .WithEffectiveFrom(DateTime.UtcNow.AddDays(-30))
            .WithEffectiveTo(DateTime.UtcNow.AddDays(-1))
            .BuildGeneralRate();
        rate.Deactivate();
        return rate;
    }

    public static ExchangeRate CreateFutureRate() =>
        new ExchangeRateTestBuilder()
            .WithEffectiveFrom(DateTime.UtcNow.AddDays(1))
            .BuildGeneralRate();

    public static ExchangeRate CreateRateWithDifferentCurrencies(Currency baseCurrency, Currency targetCurrency) =>
        new ExchangeRateTestBuilder()
            .WithBaseCurrency(baseCurrency)
            .WithTargetCurrency(targetCurrency)
            .BuildGeneralRate();

    // Method to create multiple rates for testing bulk operations
    public static List<ExchangeRate> CreateMultipleRates(int count, Action<ExchangeRateTestBuilder, int>? configure = null)
    {
        var rates = new List<ExchangeRate>();
        for (int i = 0; i < count; i++)
        {
            var builder = new ExchangeRateTestBuilder()
                .WithBaseCurrencyValue(575.50m + i)
                .WithTargetCurrencyValue(1.0m);

            configure?.Invoke(builder, i);
            rates.Add(builder.BuildGeneralRate());
        }
        return rates;
    }
}