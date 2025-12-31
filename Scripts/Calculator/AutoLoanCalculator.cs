using System;

namespace Lender.Calculators;

public static class AutoLoanCalculator
{
    public record AutoLoanResult(decimal AmountFinanced, decimal PaymentPerPeriod, decimal TotalPayment, decimal TotalInterest, int TotalPayments, int PaymentsPerYear);

    public static AutoLoanResult CalculateAutoLoan(
        decimal vehiclePrice,
        decimal downPayment,
        decimal tradeInValue,
        decimal salesTaxPercent,
        decimal fees,
        decimal annualRatePercent,
        int termMonths,
        int paymentsPerYear = 12)
    {
        decimal taxableAmount = vehiclePrice - downPayment - tradeInValue;
        if (taxableAmount < 0) taxableAmount = 0;

        decimal salesTax = taxableAmount * (salesTaxPercent / 100m);
        decimal amountFinanced = taxableAmount + salesTax + fees;

        decimal ratePerPeriod = (annualRatePercent / 100m) / paymentsPerYear;
        int totalPayments = Math.Max(1, (int)Math.Round((termMonths / 12m) * paymentsPerYear));

        decimal paymentPerPeriod;
        if (ratePerPeriod == 0)
        {
            paymentPerPeriod = amountFinanced / totalPayments;
        }
        else
        {
            decimal factor = Pow(1m + ratePerPeriod, totalPayments);
            paymentPerPeriod = amountFinanced * (ratePerPeriod * factor) / (factor - 1m);
        }

        decimal totalPayment = paymentPerPeriod * totalPayments;
        decimal totalInterest = totalPayment - amountFinanced;

        return new AutoLoanResult(amountFinanced, paymentPerPeriod, totalPayment, totalInterest, totalPayments, paymentsPerYear);
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
