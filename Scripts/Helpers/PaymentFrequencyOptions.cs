using System.Collections.Generic;

namespace Lender.Helpers;

public record PaymentFrequencyOption(string Label, int PaymentsPerYear)
{
    public static readonly IReadOnlyList<PaymentFrequencyOption> All = new List<PaymentFrequencyOption>
    {
        new("Monthly", 12),
        new("Biweekly", 26),
        new("Weekly", 52),
        new("Quarterly", 4),
        new("Yearly", 1)
    };

    public static PaymentFrequencyOption Default => All[0];
}
