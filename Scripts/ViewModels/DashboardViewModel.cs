using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using Lender.Helpers;
using Lender.Models;
using Lender.Services;

namespace Lender.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private bool _isDemoMode = false;
    private string _modeLabel = "Live data";
    private UserProfile _userProfile = UserProfile.CreateDemo();
    private LoanStatistics _loanStatistics = LoanStatistics.CreateDemo();
    private ObservableCollection<LoanRequest> _recentLoans = null!;
    private ObservableCollection<Transaction> _recentTransactions = null!;
    private readonly IAuthenticationService _authService;
    private readonly FirestoreService _firestoreService;

    public ICommand SignOutCommand { get; }
    public ICommand SignInCommand { get; }
    public ICommand ToggleDemoCommand { get; }
    public ICommand NavigateToTransactionsCommand { get; }
    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToLoansCommand { get; }
    public ICommand NavigateToCalculatorCommand { get; }
    public ICommand NavigateToProfileCommand { get; }

    public DashboardViewModel()
    {
        _authService = ServiceHelper.GetService<IAuthenticationService>() ?? 
            throw new InvalidOperationException("AuthenticationService not registered");
        _firestoreService = FirestoreService.Instance;
        
        SignOutCommand = new Command(SignOutAsync);
        SignInCommand = new Command(async () => await Shell.Current.GoToAsync("//login", animate: false));
        ToggleDemoCommand = new Command(EnableDemoMode);
        NavigateToTransactionsCommand = new Command(async () => await NavigateToTransactions());
        NavigateToDashboardCommand = new Command(async () => await NavigateToDashboard());
        NavigateToLoansCommand = new Command(async () => { await Shell.Current.GoToAsync("loanform"); });
        NavigateToCalculatorCommand = new Command(async () => await NavigateToCalculator());
        NavigateToProfileCommand = new Command(async () => await NavigateToProfile());
        InitializeData();
        _ = LoadUserDataAsync();
    }

    public bool IsDemoMode
    {
        get => _isDemoMode;
        set => SetProperty(ref _isDemoMode, value);
    }

    public string ModeLabel
    {
        get => _modeLabel;
        set => SetProperty(ref _modeLabel, value);
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
                OnPropertyChanged();
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

    public string DateOfBirthDisplay
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

    public string Status
    {
        get => _userProfile.Status;
        set
        {
            if (_userProfile.Status != value)
            {
                _userProfile.Status = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal BalanceUser
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
    public decimal TotalLentAmount
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

    public decimal TotalBorrowedAmount
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

    public decimal ExpectedReturn
    {
        get => _loanStatistics.ExpectedReturn;
        set
        {
            if (_loanStatistics.ExpectedReturn != value)
            {
                _loanStatistics.ExpectedReturn = value;
                OnPropertyChanged();
            }
        }
    }

    public string NextPaymentDisplay
    {
        get => _loanStatistics.NextPaymentDisplay;
        set
        {
            if (_loanStatistics.NextPaymentDisplay != value)
            {
                _loanStatistics.NextPaymentDisplay = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<LoanRequest> RecentLoans
    {
        get => _recentLoans;
        set => SetProperty(ref _recentLoans, value);
    }

    public ObservableCollection<Transaction> RecentTransactions
    {
        get => _recentTransactions;
        set => SetProperty(ref _recentTransactions, value);
    }

    private void InitializeData()
    {
        // Initialize with empty collections
        RecentTransactions = new ObservableCollection<Transaction>();
        RecentLoans = new ObservableCollection<LoanRequest>();
        
        EnableDemoMode();
    }

    private async Task LoadUserDataAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_authService.CurrentUserEmail))
            {
                EnableDemoMode();
                return;
            }

            var user = await _firestoreService.GetUserAsync(_authService.CurrentUserEmail);
            if (user == null)
            {
                EnableDemoMode();
                return;
            }

            // Update user profile from User model
            _userProfile = UserProfile.FromUser(user);
            
            // Notify all properties that depend on UserProfile
            OnPropertyChanged(nameof(UserName));
            OnPropertyChanged(nameof(UserEmail));
            OnPropertyChanged(nameof(PhoneNumber));
            OnPropertyChanged(nameof(DateOfBirthDisplay));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(BalanceUser));

            IsDemoMode = false;
            ModeLabel = "Live data";
            
            // Clear demo data and load real data (from Firestore in future)
            RecentLoans = new ObservableCollection<LoanRequest>();
            RecentTransactions = new ObservableCollection<Transaction>();
            
            // Reset loan statistics
            _loanStatistics = LoanStatistics.CreateEmpty();
            OnPropertyChanged(nameof(TotalLentAmount));
            OnPropertyChanged(nameof(TotalBorrowedAmount));
            OnPropertyChanged(nameof(ExpectedReturn));
            OnPropertyChanged(nameof(NextPaymentDisplay));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] LoadUserDataAsync error: {ex.Message}");
            EnableDemoMode();
        }
    }

    public Task RefreshAsync()
    {
        return LoadUserDataAsync();
    }

    private void EnableDemoMode()
    {
        IsDemoMode = true;
        ModeLabel = "Demo mode";
        
        // Use pre-built demo profile and statistics
        _userProfile = UserProfile.CreateDemo();
        _loanStatistics = LoanStatistics.CreateDemo();
        
        // Notify all properties
        OnPropertyChanged(nameof(UserName));
        OnPropertyChanged(nameof(UserEmail));
        OnPropertyChanged(nameof(PhoneNumber));
        OnPropertyChanged(nameof(DateOfBirthDisplay));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(BalanceUser));
        OnPropertyChanged(nameof(TotalLentAmount));
        OnPropertyChanged(nameof(TotalBorrowedAmount));
        OnPropertyChanged(nameof(ExpectedReturn));
        OnPropertyChanged(nameof(NextPaymentDisplay));
        
        // Demo loan data
        RecentLoans = new ObservableCollection<LoanRequest>
        {
            new LoanRequest
            {
                Id = "L-DEMO-001",
                UserName = "Carlos Ruiz",
                Amount = 5000,
                Description = "Personal loan (you lent)",
                Status = LoanStatus.Active,
                FundedPercentage = 100,
                AmountFunded = 5000,
                Category = "Given",
                InterestRate = 8.5m,
                DurationMonths = 12
            },
            new LoanRequest
            {
                Id = "L-DEMO-002",
                UserName = "You",
                Amount = 2000,
                Description = "Borrowed for laptop",
                Status = LoanStatus.Active,
                FundedPercentage = 100,
                AmountFunded = 2000,
                Category = "Received",
                InterestRate = 6.0m,
                DurationMonths = 10
            }
        };
        
        RecentTransactions = new ObservableCollection<Transaction>
        {
            new Transaction
            {
                Id = "T-DEMO-001",
                Amount = 500m,
                Merchant = "Payment from Carlos",
                Category = "Loan Payment",
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                CreatedDate = DateTime.UtcNow.AddDays(-5)
            }
        };
    }

    private async void SignOutAsync()
    {
        try
        {
            await _authService.SignOutAsync();
            EnableDemoMode();
            // Navigate back to login
            await Shell.Current.GoToAsync("//login", animate: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sign out error: {ex.Message}");
        }
    }

    private async Task NavigateToTransactions()
    {
        if (IsDemoMode)
        {
            await Shell.Current.DisplayAlertAsync("Demo mode", "Navigation is disabled in demo mode. Sign in to access this section.", "OK");
            return;
        }
        await Shell.Current.GoToAsync("//transactions");
    }

    private async Task NavigateToDashboard()
    {
        await NavBarNavigation.GoToDashboardAsync(IsDemoMode);
    }


    private async Task NavigateToCalculator()
    {
        await NavBarNavigation.GoToCalculatorAsync(IsDemoMode);
    }

    private async Task NavigateToProfile()
    {
        await NavBarNavigation.GoToProfileAsync(IsDemoMode);
    }
}
