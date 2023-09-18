using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allvue.DataAccess.Contracts.Dtos;
using Allvue.DataAccess.InMemory;
using Allvue.Investment.Tracking;
using Allvue.Investment.Tracking.Exceptions;

namespace Allvue.ProfitCalculator;

// TODO: this class is a mess - UI + basic validations + business logic are mixed
//       As a result business use case flow (select parameter, perform calculation, present results)
//       need to create "user interaction abstraction" and here only have Console related implementation
//       + move application logic to Domain
internal class Program
{
    // Current trade month and purchase lots are hardcoded
    private const Month CurrentTradeMonth = Month.April;
    private readonly InvestmentSellingCalculator _investmentSellingCalculator;

    private readonly LotDto[] _purchaseLots =
    {
        new() { Count = 100, PriceUSD = 20.0m, TradeMonth = MonthDto.January },
        new() { Count = 150, PriceUSD = 30.0m, TradeMonth = MonthDto.February },
        new() { Count = 120, PriceUSD = 10.0m, TradeMonth = MonthDto.March }
    };

    private readonly InMemoryCustomerPurchaseLotsRepository _purchaseLotsRepository;

    public Program()
    {
        // It is a composition root - usually it is DI container responsibility to build all required services
        _purchaseLotsRepository = new InMemoryCustomerPurchaseLotsRepository();
        _investmentSellingCalculator = new InvestmentSellingCalculator(
            new ShareOwnershipForSaleSelectorFactory()
        );
    }

    public static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException
            += (_, ex) =>
               {
                   DisplayDebuggingError((Exception)ex.ExceptionObject);
                   Environment.FailFast("Unexpected error occurred. Please contact support team.");
               };

        new Program().RunAsync(CancellationToken.None).Wait();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await InitializeData(cancellationToken);

