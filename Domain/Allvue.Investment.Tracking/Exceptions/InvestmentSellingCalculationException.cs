namespace Allvue.Investment.Tracking.Exceptions;

public abstract class InvestmentSellingCalculationException : ApplicationException
{
    protected InvestmentSellingCalculationException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
