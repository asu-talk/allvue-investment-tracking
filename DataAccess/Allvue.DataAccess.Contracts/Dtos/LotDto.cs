namespace Allvue.DataAccess.Contracts.Dtos;

public class LotDto
{
    public uint Count { get; set; }

    public decimal PriceUSD { get; set; }

    public MonthDto TradeMonth { get; set; }
}
