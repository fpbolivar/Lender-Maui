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
    public ICommand ChangePasswordCommand { get; }
    public ICommand NotificationsCommand { get; }
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
        ChangePasswordCommand = new Command(async () => await ChangePassword());
        NotificationsCommand = new Command(async () => await OpenNotifications());
        AboutCommand = new Command(async () => await OpenAbout());
        SignOutCommand = new Command(async () => await SignOut());
        DeleteAccountCommand = new Command(async () => await DeleteAccount());
        NavigateToTransactionsCommand = new Command(async () => await NavigateToTransactions());
        NavigateToDashboardCommand = new Command(async () => await NavigateToDashboard());
        NavigateToRequestLoanCommand = new Command(async () => await NavigateToRequestLoan());
        NavigateToCalculatorCommand = new Command(async () => await NavigateToCalculator());
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
                System.Diagnostics.Debug.WriteLine("No user email found");
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
        await Shell.Current.GoToAsync("..");
    }

    private async Task EditProfile()
    {
        await Shell.Current.DisplayAlertAsync("Edit Profile", "Edit profile functionality will be implemented", "OK");
        // TODO: Navigate to edit profile page
    }

    private async Task ChangePassword()
    {
        await Shell.Current.DisplayAlertAsync("Change Password", "Change password functionality will be implemented", "OK");
        // TODO: Implement change password dialog
    }

    private async Task OpenNotifications()
    {
        await Shell.Current.DisplayAlertAsync("Notifications", "Notification settings will be implemented", "OK");
        // TODO: Navigate to notifications settings
    }

    private async Task OpenAbout()
    {
        await Shell.Current.DisplayAlertAsync("About Lender", "Lender v1.0.0\n\nA professional peer-to-peer lending management app.\n\nSupport: support@lenderapp.com", "OK");
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
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Delete Account",
            "Are you sure you want to delete your account? This action cannot be undone.",
            "Delete",
            "Cancel");

        if (confirm)
        {
            bool finalConfirm = await Shell.Current.DisplayAlertAsync(
                "Final Confirmation",
                "This will permanently delete all your data including loans, transactions, and profile information.",
                "Yes, Delete",
                "Cancel");

            if (finalConfirm)
            {
                // TODO: Implement account deletion
                await Shell.Current.DisplayAlertAsync("Account Deletion", "Account deletion will be implemented", "OK");
            }
        }
    }

    private async Task NavigateToTransactions()
    {
        await Shell.Current.DisplayAlertAsync("Transactions", "Navigate to Transactions page", "OK");
        // TODO: await Shell.Current.GoToAsync("//transactions");
    }

    private async Task NavigateToDashboard()
    {
        await Shell.Current.GoToAsync("//mainpage");
    }

    private async Task NavigateToRequestLoan()
    {
        await Shell.Current.DisplayAlertAsync("Request/Send Loan", "Navigate to Request/Send Loan page", "OK");
        // TODO: await Shell.Current.GoToAsync("//requestloan");
    }

    private async Task NavigateToCalculator()
    {
        await Shell.Current.DisplayAlertAsync("Calculator", "Navigate to Loan Calculator page", "OK");
        // TODO: await Shell.Current.GoToAsync("//calculator");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
