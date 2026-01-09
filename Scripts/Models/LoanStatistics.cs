namespace Lender.Models;

/// <summary>
/// Represents loan statistics for display
/// </summary>
public class LoanStatistics
{
    public int TotalLoans { get; set; }
    public decimal TotalLent { get; set; }
    public decimal TotalBorrowed { get; set; }
    public decimal ExpectedReturn { get; set; }
    public string NextPaymentDisplay { get; set; } = string.Empty;
    public int OnTimePaymentRate { get; set; } = 100;

    /// <summary>
    /// Create empty statistics
    /// </summary>
    public static LoanStatistics CreateEmpty()
    {
        return new LoanStatistics
        {
            TotalLoans = 0,
            TotalLent = 0,
            TotalBorrowed = 0,
            ExpectedReturn = 0,
            NextPaymentDisplay = "No upcoming payments",
            OnTimePaymentRate = 100
        };
    }

    /// <summary>
    /// Create demo statistics
    /// </summary>
    public static LoanStatistics CreateDemo()
    {
        return new LoanStatistics
        {
            TotalLoans = 3,
            TotalLent = 5000m,
            TotalBorrowed = 2000m,
            ExpectedReturn = 5200m,
            NextPaymentDisplay = "Jan 15, 2026 - $200",
            OnTimePaymentRate = 100
        };
    }
}
