using FluentAssertions;
using TegWallet.Domain.Entity.Core;
using TegWallet.Domain.Exceptions;
using TegWallet.Domain.ValueObjects;

namespace TegWallet.Core.Test.ExchangeRateTests;

public class ExchangeRateTests
{
    private readonly Currency _xof = Currency.XAF;
    private readonly Currency _usd = Currency.USD;
    private readonly decimal _validBaseValue = 575.50m;  // 1 USD = 575.50 XOF
    private readonly decimal _validTargetValue = 1.0m;   // 1 USD = 1.00 USD
    private readonly decimal _validMargin = 0.05m;       // 5% margin
    private readonly DateTime _validEffectiveFrom = DateTime.UtcNow.AddHours(1);
    private readonly string _validCreatedBy = "system";
    private readonly string _validSource = "Market";

    [Fact]
    public void CreateGeneralRate_WithValidData_ShouldCreateActiveGeneralRate()
    {
        // Act
        var rate = new ExchangeRateTestBuilder()
            .WithBaseCurrency(_xof)
            .WithTargetCurrency(_usd)
            .WithBaseCurrencyValue(_validBaseValue)
            .WithTargetCurrencyValue(_validTargetValue)
            .WithMargin(_validMargin)
            .WithEffectiveFrom(_validEffectiveFrom)
            .WithCreatedBy(_validCreatedBy)
            .WithSource(_validSource)
            .BuildGeneralRate();

        // Assert
        rate.Should().NotBeNull();
        rate.Id.Should().NotBe(Guid.Empty);
        rate.BaseCurrency.Should().Be(_xof);
        rate.TargetCurrency.Should().Be(_usd);
        rate.BaseCurrencyValue.Should().Be(_validBaseValue);
        rate.TargetCurrencyValue.Should().Be(_validTargetValue);
        rate.Margin.Should().Be(_validMargin);
        rate.EffectiveFrom.Should().Be(_validEffectiveFrom);
        rate.EffectiveTo.Should().BeNull();
        rate.IsActive.Should().BeTrue();
        rate.Type.Should().Be(RateType.General);
        rate.ClientId.Should().BeNull();
        rate.ClientGroupId.Should().BeNull();
        rate.Source.Should().Be(_validSource.Trim());
        rate.CreatedBy.Should().Be(_validCreatedBy.Trim());
        rate.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateGroupRate_WithValidData_ShouldCreateGroupRate()
    {
        // Arrange
        var clientGroupId = Guid.NewGuid();

        // Act
        var rate = new ExchangeRateTestBuilder()
            .WithBaseCurrency(_xof)
            .WithTargetCurrency(_usd)
            .WithBaseCurrencyValue(_validBaseValue)
            .WithTargetCurrencyValue(_validTargetValue)
            .WithMargin(_validMargin)
            .WithEffectiveFrom(_validEffectiveFrom)
            .WithClientGroupId(clientGroupId)
            .BuildGroupRate();

        // Assert
        rate.Should().NotBeNull();
        rate.Type.Should().Be(RateType.Group);
        rate.ClientGroupId.Should().Be(clientGroupId);
        rate.ClientId.Should().BeNull();
    }

    [Fact]
    public void CreateIndividualRate_WithValidData_ShouldCreateIndividualRate()
    {
        // Arrange
        var clientId = Guid.NewGuid();

        // Act
        var rate = new ExchangeRateTestBuilder()
            .WithBaseCurrency(_xof)
            .WithTargetCurrency(_usd)
            .WithBaseCurrencyValue(_validBaseValue)
            .WithTargetCurrencyValue(_validTargetValue)
            .WithMargin(_validMargin)
            .WithEffectiveFrom(_validEffectiveFrom)
            .WithClientId(clientId)
            .BuildIndividualRate();

        // Assert
        rate.Should().NotBeNull();
        rate.Type.Should().Be(RateType.Individual);
        rate.ClientId.Should().Be(clientId);
        rate.ClientGroupId.Should().BeNull();
        rate.Source.Should().Be("Manual");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidBaseCurrencyValue_ShouldThrowDomainException(decimal invalidValue)
    {
        // Act & Assert
        var action = () => new ExchangeRateTestBuilder()
            .WithBaseCurrencyValue(invalidValue)
            .BuildGeneralRate();

        action.Should().Throw<DomainException>().WithMessage("*Base currency value must be positive*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidTargetCurrencyValue_ShouldThrowDomainException(decimal invalidValue)
    {
        // Act & Assert
        var action = () => new ExchangeRateTestBuilder()
            .WithTargetCurrencyValue(invalidValue)
            .BuildGeneralRate();

        action.Should().Throw<DomainException>().WithMessage("*Target currency value must be positive*");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(2.0)]
    public void Create_WithInvalidMargin_ShouldThrowDomainException(decimal invalidMargin)
    {
        // Act & Assert
        var action = () => new ExchangeRateTestBuilder()
            .WithMargin(invalidMargin)
            .BuildGeneralRate();

        action.Should().Throw<DomainException>().WithMessage("*Margin must be between 0 and 1*");
    }

    [Fact]
    public void Create_WithEffectiveToBeforeEffectiveFrom_ShouldThrowDomainException()
    {
        // Arrange
        var effectiveFrom = DateTime.UtcNow.AddDays(1);
        var effectiveTo = DateTime.UtcNow;

        // Act & Assert
        var action = () => new ExchangeRateTestBuilder()
            .WithEffectiveFrom(effectiveFrom)
            .WithEffectiveTo(effectiveTo)
            .BuildGeneralRate();

        action.Should().Throw<DomainException>().WithMessage("*End date must be after start date*");
    }

    [Fact]
    public void Create_WithEffectiveToEqualToEffectiveFrom_ShouldThrowDomainException()
    {
        // Arrange
        var effectiveFrom = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        var action = () => new ExchangeRateTestBuilder()
            .WithEffectiveFrom(effectiveFrom)
            .WithEffectiveTo(effectiveFrom)
            .BuildGeneralRate();

        action.Should().Throw<DomainException>().WithMessage("*End date must be after start date*");
    }

    [Fact]
    public void CreateGroupRate_WithDefaultClientGroupId_ShouldThrowDomainException()
    {
        // Act & Assert
        var action = () => new ExchangeRateTestBuilder()
            .WithClientGroupId(Guid.Empty)
            .BuildGroupRate();

        action.Should().Throw<DomainException>().WithMessage("*clientGroupId*");
    }

    [Fact]
    public void CreateIndividualRate_WithDefaultClientId_ShouldThrowDomainException()
    {
        // Act & Assert
        var action = () => new ExchangeRateTestBuilder()
            .WithClientId(Guid.Empty)
            .BuildIndividualRate();

        action.Should().Throw<DomainException>().WithMessage("*clientId*");
    }

    [Fact]
    public void MarketRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var rate = new ExchangeRateTestBuilder()
            .WithBaseCurrencyValue(575.50m)  // 1 USD = 575.50 XOF
            .WithTargetCurrencyValue(1.0m)   // 1 USD = 1.00 USD
            .BuildGeneralRate();

        // Act & Assert
        // Market rate: XOF -> USD = 1/575.50 ≈ 0.001737
        rate.MarketRate.Should().BeApproximately(0.001737m, 0.000001m);
    }

    [Fact]
    public void EffectiveRate_ShouldIncludeMargin()
    {
        // Arrange
        var rate = new ExchangeRateTestBuilder()
            .WithBaseCurrencyValue(575.50m)
            .WithTargetCurrencyValue(1.0m)
            .WithMargin(0.05m) // 5% margin
            .BuildGeneralRate();

        var expectedMarketRate = 1.0m / 575.50m;
        var expectedEffectiveRate = expectedMarketRate * 1.05m;

        // Act & Assert
        rate.EffectiveRate.Should().BeApproximately(expectedEffectiveRate, 0.000001m);
    }

    [Fact]
    public void InverseMarketRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var rate = new ExchangeRateTestBuilder()
            .WithBaseCurrencyValue(575.50m)  // 1 USD = 575.50 XOF
            .WithTargetCurrencyValue(1.0m)   // 1 USD = 1.00 USD
            .BuildGeneralRate();

        // Act & Assert
        // Inverse market rate: USD -> XOF = 575.50
        rate.InverseMarketRate.Should().Be(575.50m);
    }

    [Fact]
    public void InverseEffectiveRate_ShouldIncludeMargin()
    {
        // Arrange
        var rate = new ExchangeRateTestBuilder()
            .WithBaseCurrencyValue(575.50m)
            .WithTargetCurrencyValue(1.0m)
            .WithMargin(0.05m) // 5% margin
            .BuildGeneralRate();

        var expectedInverseEffectiveRate = 575.50m * 1.05m;

        // Act & Assert
        rate.InverseEffectiveRate.Should().Be(expectedInverseEffectiveRate);
    }

    [Fact]
    public void ConvertToTarget_WithValidAmount_ShouldConvertCorrectly()
    {
        // Arrange
        var rate = new ExchangeRateTestBuilder()
            .WithBaseCurrencyValue(575.50m)
            .WithTargetCurrencyValue(1.0m)
            .WithMargin(0.05m)
            .BuildGeneralRate();

        var amountInBase = 1000m; // 1000 XOF
        var expectedAmount = 1000m * rate.EffectiveRate;

        // Act
        var result = rate.ConvertToTarget(amountInBase);

        // Assert
        result.Should().BeApproximately(expectedAmount, 0.0001m);
    }

    [Fact]
    public void ConvertToTarget_WithNegativeAmount_ShouldThrowDomainException()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();
        var negativeAmount = -1000m;

        // Act & Assert
        var action = () => rate.ConvertToTarget(negativeAmount);
        action.Should().Throw<DomainException>().WithMessage("*Amount to convert must be non-negative*");
    }

    [Fact]
    public void ConvertToBase_WithValidAmount_ShouldConvertCorrectly()
    {
        // Arrange
        var rate = new ExchangeRateTestBuilder()
            .WithBaseCurrencyValue(575.50m)
            .WithTargetCurrencyValue(1.0m)
            .WithMargin(0.05m)
            .BuildGeneralRate();

        var amountInTarget = 100m; // 100 USD
        var expectedAmount = 100m / rate.EffectiveRate;

        // Act
        var result = rate.ConvertToBase(amountInTarget);

        // Assert
        result.Should().BeApproximately(expectedAmount, 0.0001m);
    }

    [Fact]
    public void ConvertUsingInverseRate_WithValidAmount_ShouldConvertCorrectly()
    {
        // Arrange
        var rate = new ExchangeRateTestBuilder()
            .WithBaseCurrencyValue(575.50m)
            .WithTargetCurrencyValue(1.0m)
            .WithMargin(0.05m)
            .BuildGeneralRate();

        var amountInTarget = 100m; // 100 USD
        var expectedAmount = 100m * rate.InverseEffectiveRate;

        // Act
        var result = rate.ConvertUsingInverseRate(amountInTarget);

        // Assert
        result.Should().Be(expectedAmount);
    }

    [Fact]
    public void GetConversionRate_WithValidCurrencyPair_ShouldReturnCorrectRate()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();

        // Act
        var baseToTargetRate = rate.GetConversionRate(_xof, _usd);
        var targetToBaseRate = rate.GetConversionRate(_usd, _xof);

        // Assert
        baseToTargetRate.Should().Be(rate.EffectiveRate);
        targetToBaseRate.Should().Be(rate.InverseEffectiveRate);
    }

