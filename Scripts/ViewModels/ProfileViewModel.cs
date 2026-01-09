using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lender.Helpers;
using Lender.Services;
using Lender.Models;

namespace Lender.ViewModels;

public class ProfileViewModel : INotifyPropertyChanged
{
    private string _userName = string.Empty;
    private string _userEmail = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _dateOfBirth = string.Empty;
    private string _memberSince = string.Empty;
    private string _userInitials = "U";
    private decimal _balance;
    private int _creditScore;
    private int _totalLoans;
    private decimal _totalLent;
    private decimal _totalBorrowed;
    private int _onTimeRate = 100;

    private readonly IAuthenticationService _authService;
    private readonly FirestoreService _firestoreService;

    public ICommand BackCommand { get; }
    public ICommand EditProfileCommand { get; }
    public ICommand ChangeEmailCommand { get; }
    public ICommand ChangePasswordCommand { get; }
    public ICommand ChangePhoneCommand { get; }
    public ICommand NotificationsCommand { get; }
    public ICommand LanguageCommand { get; }
    public ICommand PrivacyCommand { get; }
    public ICommand AboutCommand { get; }
    public ICommand SignOutCommand { get; }
    public ICommand DeleteAccountCommand { get; }
    public ICommand NavigateToTransactionsCommand { get; }
    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToRequestLoanCommand { get; }
    public ICommand NavigateToCalculatorCommand { get; }

    public ProfileViewModel()
    {
        _authService = ServiceHelper.GetService<IAuthenticationService>() ?? 
            throw new InvalidOperationException("AuthenticationService not registered");
        _firestoreService = FirestoreService.Instance;

        BackCommand = new Command(async () => await GoBack());
        EditProfileCommand = new Command(async () => await EditProfile());
        ChangeEmailCommand = new Command(async () => await ChangeEmail());
        ChangePasswordCommand = new Command(async () => await ChangePassword());
        ChangePhoneCommand = new Command(async () => await ChangePhone());
        NotificationsCommand = new Command(async () => await OpenNotifications());
        LanguageCommand = new Command(async () => await OpenLanguage());
        PrivacyCommand = new Command(async () => await OpenPrivacy());
        AboutCommand = new Command(async () => await OpenAbout());
        SignOutCommand = new Command(async () => await SignOut());
        DeleteAccountCommand = new Command(async () => await DeleteAccount());
        NavigateToTransactionsCommand = new Command(async () => await NavigateToTransactions());
        NavigateToDashboardCommand = new Command(async () => await NavigateToDashboard());
        NavigateToRequestLoanCommand = new Command(async () => await NavigateToRequestLoan());
        NavigateToCalculatorCommand = new Command(async () => await NavigateToCalculator());
        
        // Initialize with defaults
        UserName = "Loading...";
        UserEmail = "Loading...";
        PhoneNumber = "Not provided";
        DateOfBirth = "Not provided";
        MemberSince = "Recently";
        UserInitials = "U";
    }

    public string UserName
    {
        get => _userName;
        set
        {
            if (_userName != value)
            {
                _userName = value;
                OnPropertyChanged();
                UpdateInitials();
            }
        }
    }

