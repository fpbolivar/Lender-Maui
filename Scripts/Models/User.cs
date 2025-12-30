namespace Lender.Models;

/// <summary>
/// Represents a user in the lending platform
/// </summary>
public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0;
    public decimal CreditScore { get; set; } = 750;
    public int LoansGiven { get; set; } = 0;
    public int LoansReceived { get; set; } = 0;
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public string ProfileImageUrl { get; set; } = string.Empty;
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    Verified,
    Unverified
}