    [Fact]
    public void GetConversionRate_WithInvalidCurrencyPair_ShouldThrowDomainException()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();
        var ngn = Currency.NGN;

        // Act & Assert
        var action1 = () => rate.GetConversionRate(_xof, ngn);
        var action2 = () => rate.GetConversionRate(ngn, _usd);

        action1.Should().Throw<DomainException>();
        action2.Should().Throw<DomainException>();
    }

    [Fact]
    public void ConvertAmount_WithValidParameters_ShouldConvertCorrectly()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();
        var amount = 1000m;

        // Act
        var baseToTarget = rate.ConvertAmount(amount, _xof, _usd);
        var targetToBase = rate.ConvertAmount(amount, _usd, _xof);

        // Assert
        baseToTarget.Should().Be(rate.ConvertToTarget(amount));
        targetToBase.Should().Be(rate.ConvertToBase(amount));
    }

    [Fact]
    public void UpdateCurrencyValues_WithValidValues_ShouldUpdateProperties()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();
        var newBaseValue = 600.00m;
        var newTargetValue = 1.0m;
        var newMargin = 0.03m;

        // Act
        rate.UpdateCurrencyValues(newBaseValue, newTargetValue, newMargin);

        // Assert
        rate.BaseCurrencyValue.Should().Be(newBaseValue);
        rate.TargetCurrencyValue.Should().Be(newTargetValue);
        rate.Margin.Should().Be(newMargin);
    }

    [Fact]
    public void UpdateCurrencyValues_WithInvalidValues_ShouldThrowDomainException()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();

        // Act & Assert
        var action1 = () => rate.UpdateCurrencyValues(0, 1.0m, 0.05m);
        var action2 = () => rate.UpdateCurrencyValues(575.50m, -1.0m, 0.05m);
        var action3 = () => rate.UpdateCurrencyValues(575.50m, 1.0m, 1.5m);

        action1.Should().Throw<DomainException>();
        action2.Should().Throw<DomainException>();
        action3.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deactivate_ActiveRate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();

        // Act
        rate.Deactivate();

        // Assert
        rate.IsActive.Should().BeFalse();
        rate.EffectiveTo.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_AlreadyInactiveRate_ShouldDoNothing()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateInactiveGeneralRate();
        var originalEffectiveTo = rate.EffectiveTo;

        // Act
        rate.Deactivate();

        // Assert
        rate.IsActive.Should().BeFalse();
        rate.EffectiveTo.Should().Be(originalEffectiveTo);
    }

    [Fact]
    public void ExtendValidity_WithValidDate_ShouldUpdateEffectiveTo()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();
        var newEffectiveTo = DateTime.UtcNow.AddDays(30);

        // Act
        rate.ExtendValidity(newEffectiveTo);

        // Assert
        rate.EffectiveTo.Should().Be(newEffectiveTo);
    }

    [Fact]
    public void ExtendValidity_WithInvalidDate_ShouldThrowDomainException()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();
        var invalidEffectiveTo = rate.EffectiveFrom.AddDays(-1);

        // Act & Assert
        var action = () => rate.ExtendValidity(invalidEffectiveTo);
        action.Should().Throw<DomainException>().WithMessage("*New effective date must be after start date*");
    }

    [Fact]
    public void IsEffectiveAt_WithCurrentDate_ShouldReturnTrueForActiveRate()
    {
        // Arrange
        var rate = new ExchangeRateTestBuilder()
            .WithEffectiveFrom(DateTime.UtcNow.AddHours(-1))
            .BuildGeneralRate();

        // Act & Assert
        rate.IsEffectiveAt(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsEffectiveAt_WithFutureDate_ShouldReturnFalse()
    {
        // Arrange
        var rate = new ExchangeRateTestBuilder()
            .WithEffectiveFrom(DateTime.UtcNow.AddHours(1))
            .BuildGeneralRate();

        // Act & Assert
        rate.IsEffectiveAt(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsEffectiveAt_WithPastDateAfterEffectiveTo_ShouldReturnFalse()
    {
        // Arrange
        var rate = new ExchangeRateTestBuilder()
            .WithEffectiveFrom(DateTime.UtcNow.AddDays(-10))
            .WithEffectiveTo(DateTime.UtcNow.AddDays(-5))
            .BuildGeneralRate();

        // Act & Assert
        rate.IsEffectiveAt(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void IsEffectiveAt_WithInactiveRate_ShouldReturnFalse()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateInactiveGeneralRate();

        // Act & Assert
        rate.IsEffectiveAt(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void Expire_WithFutureNewRate_ShouldSetEffectiveTo()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();
        var newRateEffectiveFrom = DateTime.UtcNow.AddDays(1);

        // Act
        rate.Expire(newRateEffectiveFrom, "system", "New rate available");

        // Assert
        rate.EffectiveTo.Should().Be(newRateEffectiveFrom.AddSeconds(-1));
        rate.IsActive.Should().BeTrue(); // Still active until new rate starts
    }

    [Fact]
    public void Expire_WithPastNewRate_ShouldDeactivate()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();
        var newRateEffectiveFrom = DateTime.UtcNow.AddHours(-1);

        // Act
        rate.Expire(newRateEffectiveFrom, "system", "New rate available");

        // Assert
        rate.IsActive.Should().BeFalse();
    }

    [Fact]
    public void MarkAsHistorical_ShouldDeactivateRate()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();

        // Act
        rate.MarkAsHistorical();

        // Assert
        rate.IsActive.Should().BeFalse();
    }

    [Fact]
    public void GetRateDescription_ShouldReturnFormattedString()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();

        // Act
        var description = rate.GetRateDescription();

        // Assert
        description.Should().Contain($"1 {_xof.Code} = ");
        description.Should().Contain(_usd.Code);
        description.Should().Contain("Market:");
        description.Should().Contain("Margin:");
    }

    [Fact]
    public void GetInverseRateDescription_ShouldReturnFormattedString()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();

        // Act
        var description = rate.GetInverseRateDescription();

        // Assert
        description.Should().Contain($"1 {_usd.Code} = ");
        description.Should().Contain(_xof.Code);
        description.Should().Contain("Market:");
        description.Should().Contain("Margin:");
    }

    [Fact]
    public void GetDirectionalRateDescription_WithValidDirection_ShouldReturnCorrectDescription()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();

        // Act
        var baseToTarget = rate.GetDirectionalRateDescription(_xof, _usd);
        var targetToBase = rate.GetDirectionalRateDescription(_usd, _xof);

        // Assert
        baseToTarget.Should().Be(rate.GetRateDescription());
        targetToBase.Should().Be(rate.GetInverseRateDescription());
    }

    [Fact]
    public void GetDirectionalRateDescription_WithInvalidDirection_ShouldThrowDomainException()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();
        var ngn = Currency.NGN;

        // Act & Assert
        var action = () => rate.GetDirectionalRateDescription(_xof, ngn);
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void CanConvert_WithValidParameters_ShouldReturnTrue()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();

        // Act & Assert
        rate.CanConvert(1000m, _xof, _usd).Should().BeTrue();
        rate.CanConvert(100m, _usd, _xof).Should().BeTrue();
    }

    [Fact]
    public void CanConvert_WithInactiveRate_ShouldReturnFalse()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateInactiveGeneralRate();

        // Act & Assert
        rate.CanConvert(1000m, _xof, _usd).Should().BeFalse();
    }

    [Fact]
    public void CanConvert_WithNegativeAmount_ShouldReturnFalse()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();

        // Act & Assert
        rate.CanConvert(-1000m, _xof, _usd).Should().BeFalse();
    }

    [Fact]
    public void CanConvert_WithInvalidCurrencyPair_ShouldReturnFalse()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();
        var ngn = Currency.NGN;

        // Act & Assert
        rate.CanConvert(1000m, _xof, ngn).Should().BeFalse();
        rate.CanConvert(1000m, ngn, _usd).Should().BeFalse();
    }

    [Fact]
    public void CalculateMarginAmount_WithValidConversion_ShouldCalculateCorrectly()
    {
        // Arrange
        var rate = new ExchangeRateTestBuilder()
            .WithBaseCurrencyValue(575.50m)
            .WithTargetCurrencyValue(1.0m)
            .WithMargin(0.05m)
            .BuildGeneralRate();

        var amount = 1000m; // 1000 XOF

        // Act
        var marginAmount = rate.CalculateMarginAmount(amount, _xof, _usd);

        // Assert
        var marketAmount = amount * rate.MarketRate;
        var effectiveAmount = amount * rate.EffectiveRate;
        var expectedMargin = effectiveAmount - marketAmount;

        marginAmount.Should().BeApproximately(expectedMargin, 0.0001m);
    }

    [Fact]
    public void GetRateSummary_ShouldIncludeRateTypeAndClientInfo()
    {
        // Arrange
        var rate = ExchangeRateTestBuilder.CreateActiveGeneralRate();

        // Act
        var summary = rate.GetRateSummary();

        // Assert
        summary.Should().Contain("General Rate");
        summary.Should().Contain(rate.GetDualRateDescription());
    }
}