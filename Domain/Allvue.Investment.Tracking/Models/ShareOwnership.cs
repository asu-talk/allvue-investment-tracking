namespace Allvue.Investment.Tracking;

public class ShareOwnership
{
    private readonly List<Lot> _saleLots = new();

    public ShareOwnership(Lot purchaseLot)
    {
        PurchaseLot = purchaseLot;
        AvailableSharesCount += purchaseLot.Count;
    }

    public Lot PurchaseLot { get; }

    public IEnumerable<Lot> SaleLots => _saleLots;

    public uint AvailableSharesCount { get; private set; }

    public void Sell(Lot saleLot)
    {
        _saleLots.Add(saleLot);
        AvailableSharesCount -= saleLot.Count;
    }
}
