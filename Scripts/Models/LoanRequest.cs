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
    /// Risk rating for investors
    /// </summary>
    public string RiskRating { get; set; } = "Medium";

    // Extended fields for richer loan details
    public bool IsOffer { get; set; } = false; // true if user is offering to lend
    public Lender.Models.Enums.InterestType InterestType { get; set; } = Lender.Models.Enums.InterestType.Amortized;
    public int PaymentsPerYear { get; set; } = 12;
    public Lender.Models.Enums.TermUnit TermUnit { get; set; } = Lender.Models.Enums.TermUnit.Months;
    public int TermLength { get; set; } = 12; // value in selected TermUnit

    public bool HasDownPayment { get; set; } = false;
    public decimal DownPaymentAmount { get; set; } = 0m;

    public bool HasCollateral { get; set; } = false;
    public string CollateralName { get; set; } = string.Empty;
    public string CollateralImageUrl { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;
}
