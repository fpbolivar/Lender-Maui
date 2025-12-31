namespace Lender.Calculators;

public static class SavingsCalculator
{
    public record SavingsResult(decimal FutureValue, decimal TotalContribution, decimal TotalInterestEarned);

    public static SavingsResult CalculateFutureValue(
        decimal initialAmount,
        decimal monthlyContribution,
        decimal annualRatePercent,
        int years,
        int compoundsPerYear = 12)
    {
        decimal ratePerPeriod = (annualRatePercent / 100m) / compoundsPerYear;
        int totalPeriods = compoundsPerYear * years;

        decimal growth = Pow(1m + ratePerPeriod, totalPeriods);
        decimal initialFuture = initialAmount * growth;

        decimal contributionFuture;
        if (ratePerPeriod == 0)
        {
            contributionFuture = monthlyContribution * totalPeriods;
        }
        else
        {
            // Contributions made every period, end of period assumption
            contributionFuture = monthlyContribution * ((growth - 1m) / ratePerPeriod);
        }

        decimal futureValue = initialFuture + contributionFuture;
        decimal totalContribution = initialAmount + (monthlyContribution * totalPeriods);
        decimal totalInterestEarned = futureValue - totalContribution;

        return new SavingsResult(futureValue, totalContribution, totalInterestEarned);
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
