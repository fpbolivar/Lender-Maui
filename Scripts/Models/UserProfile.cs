namespace Lender.Models;

/// <summary>
/// Represents user profile data for display in ViewModels
/// Separate from User model to handle display-specific formatting
/// </summary>
public class UserProfile
{
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DateOfBirthDisplay { get; set; } = string.Empty;
    public string MemberSince { get; set; } = string.Empty;
    public string UserInitials { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public decimal Balance { get; set; }

    /// <summary>
    /// Calculate initials from UserName
    /// </summary>
    public void UpdateInitials()
    {
        if (!string.IsNullOrEmpty(UserName))
        {
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                UserInitials = $"{parts[0][0]}{parts[1][0]}".ToUpper();
            }
            else if (parts.Length == 1)
            {
                UserInitials = parts[0][0].ToString().ToUpper();
            }
        }
    }

    /// <summary>
    /// Create UserProfile from User model
    /// </summary>
    public static UserProfile FromUser(User user)
    {
        var profile = new UserProfile
        {
            UserName = user.FullName,
            UserEmail = user.Email,
            PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber) ? "Not provided" : user.PhoneNumber,
            DateOfBirthDisplay = user.DateOfBirth != DateTime.MinValue 
                ? user.DateOfBirth.ToString("MMMM dd, yyyy") 
                : "Not provided",
            MemberSince = user.JoinDate != DateTime.MinValue 
                ? user.JoinDate.ToString("MMMM yyyy") 
                : "Recently",
            Status = user.Status.ToString(),
            Balance = user.Balance
        };
        profile.UpdateInitials();
        return profile;
    }

    /// <summary>
    /// Create demo UserProfile
    /// </summary>
    public static UserProfile CreateDemo()
    {
        return new UserProfile
        {
            UserName = "Demo User",
            UserEmail = "demo@example.com",
            PhoneNumber = "Not provided",
            DateOfBirthDisplay = "Not provided",
            MemberSince = "Demo",
            UserInitials = "DU",
            Status = "Active",
            Balance = 0
        };
    }
}
