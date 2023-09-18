using Allvue.Investment.Tracking.Exceptions;
using Allvue.Investment.Tracking.SharesForSaleSelectors;
using FluentAssertions;

namespace Allvue.Investment.Tracking.Tests.SharesForSaleSelectors;

public class ShareOwnershipForSaleSelectorFIFOTests
{
    private readonly ShareOwnershipForSaleSelectorFIFO _sut;

    public ShareOwnershipForSaleSelectorFIFOTests()
    {
        _sut = new ShareOwnershipForSaleSelectorFIFO();
    }

    [Test]
    public void SelectFrom_ShareOwnershipCollectionIsNull_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _sut.SelectFrom(shareOwnerships: null!)
        );
    }

    [Test]
    public void SelectFrom_ShareOwnershipCollectionIsEmpty_ThrowsNoAvailableShareOwnershipForSaleException()
    {
        // Act & Assert
        Assert.Throws<NoAvailableShareOwnershipForSaleException>(
            () => _sut.SelectFrom(shareOwnerships: Array.Empty<ShareOwnership>())
        );
    }

    [Test]
    public void SelectFrom_FirstShareOwnershipHasAvailableSharesToSell_ReturnsFirstSharesOwnership()
    {
        // Arrange
        var shareOwnerships = new[]
                              {
                                  CreateShareOwnership(),
                                  CreateShareOwnership(),
                                  CreateShareOwnership()
                              };

        // Act
        var actual = _sut.SelectFrom(shareOwnerships);

        // Assert
        actual.Should().Be(shareOwnerships.First());
    }

    [Test]
    public void SelectFrom_FirstShareOwnershipDoesNotHaveAvailableSharesToSellButOtherHave_ReturnsSecondSharesOwnership()
    {
        // Arrange
        var shareOwnerships = new[]
                              {
                                  CreateShareOwnershipWithoutAvailableShares(),
                                  CreateShareOwnership(),
                                  CreateShareOwnership()
                              };

        // Act
        var actual = _sut.SelectFrom(shareOwnerships);

        // Assert
        actual.Should().Be(shareOwnerships[1]);
    }

    [Test]
    public void SelectFrom_OnlyLastShareOwnershipHasAvailableSharesToSell_ReturnsLastSharesOwnership()
    {
        // Arrange
        var shareOwnerships = new[]
                              {
                                  CreateShareOwnershipWithoutAvailableShares(),
                                  CreateShareOwnershipWithoutAvailableShares(),
                                  CreateShareOwnership()
                              };

        // Act
        var actual = _sut.SelectFrom(shareOwnerships);

        // Assert
        actual.Should().Be(shareOwnerships.Last());
    }

    [Test]
    public void SelectFrom_AllShareOwnershipAreEmpty_ThrowsAllShareOwnershipsAreEmptyException()
    {
        // Arrange
        var shareOwnerships = new[]
                              {
                                  CreateShareOwnershipWithoutAvailableShares(),
                                  CreateShareOwnershipWithoutAvailableShares(),
                                  CreateShareOwnershipWithoutAvailableShares()
                              };

        // Act & Assert
        Assert.Throws<AllShareOwnershipsAreEmptyException>(
            () => _sut.SelectFrom(shareOwnerships)
        );
    }

    private static ShareOwnership CreateShareOwnership()
    {
        var purchaseLot = new Lot(count: 42, priceUSD: 20m, Month.January);
        var result = new ShareOwnership(purchaseLot);

        result.AvailableSharesCount.Should().BeGreaterThan(0);
        return result;
    }

    private static ShareOwnership CreateShareOwnershipWithoutAvailableShares()
    {
        var purchaseLot = new Lot(count: 42, priceUSD: 20m, Month.January);
        var result = new ShareOwnership(purchaseLot);

        var saleLot = new Lot(purchaseLot.Count, priceUSD: 20m, Month.April);
        result.Sell(saleLot);

        result.AvailableSharesCount.Should().Be(0);
        return result;
    }
}
