using System.Windows.Input;
using Lender.Helpers;
using Lender.Services;
using Lender.Models;

namespace Lender.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private UserProfile _userProfile = new UserProfile();
    private LoanStatistics _loanStatistics = LoanStatistics.CreateEmpty();

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
    public ICommand NavigateToLoansCommand { get; }
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
        NavigateToLoansCommand = new Command(async () => { await Shell.Current.GoToAsync("loanform"); });
        NavigateToCalculatorCommand = new Command(async () => await NavigateToCalculator());
        
        // Initialize with demo data instead of loading text
        // This ensures UI shows something immediately while refresh happens
        _userProfile = UserProfile.CreateDemo();
        _loanStatistics = LoanStatistics.CreateEmpty();
    }

    // Expose UserProfile properties for data binding
    public string UserName
    {
        get => _userProfile.UserName;
        set
        {
            if (_userProfile.UserName != value)
            {
                _userProfile.UserName = value;
                _userProfile.UpdateInitials();
                OnPropertyChanged();
                OnPropertyChanged(nameof(UserInitials));
            }
        }
    }

    public string UserEmail
    {
        get => _userProfile.UserEmail;
        set
        {
            if (_userProfile.UserEmail != value)
            {
                _userProfile.UserEmail = value;
                OnPropertyChanged();
            }
        }
    }

    public string PhoneNumber
    {
        get => _userProfile.PhoneNumber;
        set
        {
            if (_userProfile.PhoneNumber != value)
            {
                _userProfile.PhoneNumber = value;
                OnPropertyChanged();
            }
        }
    }

    public string DateOfBirth
    {
        get => _userProfile.DateOfBirthDisplay;
        set
        {
            if (_userProfile.DateOfBirthDisplay != value)
            {
                _userProfile.DateOfBirthDisplay = value;
                OnPropertyChanged();
            }
        }
    }

    public string MemberSince
    {
        get => _userProfile.MemberSince;
        set
        {
            if (_userProfile.MemberSince != value)
            {
                _userProfile.MemberSince = value;
                OnPropertyChanged();
            }
        }
    }

    public string UserInitials
    {
        get => _userProfile.UserInitials;
        set
        {
            if (_userProfile.UserInitials != value)
            {
                _userProfile.UserInitials = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal Balance
    {
        get => _userProfile.Balance;
        set
        {
            if (_userProfile.Balance != value)
            {
                _userProfile.Balance = value;
                OnPropertyChanged();
            }
        }
    }

    // Expose LoanStatistics properties for data binding
    public int TotalLoans
    {
        get => _loanStatistics.TotalLoans;
        set
        {
            if (_loanStatistics.TotalLoans != value)
            {
                _loanStatistics.TotalLoans = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal TotalLent
    {
        get => _loanStatistics.TotalLent;
        set
        {
            if (_loanStatistics.TotalLent != value)
            {
                _loanStatistics.TotalLent = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal TotalBorrowed
    {
        get => _loanStatistics.TotalBorrowed;
        set
        {
            if (_loanStatistics.TotalBorrowed != value)
            {
                _loanStatistics.TotalBorrowed = value;
                OnPropertyChanged();
            }
        }
    }

    public int OnTimeRate
    {
        get => _loanStatistics.OnTimePaymentRate;
        set
        {
            if (_loanStatistics.OnTimePaymentRate != value)
            {
                _loanStatistics.OnTimePaymentRate = value;
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
                _userProfile = UserProfile.CreateDemo();
                _loanStatistics = LoanStatistics.CreateEmpty();
                
                // Notify all properties
                NotifyAllPropertiesChanged();
                
                System.Diagnostics.Debug.WriteLine("Profile set to demo defaults (no user email) - ProfileViewModel.cs:221");
                return;
            }

            var user = await _firestoreService.GetUserAsync(currentEmail);
            if (user != null)
            {
                _userProfile = UserProfile.FromUser(user);
                
                // TODO: Load loan statistics from Firestore when loans collection is implemented
                _loanStatistics = LoanStatistics.CreateEmpty();
                
                // Notify all properties
                NotifyAllPropertiesChanged();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message} - ProfileViewModel.cs:239");
        }
    }

    private void NotifyAllPropertiesChanged()
    {
        OnPropertyChanged(nameof(UserName));
        OnPropertyChanged(nameof(UserEmail));
        OnPropertyChanged(nameof(PhoneNumber));
        OnPropertyChanged(nameof(DateOfBirth));
        OnPropertyChanged(nameof(MemberSince));
        OnPropertyChanged(nameof(UserInitials));
        OnPropertyChanged(nameof(Balance));
        OnPropertyChanged(nameof(TotalLoans));
        OnPropertyChanged(nameof(TotalLent));
        OnPropertyChanged(nameof(TotalBorrowed));
        OnPropertyChanged(nameof(OnTimeRate));
    }

    private void UpdateInitials()
    {
        _userProfile.UpdateInitials();
        OnPropertyChanged(nameof(UserInitials));
    }

    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("//mainpage");
    }

    private async Task EditProfile()
    {
        try
        {
            string? newName = await DialogService.ShowPromptAsync(
                "Enter your new name:",
                "Edit Profile",
                "Your Name",
                UserName);

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
                    await DialogService.ShowSuccessAsync("Profile updated successfully!");
                }
            }
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync($"Failed to update profile: {ex.Message}");
        }
    }

    private async Task ChangePassword()
    {
        try
        {
            string? currentPassword = await DialogService.ShowPromptAsync(
                "Enter your current password:",
                "Change Password",
                "Current password");

            if (string.IsNullOrWhiteSpace(currentPassword))
                return;

            string? newPassword = await DialogService.ShowPromptAsync(
                "Enter your new password (min 6 characters):",
                "Change Password",
                "New password");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                await DialogService.ShowErrorAsync("Password must be at least 6 characters");
                return;
            }

            string? confirmPassword = await DialogService.ShowPromptAsync(
                "Confirm your new password:",
                "Change Password",
                "Confirm password");

            if (newPassword != confirmPassword)
            {
                await DialogService.ShowErrorAsync("Passwords do not match");
                return;
            }

            bool success = await _authService.ChangePasswordAsync(currentPassword, newPassword);
            
            if (success)
            {
                await DialogService.ShowSuccessAsync("Password changed successfully!");
            }
            else
            {
                await DialogService.ShowErrorAsync("Failed to change password. Check your current password.");
            }
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync($"Failed to change password: {ex.Message}");
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
        bool confirm = await DialogService.ShowConfirmAsync(
            "Are you sure you want to sign out?",
            "Sign Out");

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


    private async Task NavigateToCalculator()
    {
        await NavBarNavigation.GoToCalculatorAsync();
    }
}
