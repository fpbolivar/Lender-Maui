using System.ComponentModel;

namespace Lender.Services;

public interface IAuthenticationService : INotifyPropertyChanged
{
    bool IsAuthenticated { get; }
    string? CurrentUserId { get; }
    string? CurrentUserEmail { get; }
    Task<bool> SignUpAsync(string email, string password, string firstName, string lastName, string? phoneNumber = null, DateTime? dateOfBirth = null);
    Task<bool> SignInAsync(string email, string password);
    Task<bool> SignInWithGoogleAsync();
    Task SignOutAsync();
    Task<bool> RestoreSessionAsync();
    Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
    Task<bool> ChangeEmailAsync(string newEmail);
    Task<bool> DeleteAccountAsync(string password);
}
