namespace Allvue.Investment.Tracking.Exceptions;

public class NoAvailableShareOwnershipForSaleException : ApplicationException
{
    public NoAvailableShareOwnershipForSaleException(string message)
        : base(message) { }

    public NoAvailableShareOwnershipForSaleException(string message, Exception inner)
        : base(message, inner) { }
}
