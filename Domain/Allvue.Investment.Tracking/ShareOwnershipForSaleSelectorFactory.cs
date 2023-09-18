using Allvue.Investment.Tracking.Exceptions;
using Allvue.Investment.Tracking.SharesForSaleSelectors;

namespace Allvue.Investment.Tracking;

public class ShareOwnershipForSaleSelectorFactory
{
    public IShareOwnershipForSaleSelector Create(LotSellingStrategy sellingStrategy)
    {
        switch (sellingStrategy)
        {
            case LotSellingStrategy.FIFO:
                return new ShareOwnershipForSaleSelectorFIFO();
            case LotSellingStrategy.LIFO:
            case LotSellingStrategy.AverageCost:
            case LotSellingStrategy.LowestTaxExposure:
            case LotSellingStrategy.HighestTaxExposure:
            case LotSellingStrategy.LotBased:
                throw new LotSellingStrategyIsNotSupportedYetException(sellingStrategy);
            default:
                throw new ArgumentOutOfRangeException(nameof(sellingStrategy), sellingStrategy, message: null);
        }
    }
}
