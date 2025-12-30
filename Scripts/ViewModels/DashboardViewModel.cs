using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using Lender.Helpers;
using Lender.Models;
using Lender.Services;

namespace Lender.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private bool _isDemoMode = true;
    private string _modeLabel = "Demo mode";
    private string _userName = "Demo User";
    private string _userEmail = "demo@example.com";
    private string _phoneNumber = "";
    private string _dateOfBirthDisplay = "";
    private string _status = "Active";
    private decimal _creditScoreUser;
    private decimal _balanceUser;
    private decimal _totalLentAmount;
    private decimal _totalBorrowedAmount;
    private decimal _expectedReturn;
    private string _nextPaymentDisplay = string.Empty;
    private ObservableCollection<LoanRequest> _recentLoans = null!;
    private ObservableCollection<Transaction> _recentTransactions = null!;
    private readonly IAuthenticationService _authService;
    private readonly FirestoreService _firestoreService;

    public ICommand SignOutCommand { get; }
    public ICommand MenuCommand { get; }
    public ICommand ToggleDemoCommand { get; }
    public ICommand NavigateToTransactionsCommand { get; }
    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToRequestLoanCommand { get; }
    public ICommand NavigateToCalculatorCommand { get; }
    public ICommand NavigateToProfileCommand { get; }

    public DashboardViewModel()
    {
        _authService = ServiceHelper.GetService<IAuthenticationService>() ?? 
            throw new InvalidOperationException("AuthenticationService not registered");
        _firestoreService = FirestoreService.Instance;
        
        SignOutCommand = new Command(SignOutAsync);
        MenuCommand = new Command(OnMenuClicked);
        ToggleDemoCommand = new Command(EnableDemoMode);
        NavigateToTransactionsCommand = new Command(async () => await NavigateToTransactions());
        NavigateToDashboardCommand = new Command(async () => await NavigateToDashboard());
        NavigateToRequestLoanCommand = new Command(async () => await NavigateToRequestLoan());
        NavigateToCalculatorCommand = new Command(async () => await NavigateToCalculator());
        NavigateToProfileCommand = new Command(async () => await NavigateToProfile());
        InitializeData();
        _ = LoadUserDataAsync();
    }

    public bool IsDemoMode
    {
        get => _isDemoMode;
        set
        {
            if (_isDemoMode != value)
            {
                _isDemoMode = value;
                OnPropertyChanged();
            }
        }
    }

    public string ModeLabel
    {
        get => _modeLabel;
        set
        {
            if (_modeLabel != value)
            {
                _modeLabel = value;
                OnPropertyChanged();
            }
        }
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

    public string DateOfBirthDisplay
    {
        get => _dateOfBirthDisplay;
        set
        {
            if (_dateOfBirthDisplay != value)
            {
                _dateOfBirthDisplay = value;
                OnPropertyChanged();
            }
        }
    }

    public string Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal CreditScoreUser
    {
        get => _creditScoreUser;
        set
        {
            if (_creditScoreUser != value)
            {
                _creditScoreUser = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal BalanceUser
    {
        get => _balanceUser;
        set
        {
            if (_balanceUser != value)
            {
                _balanceUser = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal TotalLentAmount
    {
        get => _totalLentAmount;
        set
        {
            if (_totalLentAmount != value)
            {
                _totalLentAmount = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal TotalBorrowedAmount
    {
        get => _totalBorrowedAmount;
        set
        {
            if (_totalBorrowedAmount != value)
            {
                _totalBorrowedAmount = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal ExpectedReturn
    {
        get => _expectedReturn;
        set
        {
            if (_expectedReturn != value)
            {
                _expectedReturn = value;
                OnPropertyChanged();
            }
        }
    }

    public string NextPaymentDisplay
    {
        get => _nextPaymentDisplay;
        set
        {
            if (_nextPaymentDisplay != value)
            {
                _nextPaymentDisplay = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<LoanRequest> RecentLoans
    {
        get => _recentLoans;
        set
        {
            if (_recentLoans != value)
            {
                _recentLoans = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<Transaction> RecentTransactions
    {
        get => _recentTransactions;
        set
        {
            if (_recentTransactions != value)
            {
                _recentTransactions = value;
                OnPropertyChanged();
            }
        }
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

            UserName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
            UserEmail = user.Email;
            PhoneNumber = user.PhoneNumber;
            DateOfBirthDisplay = user.DateOfBirth == DateTime.MinValue ? "" : user.DateOfBirth.ToString("yyyy-MM-dd");
            Status = user.Status.ToString();
            CreditScoreUser = user.CreditScore;
            BalanceUser = user.Balance;

            IsDemoMode = false;
            ModeLabel = "Live data";
            
            // Clear demo data and load real data (from Firestore in future)
            RecentLoans = new ObservableCollection<LoanRequest>();
            RecentTransactions = new ObservableCollection<Transaction>();
            TotalLentAmount = 0;
            TotalBorrowedAmount = 0;
            ExpectedReturn = 0;
            NextPaymentDisplay = "No payments scheduled";
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
        UserName = "Demo User";
        UserEmail = "demo@example.com";
        PhoneNumber = "";
        DateOfBirthDisplay = "";
        Status = "Active";
        CreditScoreUser = 720;
        BalanceUser = 0;
        
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
        
        TotalLentAmount = RecentLoans.Where(l => l.Category == "Given").Sum(l => l.Amount);
        TotalBorrowedAmount = RecentLoans.Where(l => l.Category == "Received").Sum(l => l.Amount);
        ExpectedReturn = RecentLoans.Where(l => l.Category == "Given").Sum(l => l.Amount + (l.Amount * (l.InterestRate / 100m)));
        NextPaymentDisplay = "Jan 15, 2026 - $420 due";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void SignOutAsync()
    {
        try
        {
            await _authService.SignOutAsync();
            // Navigate back to login
            await Shell.Current.GoToAsync("//login", animate: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sign out error: {ex.Message}");
        }
    }

    private void OnMenuClicked()
    {
        System.Diagnostics.Debug.WriteLine("Menu button clicked - opening flyout");
        Shell.Current.FlyoutIsPresented = true;
    }

    private async Task NavigateToTransactions()
    {
        await Shell.Current.DisplayAlertAsync("Transactions", "Navigate to Transactions page", "OK");
        // TODO: Implement navigation when page is created
        // await Shell.Current.GoToAsync("//transactions");
    }

    private async Task NavigateToDashboard()
    {
        // Already on dashboard
        await Task.CompletedTask;
    }

    private async Task NavigateToRequestLoan()
    {
        await Shell.Current.DisplayAlertAsync("Request/Send Loan", "Navigate to Request/Send Loan page", "OK");
        // TODO: Implement navigation when page is created
        // await Shell.Current.GoToAsync("//requestloan");
    }

    private async Task NavigateToCalculator()
    {
        await Shell.Current.DisplayAlertAsync("Calculator", "Navigate to Loan Calculator page", "OK");
        // TODO: Implement navigation when page is created
        // await Shell.Current.GoToAsync("//calculator");
    }

    private async Task NavigateToProfile()
    {
        await Shell.Current.GoToAsync("//profile");
    }
}

