using Lender.ViewModels;
using Lender.Helpers;

namespace Lender.Services;

public class PdfTestHelper
{
    public static void TestPaymentDates()
    {
        // Test Case 1: Monthly payments for 3 months
        Console.WriteLine("=== Test Case 1: Monthly payments for 3 months ===");
        var test1 = SimulatePaymentDates(
            startDate: DateTime.Today,
            paymentsPerYear: 12,
            totalPayments: 3
        );
        
        // Test Case 2: Weekly payments for 4 weeks
        Console.WriteLine("\n=== Test Case 2: Weekly payments for 4 weeks ===");
        var test2 = SimulatePaymentDates(
            startDate: DateTime.Today,
            paymentsPerYear: 52,
            totalPayments: 4
        );
        
        // Test Case 3: Bi-weekly payments for 6 payments
        Console.WriteLine("\n=== Test Case 3: Bi-weekly payments for 6 payments ===");
        var test3 = SimulatePaymentDates(
            startDate: DateTime.Today,
            paymentsPerYear: 26,
            totalPayments: 6
        );
        
        // Test Case 4: One Time payment
        Console.WriteLine("\n=== Test Case 4: One Time payment ===");
        var test4 = SimulatePaymentDates(
            startDate: DateTime.Today,
            paymentsPerYear: 1,
            totalPayments: 1
        );
        
        // Test Case 5: Two Payments
        Console.WriteLine("\n=== Test Case 5: Two Payments (half and half) ===");
        var test5 = SimulatePaymentDates(
            startDate: DateTime.Today,
            paymentsPerYear: 2,
            totalPayments: 2
        );
    }
    
    private static List<DateTime> SimulatePaymentDates(DateTime startDate, int paymentsPerYear, int totalPayments)
    {
        var dates = new List<DateTime>();
        
        for (int i = 1; i <= totalPayments; i++)
        {
            DateTime paymentDate;
            
            if (paymentsPerYear > 0)
            {
                double daysPerPeriod = 365.0 / paymentsPerYear;
                paymentDate = startDate.AddDays(daysPerPeriod * (i - 1));
            }
            else
            {
                paymentDate = startDate;
            }
            
            dates.Add(paymentDate);
            Console.WriteLine($"Payment {i}: {paymentDate:MMM dd, yyyy} (Day {(paymentDate - startDate).Days})");
        }
        
        return dates;
    }
}
