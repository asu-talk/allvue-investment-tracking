namespace Allvue.Investment.Tracking.Exceptions;

public class NotEnoughSharesToSellException : InvestmentSellingCalculationException
{
    public NotEnoughSharesToSellException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
