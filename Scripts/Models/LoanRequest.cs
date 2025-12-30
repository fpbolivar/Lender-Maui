namespace Lender.Models;

/// <summary>
/// Represents a loan request in the peer-to-peer lending system
/// </summary>
public class LoanRequest
{
    /// <summary>
    /// Unique loan ID - Primary key in Firestore
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the user requesting the loan - Foreign key to Users collection
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the user (denormalized for display)
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Loan purpose/description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Principal amount requested
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Annual interest rate (percentage, e.g., 5.5 for 5.5%)
    /// </summary>
    public decimal InterestRate { get; set; }

    /// <summary>
    /// Loan duration in months
    /// </summary>
    public int DurationMonths { get; set; }

    /// <summary>
    /// Current status of the loan
    /// </summary>
    public LoanStatus Status { get; set; } = LoanStatus.Pending;

    /// <summary>
    /// When the loan was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the loan is due to be fully repaid
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Loan category (e.g., Education, Business, Personal)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Funding progress percentage (0-100)
    /// </summary>
    public int FundedPercentage { get; set; } = 0;

    /// <summary>
    /// Total amount funded so far
    /// </summary>
    public decimal AmountFunded { get; set; } = 0;

    /// <summary>
    /// When the loan was fully funded
    /// </summary>
    public DateTime? FundedDate { get; set; }

    /// <summary>
    /// Credit score required to invest in this loan
    /// </summary>
    public decimal MinCreditScore { get; set; } = 700;

    /// <summary>
    /// Risk rating for investors
    /// </summary>
    public string RiskRating { get; set; } = "Medium";
}

public enum LoanStatus
{
    Pending,      // Waiting for funding
    Active,       // Currently being funded
    Funded,       // Fully funded, ready for disbursement
    Repaying,     // In repayment phase
    Completed,    // All payments received
    Defaulted,    // Payment default occurred
    Rejected      // Application rejected
}
