namespace Lender.Models;

/// <summary>
/// Represents an investment in a loan by a lender
/// Tracks the relationship between investors and loans
/// </summary>
public class LoanInvestment
{
    /// <summary>
    /// Unique investment ID - Primary key in Firestore
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the loan being invested in - Foreign key to LoanRequests
    /// </summary>
    public string LoanRequestId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the investor - Foreign key to Users
    /// </summary>
    public string InvestorUserId { get; set; } = string.Empty;

    /// <summary>
    /// Amount invested by this investor
    /// </summary>
    public decimal InvestmentAmount { get; set; }

    /// <summary>
    /// Date of investment
    /// </summary>
    public DateTime InvestmentDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Expected return amount (principal + interest share)
    /// </summary>
    public decimal ExpectedReturn { get; set; }

    /// <summary>
    /// Amount already returned to investor
    /// </summary>
    public decimal AmountReturned { get; set; } = 0;

    /// <summary>
    /// Status of the investment
    /// </summary>
    public InvestmentStatus Status { get; set; } = InvestmentStatus.Active;

    /// <summary>
    /// Interest rate for this investor (can vary based on loan terms)
    /// </summary>
    public decimal InterestRate { get; set; }

    /// <summary>
    /// Next payment due date
    /// </summary>
    public DateTime? NextPaymentDate { get; set; }

    /// <summary>
    /// Number of payments completed
    /// </summary>
    public int PaymentsCompleted { get; set; } = 0;

    /// <summary>
    /// Total number of payments expected
    /// </summary>
    public int TotalPaymentsExpected { get; set; }
}
