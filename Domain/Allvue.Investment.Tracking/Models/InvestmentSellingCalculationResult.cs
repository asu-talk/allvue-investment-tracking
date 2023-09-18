namespace Allvue.Investment.Tracking;

public class InvestmentSellingCalculationResult
{
    public InvestmentSellingCalculationResult(uint remainingNumberOfShares,
                                              decimal? costBasisOfSoldSharesUSD,
                                              decimal? costBasisOfRemainingSharesUSD,
                                              decimal profitUSD)
    {
        RemainingNumberOfShares = remainingNumberOfShares;
        CostBasisOfSoldSharesUSD = costBasisOfSoldSharesUSD;
        CostBasisOfRemainingSharesUSD = costBasisOfRemainingSharesUSD;
        ProfitUSD = profitUSD;
    }

    public uint RemainingNumberOfShares { get; }

    /// <summary>
    /// Null in case when there where no share sales
    /// </summary>
    public decimal? CostBasisOfSoldSharesUSD { get; }

    /// <summary>
    /// Null in case when there is no remaining shares
    /// </summary>
    public decimal? CostBasisOfRemainingSharesUSD { get; }

    public decimal ProfitUSD { get; }
}
