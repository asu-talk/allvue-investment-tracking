using Allvue.Investment.Tracking.Exceptions;

namespace Allvue.Investment.Tracking;

public interface IShareOwnershipForSaleSelector
{
    /// <exception cref="ArgumentNullException">
    ///     If <see cref="shareOwnerships" /> is null
    /// </exception>
    /// <exception cref="NoAvailableShareOwnershipForSaleException">
    ///     If <see cref="shareOwnerships" /> collection is empty.
    /// </exception>
    /// <exception cref="AllShareOwnershipsAreEmptyException">
    ///     If all <see cref="shareOwnerships" /> entries are empty.
    /// </exception>
    ShareOwnership SelectFrom(IReadOnlyCollection<ShareOwnership> shareOwnerships);
}
