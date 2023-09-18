using Allvue.Investment.Tracking.Exceptions;
using FluentAssertions;

namespace Allvue.Investment.Tracking.Tests;

public class InvestmentSellingCalculatorIntegrationTests
{
    private InvestmentSellingCalculator _sut;

    [OneTimeSetUp]
    public void Setup()
    {
        var ownershipForSaleSelectorFactory = new ShareOwnershipForSaleSelectorFactory();
        _sut = new InvestmentSellingCalculator(ownershipForSaleSelectorFactory);
    }

    [Test]
    public void Calculate_CaseDescribedInRequirements_Success()
    {
        // Arrange
        var purchaseLots = new List<Lot>
                           {
                               new(count: 100, priceUSD: 20, Month.January),
                               new(count: 200, priceUSD: 30, Month.March)
                           };

        // Act
        var actualResult = _sut.Calculate(
            purchaseLots,
            Month.April,
            LotSellingStrategy.FIFO,
            numberOfSharesToSell: 150,
            shareSellPriceUSD: 40
        );

        // Assert
        actualResult.Should().NotBeNull();
        actualResult.RemainingNumberOfShares.Should().Be(100 + 200 - 150);
        actualResult.CostBasisOfSoldSharesUSD.Should().Be((100 * 20m + 50 * 30m) / 150m);
        actualResult.CostBasisOfRemainingSharesUSD.Should().Be(30);
        actualResult.ProfitUSD.Should().Be(100 * (40m - 20m) + 50 * (40m - 30m));
    }

    [Test]
    public void Calculate_PurchaseLotsIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () =>
            {
                _sut.Calculate(
                    null!,
                    Month.April,
                    LotSellingStrategy.FIFO,
                    numberOfSharesToSell: 150,
                    shareSellPriceUSD: 40
                );
            }
        );
    }

    [Test]
    public void Calculate_NumberOfSharesToSellIsZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var purchaseLots = new List<Lot>
                           {
                               new(count: 100, priceUSD: 20, Month.January),
                               new(count: 200, priceUSD: 30, Month.March)
                           };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
            {
                _sut.Calculate(
                    purchaseLots,
                    Month.April,
                    LotSellingStrategy.FIFO,
                    numberOfSharesToSell: 0,
                    shareSellPriceUSD: 40m
                );
            }
        );
    }

    [Test]
    public void Calculate_ShareSellPriceUsdLessOrEqualZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var purchaseLots = new List<Lot>
                           {
                               new(count: 100, priceUSD: 20, Month.January),
                               new(count: 200, priceUSD: 30, Month.March)
                           };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
            {
                _sut.Calculate(
                    purchaseLots,
                    Month.April,
                    LotSellingStrategy.FIFO,
                    numberOfSharesToSell: 40,
                    shareSellPriceUSD: 0m
                );
            }
        );

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
            {
                _sut.Calculate(
                    purchaseLots,
                    Month.April,
                    LotSellingStrategy.FIFO,
                    numberOfSharesToSell: 40,
                    shareSellPriceUSD: -0.01m
                );
            }
        );
    }

    [Test]
    public void Calculate_SellAllShares_Success()
    {
        // Arrange
        var purchaseLots = new List<Lot>
                           {
                               new(count: 100, priceUSD: 20, Month.January),
                               new(count: 200, priceUSD: 30, Month.March)
                           };

        // Act
        var actualResult = _sut.Calculate(
            purchaseLots,
            Month.April,
            LotSellingStrategy.FIFO,
            numberOfSharesToSell: 300,
            shareSellPriceUSD: 40
        );

        actualResult.Should().NotBeNull();
        actualResult.RemainingNumberOfShares.Should().Be(0);
        actualResult.CostBasisOfSoldSharesUSD.Should().Be((100 * 20m + 200 * 30m) / 300m);
        actualResult.CostBasisOfRemainingSharesUSD.Should().BeNull();
        actualResult.ProfitUSD.Should().Be(100 * (40m - 20m) + 200 * (40m - 30m));
    }

    [Test]
    public void Calculate_ThereIsNoPurchaseLots_ThrowsNotEnoughSharesToSellException()
    {
        // Arrange
        var purchaseLots = Array.Empty<Lot>();

        // Act & Assert
        Assert.Throws<NotEnoughSharesToSellException>(
            () =>
            {
                _sut.Calculate(
                    purchaseLots,
                    Month.April,
                    LotSellingStrategy.FIFO,
                    numberOfSharesToSell: 150,
                    shareSellPriceUSD: 40
                );
            }
        );
    }

    [Test]
    public void Calculate_AttemptToSellMoreSharesThenAvailable_ThrowsNotEnoughSharesToSellException()
    {
        // Arrange
        var purchaseLots = new List<Lot>
                           {
                               new(count: 100, priceUSD: 20, Month.January),
                               new(count: 200, priceUSD: 30, Month.March)
                           };

        // Act & Assert
        Assert.Throws<NotEnoughSharesToSellException>(
            () =>
            {
                _sut.Calculate(
                    purchaseLots,
                    Month.April,
                    LotSellingStrategy.FIFO,
                    numberOfSharesToSell: 301,
                    shareSellPriceUSD: 40
                );
            }
        );
    }

    [Test]
    public void Calculate_NotSupportedLotSellingStrategy_ThrowsLotSellingStrategyIsNotSupportedYet()
    {
        // Arrange
        LotSellingStrategy[] supportedStrategies = { LotSellingStrategy.FIFO };
        var allAvailableStrategies = Enum.GetValues<LotSellingStrategy>();
        var unsupportedStrategies = allAvailableStrategies.Except(supportedStrategies);
        var purchaseLots = new List<Lot>
                           {
                               new(count: 100, priceUSD: 20, Month.January),
                               new(count: 200, priceUSD: 30, Month.March)
                           };

        foreach (var unsupportedStrategy in unsupportedStrategies)
        {
            // Act & Assert
            Assert.Throws<LotSellingStrategyIsNotSupportedYetException>(
                () =>
                {
                    _sut.Calculate(
                        purchaseLots,
                        Month.April,
                        unsupportedStrategy,
                        numberOfSharesToSell: 301,
                        shareSellPriceUSD: 40
                    );
                }
            );
        }
    }
}
