namespace Lender.Models;

/// <summary>
/// Represents a user in the lending platform
/// </summary>
public class User
{
    /// <summary>
    /// Firebase UID - Primary key in Firestore
    /// </summary>
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Available balance in the wallet
    /// </summary>
    public decimal Balance { get; set; } = 0;

    /// <summary>
    /// Credit score (default 750, ranges 300-850)
    /// </summary>
    public decimal CreditScore { get; set; } = 750;

    /// <summary>
    /// Number of loans user has given out
    /// </summary>
    public int LoansGiven { get; set; } = 0;

    /// <summary>
    /// Number of loans user has received
    /// </summary>
    public int LoansReceived { get; set; } = 0;

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Current user account status
    /// </summary>
    public UserStatus Status { get; set; } = UserStatus.Active;

    /// <summary>
    /// Profile picture URL
    /// </summary>
    public string ProfileImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// User's date of birth
    /// </summary>
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Verification status for lending
    /// </summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Total amount lent out (all time)
    /// </summary>
    public decimal TotalLent { get; set; } = 0;

    /// <summary>
    /// Total amount borrowed (all time)
    /// </summary>
    public decimal TotalBorrowed { get; set; } = 0;

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    Verified,
    Unverified
}
