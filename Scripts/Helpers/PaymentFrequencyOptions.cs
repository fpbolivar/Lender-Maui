using System.Collections.Generic;

namespace Lender.Helpers;

public record PaymentFrequencyOption(string Label, int PaymentsPerYear)
{
    public static readonly IReadOnlyList<PaymentFrequencyOption> All = new List<PaymentFrequencyOption>
    {
        new("One Time", 1),
        new("Two Payments", 2),
        new("Daily", 365),
        new("Weekly", 52),
        new("Bi-week", 26),
        new("Half Month", 24),
        new("Monthly", 12)
    };

    public static PaymentFrequencyOption Default => All[6]; // Default to Monthly
}
