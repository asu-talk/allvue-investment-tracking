namespace Allvue.Investment.Tracking.Exceptions;

public class LotSellingStrategyIsNotSupportedYetException : NotSupportedException
{
    public LotSellingStrategyIsNotSupportedYetException(LotSellingStrategy sellingStrategy)
        : base($"{sellingStrategy} is not supported yet")
    {
        SellingStrategy = sellingStrategy;
    }

    public LotSellingStrategy SellingStrategy { get; }
}
