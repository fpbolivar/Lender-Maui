namespace Lender.Calculators;

public static class InvestmentCalculator
{
    public record InvestmentResult(decimal FutureValue, decimal TotalContribution, decimal TotalGrowth);

    public static InvestmentResult CalculateInvestment(
        decimal initialAmount,
        decimal periodicContribution,
        decimal annualRatePercent,
        int years,
        int contributionsPerYear = 12)
    {
        decimal ratePerPeriod = (annualRatePercent / 100m) / contributionsPerYear;
        int totalPeriods = contributionsPerYear * years;

        decimal growth = Pow(1m + ratePerPeriod, totalPeriods);
        decimal initialFuture = initialAmount * growth;

        decimal contributionFuture;
        if (ratePerPeriod == 0)
        {
            contributionFuture = periodicContribution * totalPeriods;
        }
        else
        {
            // Contributions at end of period
            contributionFuture = periodicContribution * ((growth - 1m) / ratePerPeriod);
        }

        decimal futureValue = initialFuture + contributionFuture;
        decimal totalContribution = initialAmount + (periodicContribution * totalPeriods);
        decimal totalGrowth = futureValue - totalContribution;

        return new InvestmentResult(futureValue, totalContribution, totalGrowth);
    }

    private static decimal Pow(decimal value, int exponent)
    {
        decimal result = 1m;
        for (int i = 0; i < exponent; i++)
        {
            result *= value;
        }
        return result;
    }
}
