using System;

namespace Lender.Calculators;

public static class LoanCalculator
{
    public record SimpleInterestResult(decimal TotalInterest, decimal TotalPayment);
    public record CompoundInterestResult(decimal FutureValue, decimal TotalContribution, decimal TotalInterest);
    public record AmortizedPaymentResult(decimal PeriodicPayment, decimal TotalPayment, decimal TotalInterest, int TotalPayments, int PaymentsPerYear);

    public static SimpleInterestResult CalculateSimpleInterest(decimal principal, decimal annualRatePercent, int years)
    {
        decimal rate = annualRatePercent / 100m;
        decimal totalInterest = principal * rate * years;
        decimal totalPayment = principal + totalInterest;
        return new SimpleInterestResult(totalInterest, totalPayment);
    }

    public static CompoundInterestResult CalculateCompoundInterest(decimal principal, decimal annualRatePercent, int compoundsPerYear, int years)
    {
        decimal ratePerPeriod = (annualRatePercent / 100m) / compoundsPerYear;
        int totalPeriods = compoundsPerYear * years;
        decimal growth = Pow(1m + ratePerPeriod, totalPeriods);
        decimal futureValue = principal * growth;
        decimal totalContribution = principal;
        decimal totalInterest = futureValue - totalContribution;
        return new CompoundInterestResult(futureValue, totalContribution, totalInterest);
    }

    public static AmortizedPaymentResult CalculateAmortizedPayment(decimal principal, decimal annualRatePercent, int termMonths, int paymentsPerYear = 12)
    {
        decimal ratePerPeriod = (annualRatePercent / 100m) / paymentsPerYear;
        int totalPayments = Math.Max(1, (int)Math.Round((termMonths / 12m) * paymentsPerYear));

        if (ratePerPeriod == 0)
        {
            decimal payment = principal / totalPayments;
            return new AmortizedPaymentResult(payment, payment * totalPayments, 0, totalPayments, paymentsPerYear);
        }

        decimal factor = Pow(1m + ratePerPeriod, totalPayments);
        decimal paymentPerPeriod = principal * (ratePerPeriod * factor) / (factor - 1m);
        decimal totalPayment = paymentPerPeriod * totalPayments;
        decimal totalInterest = totalPayment - principal;
        return new AmortizedPaymentResult(paymentPerPeriod, totalPayment, totalInterest, totalPayments, paymentsPerYear);
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
