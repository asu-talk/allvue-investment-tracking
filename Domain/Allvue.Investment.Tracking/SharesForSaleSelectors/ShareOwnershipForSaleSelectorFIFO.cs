using Allvue.Investment.Tracking.Exceptions;

namespace Allvue.Investment.Tracking.SharesForSaleSelectors;

internal class ShareOwnershipForSaleSelectorFIFO : IShareOwnershipForSaleSelector
{
    public ShareOwnership SelectFrom(IReadOnlyCollection<ShareOwnership> shareOwnerships)
    {
        if (shareOwnerships == null) throw new ArgumentNullException(nameof(shareOwnerships));

        if (!shareOwnerships.Any())
        {
            throw new NoAvailableShareOwnershipForSaleException("Share ownership collection is empty.");
        }

        var shareOwnership = shareOwnerships.FirstOrDefault(
            shareOwnership => shareOwnership.AvailableSharesCount > 0
        );

        return shareOwnership ?? throw new AllShareOwnershipsAreEmptyException();
    }
}
