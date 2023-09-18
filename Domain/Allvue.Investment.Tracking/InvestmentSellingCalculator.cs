using Allvue.Investment.Tracking.Exceptions;

namespace Allvue.Investment.Tracking;

// TODO: this class doing to much - refactor to have
//       - class that is responsible for performing sell operation against Portfolio
//       - classes that are responsible for different calculations like:
//            - CalculateRemainingNumberOfShares
//            - CalculateCostBasisOfSoldSharesUSD
//            - CalculateCostBasisOfRemainingSharesUSD
public class InvestmentSellingCalculator
{
    private readonly ShareOwnershipForSaleSelectorFactory _shareOwnershipForSaleSelectorFactory;

    public InvestmentSellingCalculator(ShareOwnershipForSaleSelectorFactory shareOwnershipForSaleSelectorFactory)
    {
        _shareOwnershipForSaleSelectorFactory = shareOwnershipForSaleSelectorFactory;
    }

    /// <exception cref="LotSellingStrategyIsNotSupportedYetException"></exception>
    /// <exception cref="NotEnoughSharesToSellException"></exception>
    /// <exception cref="ArgumentNullException">If <see cref="purchaseLots" /> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">If <see cref="numberOfSharesToSell" /> is less or equal to zero</exception>
    /// <exception cref="ArgumentOutOfRangeException"> If <see cref="shareSellPriceUSD" /> is less or equal to zero </exception>
    public InvestmentSellingCalculationResult Calculate(IReadOnlyCollection<Lot> purchaseLots,
                                                        Month tradeMonth,
                                                        LotSellingStrategy sellingStrategy,
                                                        uint numberOfSharesToSell,
                                                        decimal shareSellPriceUSD)
    {
        if (purchaseLots == null) throw new ArgumentNullException(nameof(purchaseLots));
        if (numberOfSharesToSell <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfSharesToSell));
        if (shareSellPriceUSD <= 0) throw new ArgumentOutOfRangeException(nameof(shareSellPriceUSD));

        var portfolio = CreateInvestmentPortfolio(purchaseLots);

        PerformSellOperation(portfolio, tradeMonth, sellingStrategy, numberOfSharesToSell, shareSellPriceUSD);
        var calculationResult = CalculateSellingResult(portfolio);

        return calculationResult;
    }

    private void PerformSellOperation(InvestmentPortfolio portfolio,
                                      Month tradeMonth,
                                      LotSellingStrategy sellingStrategy,
                                      uint numberOfSharesToSell,
                                      decimal shareSellPriceUSD)
    {
        var ownershipForSaleSelector = _shareOwnershipForSaleSelectorFactory.Create(sellingStrategy);
        try
        {
            var leftToSell = numberOfSharesToSell;
            while (leftToSell > 0)
            {
                var shareOwnershipForSale = ownershipForSaleSelector.SelectFrom(portfolio.ShareOwnerships);
                var availableSharesCount = shareOwnershipForSale.AvailableSharesCount;
                var sellCount = availableSharesCount < leftToSell
                                    ? availableSharesCount
                                    : leftToSell;

                var saleLot = new Lot(sellCount, shareSellPriceUSD, tradeMonth);
                shareOwnershipForSale.Sell(saleLot);
                leftToSell -= saleLot.Count;
            }
        }
        catch (AllShareOwnershipsAreEmptyException ex)
        {
            throw new NotEnoughSharesToSellException("Not enough shares to perform sale operation.", ex);
        }
        catch (NoAvailableShareOwnershipForSaleException ex)
        {
            throw new NotEnoughSharesToSellException("No available shares for sale.", ex);
        }
    }

    private static InvestmentSellingCalculationResult CalculateSellingResult(InvestmentPortfolio portfolio)
    {
        var remainingNumberOfShares = CalculateRemainingNumberOfShares(portfolio);
        var costBasisOfSoldSharesUSD = CalculateCostBasisOfSoldSharesUSD(portfolio);
        var costBasisOfRemainingSharesUSD = CalculateCostBasisOfRemainingSharesUSD(portfolio);
        var profitUSD = CalculateProfitUSD(portfolio);

        return new InvestmentSellingCalculationResult(
            remainingNumberOfShares,
            costBasisOfSoldSharesUSD,
            costBasisOfRemainingSharesUSD,
            profitUSD
        );
    }

    private static InvestmentPortfolio CreateInvestmentPortfolio(IEnumerable<Lot> purchaseLots)
    {
        var portfolio = new InvestmentPortfolio();
        foreach (var purchaseLot in purchaseLots)
        {
            portfolio.Purchase(purchaseLot);
        }

        return portfolio;
    }

    private static uint CalculateRemainingNumberOfShares(InvestmentPortfolio portfolio)
    {
        uint result = 0;
        foreach (var shareOwnership in portfolio.ShareOwnerships)
        {
            var availableSharesCount = shareOwnership.AvailableSharesCount;
            result += availableSharesCount;
        }

        return result;
    }

    private static decimal? CalculateCostBasisOfSoldSharesUSD(InvestmentPortfolio portfolio)
    {
        uint totalSoldSharesCount = 0;
        decimal totalReceivedMoneyUSD = 0;
        foreach (var shareOwnership in portfolio.ShareOwnerships)
        {
            var purchasePriceUSD = shareOwnership.PurchaseLot.PriceUSD;

            foreach (var saleLot in shareOwnership.SaleLots)
            {
                totalReceivedMoneyUSD += saleLot.Count * purchasePriceUSD;
                totalSoldSharesCount += saleLot.Count;
            }
        }

        if (totalSoldSharesCount == 0) return null;
        return totalReceivedMoneyUSD / totalSoldSharesCount;
    }

    private static decimal? CalculateCostBasisOfRemainingSharesUSD(InvestmentPortfolio portfolio)
    {
        uint totalAvailableSharesCount = 0;
        decimal totalPriceUSD = 0;
        foreach (var shareOwnership in portfolio.ShareOwnerships)
        {
            var purchasePriceUSD = shareOwnership.PurchaseLot.PriceUSD;
            var availableSharesCount = shareOwnership.AvailableSharesCount;

            totalAvailableSharesCount += availableSharesCount;
            totalPriceUSD += availableSharesCount * purchasePriceUSD;
        }

        if (totalAvailableSharesCount == 0) return null;
        return totalPriceUSD / totalAvailableSharesCount;
    }

    private static decimal CalculateProfitUSD(InvestmentPortfolio portfolio)
    {
        decimal profit = 0;
        foreach (var shareOwnership in portfolio.ShareOwnerships)
        {
            var purchasePriceUSD = shareOwnership.PurchaseLot.PriceUSD;

            foreach (var saleLot in shareOwnership.SaleLots)
            {
                var salePriceUSD = saleLot.PriceUSD;
                var priceMarginUSD = salePriceUSD - purchasePriceUSD;
                profit += saleLot.Count * priceMarginUSD;
            }
        }

        return profit;
    }
}