    public string UserEmail
    {
        get => _userEmail;
        set
        {
            if (_userEmail != value)
            {
                _userEmail = value;
                OnPropertyChanged();
            }
        }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (_phoneNumber != value)
            {
                _phoneNumber = value;
                OnPropertyChanged();
            }
        }
    }

    public string DateOfBirth
    {
        get => _dateOfBirth;
        set
        {
            if (_dateOfBirth != value)
            {
                _dateOfBirth = value;
                OnPropertyChanged();
            }
        }
    }

    public string MemberSince
    {
        get => _memberSince;
        set
        {
            if (_memberSince != value)
            {
                _memberSince = value;
                OnPropertyChanged();
            }
        }
    }

    public string UserInitials
    {
        get => _userInitials;
        set
        {
            if (_userInitials != value)
            {
                _userInitials = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal Balance
    {
        get => _balance;
        set
        {
            if (_balance != value)
            {
                _balance = value;
                OnPropertyChanged();
            }
        }
    }

    public int CreditScore
    {
        get => _creditScore;
        set
        {
            if (_creditScore != value)
            {
                _creditScore = value;
                OnPropertyChanged();
            }
        }
    }

    public int TotalLoans
    {
        get => _totalLoans;
        set
        {
            if (_totalLoans != value)
            {
                _totalLoans = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal TotalLent
    {
        get => _totalLent;
        set
        {
            if (_totalLent != value)
            {
                _totalLent = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal TotalBorrowed
    {
        get => _totalBorrowed;
        set
        {
            if (_totalBorrowed != value)
            {
                _totalBorrowed = value;
                OnPropertyChanged();
            }
        }
    }

    public int OnTimeRate
    {
        get => _onTimeRate;
        set
        {
            if (_onTimeRate != value)
            {
                _onTimeRate = value;
                OnPropertyChanged();
            }
        }
    }

    public async Task RefreshAsync()
    {
        try
        {
            var currentEmail = _authService.CurrentUserEmail;
            if (string.IsNullOrEmpty(currentEmail))
            {
                // Demo mode fallback – ensure no previous user data leaks
                UserName = "Demo User";
                UserEmail = "demo@example.com";
                PhoneNumber = "Not provided";
                DateOfBirth = "Not provided";
                MemberSince = "Demo";
                Balance = 0;
                CreditScore = 0;
                TotalLoans = 0;
                TotalLent = 0;
                TotalBorrowed = 0;
                OnTimeRate = 100;
                UpdateInitials();
                System.Diagnostics.Debug.WriteLine("Profile set to demo defaults (no user email)");
                return;
            }

            var user = await _firestoreService.GetUserAsync(currentEmail);
            if (user != null)
            {
                UserName = user.FullName ?? "User";
                UserEmail = user.Email ?? currentEmail;
                PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber) ? "Not provided" : user.PhoneNumber;
                DateOfBirth = user.DateOfBirth != DateTime.MinValue ? user.DateOfBirth.ToString("MMMM dd, yyyy") : "Not provided";
                Balance = user.Balance;
                CreditScore = (int)user.CreditScore;

                // Parse member since from joinDate
                if (user.JoinDate != DateTime.MinValue)
                {
                    MemberSince = user.JoinDate.ToString("MMMM yyyy");
                }
                else
                {
                    MemberSince = "Recently";
                }

                // TODO: Load loan statistics from Firestore when loans collection is implemented
                TotalLoans = 0;
                TotalLent = 0;
                TotalBorrowed = 0;
                OnTimeRate = 100;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
        }
    }

    private void UpdateInitials()
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

    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("//mainpage");
    }

    private async Task EditProfile()
    {
        try
        {
            var page = Application.Current?.Windows[0]?.Page;
            if (page == null) return;
            
            string newName = await page.DisplayPromptAsync(
                "Edit Profile",
                "Enter your new name:",
                initialValue: UserName,
                placeholder: "Your Name");

            if (!string.IsNullOrWhiteSpace(newName) && newName != UserName && newName != "Loading...")
            {
                var email = _authService.CurrentUserEmail;
                if (string.IsNullOrEmpty(email)) return;
                
                var user = await _firestoreService.GetUserAsync(email);
                if (user != null)
                {
                    user.FullName = newName;
                    await _firestoreService.UpdateUserAsync(user);
                    
                    UserName = newName;
                    await page.DisplayAlertAsync("Success", "Profile updated successfully!", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            var page = Application.Current?.Windows[0]?.Page;
            if (page != null)
                await page.DisplayAlertAsync("Error", $"Failed to update profile: {ex.Message}", "OK");
        }
    }

    private async Task ChangePassword()
    {
        try
        {
            var page = Application.Current?.Windows[0]?.Page;
            if (page == null) return;
            
            string currentPassword = await page.DisplayPromptAsync(
                "Change Password",
                "Enter your current password:",
                placeholder: "Current password",
                maxLength: 50);

            if (string.IsNullOrWhiteSpace(currentPassword))
                return;

            string newPassword = await page.DisplayPromptAsync(
                "Change Password",
                "Enter your new password (min 6 characters):",
                placeholder: "New password",
                maxLength: 50);

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                await page.DisplayAlertAsync("Error", "Password must be at least 6 characters", "OK");
                return;
            }

            string confirmPassword = await page.DisplayPromptAsync(
                "Change Password",
                "Confirm your new password:",
                placeholder: "Confirm password",
                maxLength: 50);

            if (newPassword != confirmPassword)
            {
                await page.DisplayAlertAsync("Error", "Passwords do not match", "OK");
                return;
            }

            // Update password via authentication service
            bool success = await _authService.ChangePasswordAsync(currentPassword, newPassword);
            
            if (success)
            {
                await page.DisplayAlertAsync("Success", "Password changed successfully!", "OK");
            }
            else
            {
                await page.DisplayAlertAsync("Error", "Failed to change password. Check your current password.", "OK");
            }
        }
        catch (Exception ex)
        {
            var page = Application.Current?.Windows[0]?.Page;
            if (page != null)
                await page.DisplayAlertAsync("Error", $"Failed to change password: {ex.Message}", "OK");
        }
    }

    private async Task ChangeEmail()
    {
        try
        {
            var page = Application.Current?.Windows[0]?.Page;
            if (page == null) return;
            
            string newEmail = await page.DisplayPromptAsync(
                "Change Email",
                "Enter your new email address:",
                initialValue: UserEmail,
                keyboard: Keyboard.Email,
                placeholder: "email@example.com");

            if (!string.IsNullOrWhiteSpace(newEmail) && newEmail != UserEmail && newEmail != "Loading...")
            {
                bool confirm = await page.DisplayAlertAsync(
                    "Confirm Email Change",
                    $"Change email to {newEmail}?\n\nYou may need to verify the new email and sign in again.",
                    "Change",
                    "Cancel");

                if (confirm)
                {
                    // Update email in authentication and Firestore
                    bool success = await _authService.ChangeEmailAsync(newEmail);
                    
                    if (success)
                    {
                        var email = _authService.CurrentUserEmail;
                        if (!string.IsNullOrEmpty(email))
                        {
                            var user = await _firestoreService.GetUserAsync(email);
                            if (user != null)
                            {
                                user.Email = newEmail;
                                await _firestoreService.UpdateUserAsync(user);
                            }
                        }
                        
                        await page.DisplayAlertAsync(
                            "Email Changed",
                            "Email updated successfully!",
                            "OK");
                        
                        UserEmail = newEmail;
                    }
                    else
                    {
                        await page.DisplayAlertAsync("Error", "Failed to change email", "OK");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var page = Application.Current?.Windows[0]?.Page;
            if (page != null)
                await page.DisplayAlertAsync("Error", $"Failed to change email: {ex.Message}", "OK");
        }
    }

    private async Task ChangePhone()
    {
        try
        {
            var page = Application.Current?.Windows[0]?.Page;
            if (page == null) return;
            
            string currentPhone = PhoneNumber == "Not provided" ? "" : PhoneNumber;
            string newPhone = await page.DisplayPromptAsync(
                "Change Phone Number",
                PhoneNumber == "Not provided" ? "Add your phone number:" : "Update your phone number:",
                initialValue: currentPhone,
                keyboard: Keyboard.Telephone,
                placeholder: "+1234567890");

            if (!string.IsNullOrWhiteSpace(newPhone) && newPhone != currentPhone)
            {
                var email = _authService.CurrentUserEmail;
                if (string.IsNullOrEmpty(email)) return;
                
                var user = await _firestoreService.GetUserAsync(email);
                if (user != null)
                {
                    user.PhoneNumber = newPhone;
                    await _firestoreService.UpdateUserAsync(user);
                    
                    PhoneNumber = newPhone;
                    await page.DisplayAlertAsync("Success", "Phone number updated successfully!", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            var page = Application.Current?.Windows[0]?.Page;
            if (page != null)
                await page.DisplayAlertAsync("Error", $"Failed to update phone: {ex.Message}", "OK");
        }
    }

    private async Task OpenNotifications()
    {
        var page = Application.Current?.Windows[0]?.Page;
        if (page == null) return;
        
        await page.DisplayAlertAsync(
            "Notifications",
            "Configure notification preferences:\n\n" +
            "• Loan payment reminders\n" +
            "• New loan requests\n" +
            "• Payment confirmations\n" +
            "• Interest calculations\n\n" +
            "Full notification settings coming soon!",
            "OK");
    }

    private async Task OpenLanguage()
    {
        var page = Application.Current?.Windows[0]?.Page;
        if (page == null) return;
        
        string language = await page.DisplayActionSheetAsync(
            "Language & Region",
            "Cancel",
            null,
            "English (US)",
            "Español",
            "Français",
            "Deutsch",
            "中文");

        if (!string.IsNullOrEmpty(language) && language != "Cancel")
        {
            await page.DisplayAlertAsync(
                "Language",
                $"{language} selected. Language switching will be implemented in a future update.",
                "OK");
        }
    }

    private async Task OpenPrivacy()
    {
        var page = Application.Current?.Windows[0]?.Page;
        if (page == null) return;
        
        await page.DisplayAlertAsync(
            "Privacy & Security",
            "Privacy Settings:\n\n" +
            "• Two-factor authentication\n" +
            "• Biometric login\n" +
            "• Data encryption\n" +
            "• Export your data\n" +
            "• Privacy policy\n\n" +
            "Full privacy settings coming soon!",
            "OK");
    }

    private async Task OpenAbout()
    {
        var page = Application.Current?.Windows[0]?.Page;
        if (page == null) return;
        
        await page.DisplayAlertAsync(
            "About Lender",
            "Lender - Loan Tracking App\n\n" +
            "Version: 1.0.0\n" +
            "Built with .NET MAUI\n\n" +
            "Features:\n" +
            "• Track loans (lend & borrow)\n" +
            "• Calculate interest (simple & compound)\n" +
            "• Payment management\n" +
            "• PDF export\n\n" +
            "© 2025 Lender. All rights reserved.",
            "OK");
    }

    private async Task SignOut()
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Sign Out",
            "Are you sure you want to sign out?",
            "Yes",
            "Cancel");

        if (confirm)
        {
            await _authService.SignOutAsync();
            await Shell.Current.GoToAsync("//login", animate: true);
        }
    }

    private async Task DeleteAccount()
    {
        try
        {
            var page = Application.Current?.Windows[0]?.Page;
            if (page == null) return;
            
            bool firstConfirm = await page.DisplayAlertAsync(
                "⚠️ Delete Account",
                "This action is PERMANENT and cannot be undone.\n\n" +
                "All your data will be deleted:\n" +
                "• User profile\n" +
                "• All loans (lent & borrowed)\n" +
                "• Transaction history\n" +
                "• Payment records\n\n" +
                "Are you sure you want to continue?",
                "Yes, Delete",
                "Cancel");

            if (!firstConfirm)
                return;

            string password = await page.DisplayPromptAsync(
                "Confirm Deletion",
                "Enter your password to confirm account deletion:",
                placeholder: "Password",
                maxLength: 50);

            if (string.IsNullOrWhiteSpace(password))
                return;

            bool finalConfirm = await page.DisplayAlertAsync(
                "⚠️ FINAL WARNING",
                "This is your last chance to cancel.\n\n" +
                "Type 'DELETE' in the next prompt to permanently delete your account.",
                "Continue",
                "Cancel");

            if (!finalConfirm)
                return;

            string confirmText = await page.DisplayPromptAsync(
                "Type DELETE",
                "Type DELETE (in capitals) to confirm:",
                placeholder: "DELETE",
                maxLength: 10);

            if (confirmText != "DELETE")
            {
                await page.DisplayAlertAsync("Cancelled", "Account deletion cancelled", "OK");
                return;
            }

            // Delete all user data
            var currentEmail = _authService.CurrentUserEmail;
            if (string.IsNullOrEmpty(currentEmail)) return;
            
            // Delete user's loans
            var loans = await _firestoreService.GetUserLoansAsync(currentEmail);
            foreach (var loan in loans)
            {
                await _firestoreService.DeleteLoanAsync(loan.Id);
            }
            
            // Delete user's transactions
            var transactions = await _firestoreService.GetUserTransactionsAsync(currentEmail);
            foreach (var transaction in transactions)
            {
                await _firestoreService.DeleteTransactionAsync(transaction.Id);
            }
            
            // Delete user document
            await _firestoreService.DeleteUserAsync(currentEmail);
            
            // Delete authentication account
            bool authDeleted = await _authService.DeleteAccountAsync(password);
            
            if (authDeleted)
            {
                await page.DisplayAlertAsync(
                    "Account Deleted",
                    "Your account has been permanently deleted.",
                    "OK");
                
                // Navigate to login
                if (Application.Current?.Windows[0] != null)
                    Application.Current.Windows[0].Page = new NavigationPage(new LoginPage());
            }
            else
            {
                await page.DisplayAlertAsync("Error", "Failed to delete account. Check your password.", "OK");
            }
        }
        catch (Exception ex)
        {
            var page = Application.Current?.Windows[0]?.Page;
            if (page != null)
                await page.DisplayAlertAsync("Error", $"Failed to delete account: {ex.Message}", "OK");
        }
    }

    private async Task NavigateToTransactions()
    {
        await Shell.Current.GoToAsync("//transactions");
    }

    private async Task NavigateToDashboard()
    {
        await NavBarNavigation.GoToDashboardAsync();
    }

    private async Task NavigateToRequestLoan()
    {
        await Shell.Current.DisplayAlertAsync("Request/Send Loan", "Navigate to Request/Send Loan page", "OK");
        // TODO: await Shell.Current.GoToAsync("//requestloan");
    }

    private async Task NavigateToCalculator()
    {
        await NavBarNavigation.GoToCalculatorAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
