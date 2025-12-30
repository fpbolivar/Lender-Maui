namespace Lender.Models;

/// <summary>
/// Represents a budget category with limits and spending tracking
/// </summary>
public class Budget
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal BudgetLimit { get; set; }
    public decimal AmountSpent { get; set; } = 0;
    public decimal AmountRemaining => BudgetLimit - AmountSpent;
    public string IconEmoji { get; set; } = "📦";
    public string ColorHex { get; set; } = "#FFFFFF";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public decimal GetPercentageUsed() => BudgetLimit > 0 ? (AmountSpent / BudgetLimit) * 100 : 0;
}
