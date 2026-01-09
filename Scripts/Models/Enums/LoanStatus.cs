namespace Lender.Models;

/// <summary>
/// Represents the status of a loan request.
/// </summary>
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
