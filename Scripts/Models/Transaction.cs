namespace Lender.Models;

/// <summary>
/// Represents a transaction in the loan system (loan funding, repayment, etc.)
/// </summary>
public class Transaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LoanRequestId { get; set; } = string.Empty;
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime CompletedDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Merchant { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public enum TransactionType
{
    Funding,
    Repayment,
    Interest,
    Transfer,
    Withdrawal,
    Deposit
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
}
