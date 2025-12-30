namespace Lender.Models;

/// <summary>
/// Represents a budget category with limits and spending tracking
/// </summary>
public class Budget
{
    /// <summary>
    /// Unique budget ID - Primary key in Firestore
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// User who owns this budget - Foreign key to Users
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Budget category name (e.g., Food, Transport, Entertainment)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Monthly budget limit for this category
    /// </summary>
    public decimal BudgetLimit { get; set; }

    /// <summary>
    /// Amount spent in the current period
    /// </summary>
    public decimal AmountSpent { get; set; } = 0;

    /// <summary>
    /// Remaining budget amount
    /// </summary>
    public decimal AmountRemaining => BudgetLimit - AmountSpent;

    /// <summary>
    /// Icon/emoji for the category
    /// </summary>
    public string IconEmoji { get; set; } = "📦";

    /// <summary>
    /// Color hex code for UI display
    /// </summary>
    public string ColorHex { get; set; } = "#FFFFFF";

    /// <summary>
    /// When the budget was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The month/year this budget applies to (e.g., "2025-01")
    /// </summary>
    public string PeriodMonthYear { get; set; } = DateTime.UtcNow.ToString("yyyy-MM");

    /// <summary>
    /// Whether this budget is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Budget warning threshold (percentage, e.g., 80 for 80% alert)
    /// </summary>
    public int WarningThreshold { get; set; } = 80;

    public decimal GetPercentageUsed() => BudgetLimit > 0 ? (AmountSpent / BudgetLimit) * 100 : 0;

    public bool IsOverBudget() => AmountSpent > BudgetLimit;

    public bool ShouldShowWarning() => GetPercentageUsed() >= WarningThreshold;
}

