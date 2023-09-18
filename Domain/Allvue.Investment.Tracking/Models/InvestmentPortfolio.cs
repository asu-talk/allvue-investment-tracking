namespace Allvue.Investment.Tracking;

public class InvestmentPortfolio
{
    private readonly List<ShareOwnership> _shareOwnerships = new();

    public IReadOnlyCollection<ShareOwnership> ShareOwnerships => _shareOwnerships;

    public void Purchase(Lot purchaseLot)
    {
        var shareOwnership = new ShareOwnership(purchaseLot);
        _shareOwnerships.Add(shareOwnership);
    }
}
