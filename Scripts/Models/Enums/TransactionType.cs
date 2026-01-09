namespace Lender.Models;

/// <summary>
/// Transaction kinds handled by the system.
/// </summary>
public enum TransactionType
{
    Funding,      // User funding a loan
    Repayment,    // Borrower repaying the loan
    Interest,     // Interest payment
    Transfer,     // User to user transfer
    Withdrawal,   // Withdrawal from wallet
    Deposit       // Deposit to wallet
}
