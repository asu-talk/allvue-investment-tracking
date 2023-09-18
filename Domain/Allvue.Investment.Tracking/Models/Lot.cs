namespace Allvue.Investment.Tracking;

public class Lot
{
    public Lot(uint count, decimal priceUSD, Month tradeMonth)
    {
        if (count == 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (priceUSD <= 0) throw new ArgumentOutOfRangeException(nameof(priceUSD));

        Count = count;
        PriceUSD = priceUSD;
        TradeMonth = tradeMonth;
    }

    // Note: Use integer for simplicity. In real application probably dedicated data type will be used to properly handle future changes like "sell fractions of share".
    public uint Count { get; }

    // Note: Use decimal data type here for simplicity.
    // In real application probably dedicated Money type will be used to cover cases when stock prices are in different currents
    public decimal PriceUSD { get; }

    public Month TradeMonth { get; }
}
