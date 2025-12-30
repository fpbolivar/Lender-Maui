namespace Lender.Models;

/// <summary>
/// Represents a loan request in the peer-to-peer lending system
/// </summary>
public class LoanRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int DurationMonths { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Pending;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public int FundedPercentage { get; set; } = 0;
    public decimal AmountFunded { get; set; } = 0;
}

public enum LoanStatus
{
    Pending,
    Active,
    Funded,
    Repaying,
    Completed,
    Defaulted,
    Rejected
}
