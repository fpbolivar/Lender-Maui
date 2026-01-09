namespace Lender.Models;

/// <summary>
/// Status for investments in loans.
/// </summary>
public enum InvestmentStatus
{
    Active,       // Investment is active and receiving payments
    Completed,    // All payments received
    Defaulted,    // Borrower defaulted
    Pending,      // Waiting for loan to be funded
    Cancelled     // Investment cancelled
}
