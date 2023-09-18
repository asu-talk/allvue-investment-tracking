namespace Allvue.Investment.Tracking.Exceptions;

public class AllShareOwnershipsAreEmptyException : NoAvailableShareOwnershipForSaleException
{
    public AllShareOwnershipsAreEmptyException()
        : base("All available share ownerships are empty") { }
}
