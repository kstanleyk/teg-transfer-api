using TegWallet.Application.Features.Core.Currencies.Dto;
using TegWallet.Application.Features.Core.ExchangeRates.Dtos;
using TegWallet.Domain.Entity.Core;

namespace TegWallet.Application.Features.Core.ExchangeRates.Queries;

public abstract class ClientWithExchangeRateHandlerBase
{
    protected ClientWithExchangeRateDto MapDto(Client client, ExchangeRate? exchangeRate)
    {
        var dto = new ClientWithExchangeRateDto
        {
            ClientId = client.Id,
            WalletId = client.Wallet.Id,
            ClientGroupId = client.ClientGroupId,
            ClientGroupName = client.ClientGroup?.Name
        };

        // Map exchange rate properties if available
        if (exchangeRate == null) return dto;

        var baseCurrency = new CurrencyDto
        {
            Code = exchangeRate.BaseCurrency.Code,
            Symbol = exchangeRate.BaseCurrency.Symbol,
            DecimalPlaces = exchangeRate.BaseCurrency.DecimalPlaces
        };

        var targetCurrency = new CurrencyDto
        {
            Code = exchangeRate.TargetCurrency.Code,
            Symbol = exchangeRate.TargetCurrency.Symbol,
            DecimalPlaces = exchangeRate.TargetCurrency.DecimalPlaces
        };

        dto.ExchangeRateId = exchangeRate.Id;
        dto.ExchangeRateType = exchangeRate.Type.ToString();
        dto.ExchangeRateBaseCurrency = baseCurrency;
        dto.ExchangeRateTargetCurrency = targetCurrency;
        dto.MarketRate = exchangeRate.MarketRate;
        dto.EffectiveRate = exchangeRate.EffectiveRate;
        dto.Margin = exchangeRate.Margin;
        dto.ExchangeRateEffectiveFrom = exchangeRate.EffectiveFrom;
        dto.ExchangeRateEffectiveTo = exchangeRate.EffectiveTo;
        dto.IsExchangeRateActive = exchangeRate.IsActive;
        dto.ExchangeRateSource = exchangeRate.Source;
        dto.ExchangeRateDescription = exchangeRate.GetRateDescription();
        dto.ExchangeRateInverseDescription = exchangeRate.GetInverseRateDescription();
        dto.ExchangeRateShortDescription = exchangeRate.GetRateShortDescription();
        dto.ExchangeRateInverseShortDescription = exchangeRate.GetInverseRateShortDescription();

        return dto;
    }
}