        while (true)
        {
            var purchaseLotsDtos = await _purchaseLotsRepository.ReadOrderedPurchaseLotsAsync(CancellationToken.None);
            var purchaseLots = Map(purchaseLotsDtos);

            DisplayPurchaseLots(purchaseLots);
            Console.WriteLine();

            // TODO: It is not user friendly to ask all parameters and do validations (like not enough shares) only
            // during calculation attempt. It is better to have dedicated validators and use them right after a user provided
            // a parameter value
            var numberSharesToSell = AskNumberSharesToSell();
            var shareSellPriceUSD = AskShareSellPriceUSD();
            var lotSellingStrategy = AskLotSellingStrategy();

            try
            {
                var calculationResult = _investmentSellingCalculator.Calculate(
                    purchaseLots,
                    CurrentTradeMonth,
                    lotSellingStrategy,
                    numberSharesToSell,
                    shareSellPriceUSD
                );

                DisplayCalculationResult(calculationResult);
            }
            catch (LotSellingStrategyIsNotSupportedYetException ex)
            {
                DisplayError($"Sorry but {ex.SellingStrategy} selling strategy is not supported yet.");
            }
            catch (NotEnoughSharesToSellException)
            {
                DisplayError("You do not have enough shares to sell.");
            }

            if (AskIfUserWantsToDoAnotherCalculation())
            {
                DisplayCalculationSessionSeparator();
                continue;
            }

            DisplayFarewellMessage();
            return;
        }
    }

    private static IReadOnlyCollection<Lot> Map(IEnumerable<LotDto> lotDtos)
    {
        return lotDtos.Select(Map).ToArray();
    }

    private static Lot Map(LotDto lotDto)
    {
        return new Lot(lotDto.Count, lotDto.PriceUSD, Map(lotDto.TradeMonth));
    }

    private static Month Map(MonthDto monthDto)
    {
        return monthDto switch
        {
            MonthDto.January => Month.January,
            MonthDto.February => Month.February,
            MonthDto.March => Month.March,
            MonthDto.April => Month.April,
            MonthDto.May => Month.May,
            MonthDto.June => Month.June,
            MonthDto.July => Month.July,
            MonthDto.August => Month.August,
            MonthDto.September => Month.September,
            MonthDto.October => Month.October,
            MonthDto.November => Month.November,
            MonthDto.December => Month.December,
            _ => throw new ArgumentOutOfRangeException(nameof(monthDto), monthDto, message: null)
        };
    }

    private static void DisplayCalculationResult(InvestmentSellingCalculationResult calculationResult)
    {
        Console.WriteLine("Calculation result:");

        Console.WriteLine($"  1. The remaining number of shares after sale: {calculationResult.RemainingNumberOfShares}");

        var costBasisOfSoldSharesUSD = NullableMoneyUSDToString(calculationResult.CostBasisOfSoldSharesUSD);
        Console.WriteLine($"  2. The cost basis per share of the sold shares: {costBasisOfSoldSharesUSD}");

        var costBasisOfRemainingSharesUSD = NullableMoneyUSDToString(calculationResult.CostBasisOfRemainingSharesUSD);
        Console.WriteLine($"  3. The cost basis per share of the remaining shares after the sale: {costBasisOfRemainingSharesUSD}.");

        Console.WriteLine($"  4. The total profit or loss of the sale: ${calculationResult.ProfitUSD:F2}");
    }

    private static string NullableMoneyUSDToString(decimal? money)
    {
        return money.HasValue ? $"${money:F2}" : "N/A";
    }

    private static void DisplayFarewellMessage()
    {
        Console.WriteLine("Thank you! Goodbye.");
    }

    private static void DisplayCalculationSessionSeparator()
    {
        Console.WriteLine("-------------------------------------------------");
    }

    private static bool AskIfUserWantsToDoAnotherCalculation()
    {
        while (true)
        {
            Console.Write("Do you want to calculate one more time (y/n)?: ");
            var rawValue = Console.ReadLine();
            switch (rawValue)
            {
                case "y": return true;
                case "n": return false;
                default:
                    DisplayError("The entered value is not valid. Please enter 'y' or 'n'.");
                    break;
            }
        }
    }

    private static uint AskNumberSharesToSell()
    {
        while (true)
        {
            Console.Write("Please enter number of shares to sell: ");
            var rawValue = Console.ReadLine();
            if (uint.TryParse(rawValue, out var result) && result > 0)
            {
                return result;
            }

            DisplayError("The entered value is not valid. It should be an integer number and should be greater than zero.");
        }
    }

    private static decimal AskShareSellPriceUSD()
    {
        while (true)
        {
            Console.Write("Please enter price in USD per share: ");
            var rawValue = Console.ReadLine();
            if (decimal.TryParse(rawValue, out var result) && result > 0)
            {
                return result;
            }

            DisplayError("The entered value is not valid. It should be a number and should be greater than zero.");
        }
    }

    private static LotSellingStrategy AskLotSellingStrategy()
    {
        var availableStrategies = Enum.GetValues<LotSellingStrategy>().ToArray();
        Console.WriteLine("Available selling strategies:");
        for (var i = 0; i < availableStrategies.Length; i++)
        {
            Console.WriteLine($"  Enter {i} for {availableStrategies[i]} selling strategy");
        }

        while (true)
        {
            Console.Write("Please select a strategy number: ");
            var rawValue = Console.ReadLine();
            if (int.TryParse(rawValue, out var selectedIndex) && selectedIndex >= 0 && selectedIndex < availableStrategies.Length)
            {
                return availableStrategies[selectedIndex];
            }

            DisplayError("The entered value is not valid. It should be an one of specified above.");
        }
    }

    private static void DisplayPurchaseLots(IEnumerable<Lot> purchaseLots)
    {
        Console.WriteLine("Available purchase lots:");
        foreach (var purchaseLot in purchaseLots)
        {
            DisplayPurchaseLot(purchaseLot);
        }
    }

    private static void DisplayPurchaseLot(Lot purchaseLot)
    {
        Console.WriteLine($"   {purchaseLot.Count} shares purchased at ${purchaseLot.PriceUSD}/share in {purchaseLot.TradeMonth}");
    }

    private async Task InitializeData(CancellationToken cancellationToken)
    {
        foreach (var purchaseLot in _purchaseLots)
        {
            await _purchaseLotsRepository.WritePurchaseLotAsync(purchaseLot, cancellationToken);
        }
    }

    private static void DisplayError(string message)
    {
        DisplayColoredMessage(message, ConsoleColor.Red);
    }

    private static void DisplayDebuggingError(Exception exception)
    {
#if DEBUG
        DisplayError(exception.ToString());
#endif
    }

    private static void DisplayColoredMessage(string message, ConsoleColor color)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = previousColor;
    }
}
