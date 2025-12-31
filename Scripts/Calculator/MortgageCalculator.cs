using System;

namespace Lender.Calculators;

public static class MortgageCalculator
{
    public record MortgageResult(decimal PaymentPerPeriod, decimal PrincipalAndInterest, decimal Taxes, decimal Insurance, decimal PMI, decimal HOA, decimal TotalPayment, decimal TotalInterest, int TotalPayments, int PaymentsPerYear);

    public static MortgageResult CalculateMortgage(
        decimal homePrice,
        decimal downPayment,
        decimal annualRatePercent,
        int termYears,
        decimal annualPropertyTax,
        decimal annualHomeInsurance,
        decimal monthlyPMI,
        decimal monthlyHOA,
        int paymentsPerYear = 12)
    {
        decimal principal = homePrice - downPayment;
        int totalPayments = Math.Max(1, termYears * paymentsPerYear);
        decimal ratePerPeriod = (annualRatePercent / 100m) / paymentsPerYear;

        decimal principalAndInterest;
        if (ratePerPeriod == 0)
        {
            principalAndInterest = principal / totalPayments;
        }
        else
        {
            decimal factor = Pow(1m + ratePerPeriod, totalPayments);
            principalAndInterest = principal * (ratePerPeriod * factor) / (factor - 1m);
        }

        decimal taxesPerPeriod = annualPropertyTax / paymentsPerYear;
        decimal insurancePerPeriod = annualHomeInsurance / paymentsPerYear;
        decimal pmiPerPeriod = monthlyPMI * 12m / paymentsPerYear;
        decimal hoaPerPeriod = monthlyHOA * 12m / paymentsPerYear;
        decimal paymentPerPeriod = principalAndInterest + taxesPerPeriod + insurancePerPeriod + pmiPerPeriod + hoaPerPeriod;

        decimal totalPayment = paymentPerPeriod * totalPayments;
        decimal totalInterest = (principalAndInterest * totalPayments) - principal;

        return new MortgageResult(
            PaymentPerPeriod: paymentPerPeriod,
            PrincipalAndInterest: principalAndInterest,
            Taxes: taxesPerPeriod,
            Insurance: insurancePerPeriod,
            PMI: pmiPerPeriod,
            HOA: hoaPerPeriod,
            TotalPayment: totalPayment,
            TotalInterest: totalInterest,
            TotalPayments: totalPayments,
            PaymentsPerYear: paymentsPerYear);
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
