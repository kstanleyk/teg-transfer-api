using FluentValidation;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Application.Features.Core.ExchangeRate.Validator;

public static class CurrencyValidationRules
{
    public static IRuleBuilderOptions<T, Currency> ValidateCurrency<T>(this IRuleBuilder<T, Currency> ruleBuilder)
    {
        return ruleBuilder
            .NotNull()
            .WithMessage("Currency is required")
            .Must(currency => currency != null && Currency.All.Contains(currency))
            .WithMessage("Unsupported currency");
    }

    public static IRuleBuilderOptions<T, decimal> ValidateCurrencyValue<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage("Currency value must be positive")
            .ScalePrecision(4, 18)
            .WithMessage("Currency value must have at most 4 decimal places and 18 total digits");
    }

    public static IRuleBuilderOptions<T, decimal> ValidateMargin<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(0, 1)
            .WithMessage("Margin must be between 0 and 1")
            .ScalePrecision(4, 6)
            .WithMessage("Margin must have at most 4 decimal places");
    }

    public static IRuleBuilderOptions<T, string> ValidateCurrencyCode<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Currency code is required")
            .Must(code => !string.IsNullOrWhiteSpace(code) && IsValidCurrencyCode(code))
            .WithMessage("Unsupported currency code. Supported codes: USD, NGN, XOF, CNY")
            .Length(3)
            .WithMessage("Currency code must be exactly 3 characters");
    }

    public static IRuleBuilderOptions<T, Guid> ValidateNotEmptyGuid<T>(this IRuleBuilder<T, Guid> ruleBuilder, string fieldName)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage($"{fieldName} is required")
            .NotEqual(Guid.Empty)
            .WithMessage($"{fieldName} must be a valid GUID");
    }

    private static bool IsValidCurrencyCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var normalizedCode = code.Trim().ToUpperInvariant();
        return normalizedCode switch
        {
            "USD" or "NGN" or "XOF" or "CNY" => true,
            _ => false
        };
    }
}