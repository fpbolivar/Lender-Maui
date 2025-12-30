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
    private decimal _totalBudget;
    private decimal _totalSpent;
    private decimal _totalRemaining;
    private ObservableCollection<LoanRequest> _recentLoans = null!;
    private ObservableCollection<Transaction> _recentTransactions = null!;
    private ObservableCollection<Budget> _budgets = null!;
    private readonly IAuthenticationService _authService;
    private readonly FirestoreService _firestoreService;

    public ICommand SignOutCommand { get; }
    public ICommand MenuCommand { get; }
    public ICommand ToggleDemoCommand { get; }

    public DashboardViewModel()
    {
        _authService = ServiceHelper.GetService<IAuthenticationService>() ?? 
            throw new InvalidOperationException("AuthenticationService not registered");
        _firestoreService = FirestoreService.Instance;
        
        SignOutCommand = new Command(SignOutAsync);
        MenuCommand = new Command(OnMenuClicked);
        ToggleDemoCommand = new Command(EnableDemoMode);
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

    public decimal TotalBudget
    {
        get => _totalBudget;
        set
        {
            if (_totalBudget != value)
            {
                _totalBudget = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal TotalSpent
    {
        get => _totalSpent;
        set
        {
            if (_totalSpent != value)
            {
                _totalSpent = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal TotalRemaining
    {
        get => _totalRemaining;
        set
        {
            if (_totalRemaining != value)
            {
                _totalRemaining = value;
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

    public ObservableCollection<Budget> Budgets
    {
        get => _budgets;
        set
        {
            if (_budgets != value)
            {
                _budgets = value;
                OnPropertyChanged();
            }
        }
    }

    private void InitializeData()
    {
        // Initialize with demo data
        EnableDemoMode();

        RecentTransactions = new ObservableCollection<Transaction>
        {
            new Transaction
            {
                Id = "1",
                Amount = 27.99m,
                Merchant = "Amc Theatres",
                Category = "Entertainment",
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            },
            new Transaction
            {
                Id = "2",
                Amount = 10.00m,
                Merchant = "Petco",
                Category = "Pet",
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            },
            new Transaction
            {
                Id = "3",
                Amount = 27.99m,
                Merchant = "Amc Theatres",
                Category = "Entertainment",
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            },
            new Transaction
            {
                Id = "4",
                Amount = 20.00m,
                Merchant = "Starbucks",
                Category = "Restaurants",
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Completed,
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            }
        };

        Budgets = new ObservableCollection<Budget>
        {
            new Budget
            {
                Category = "Gifts",
                BudgetLimit = 117,
                AmountSpent = 234,
                IconEmoji = "🎁",
                ColorHex = "#FF4757"
            },
            new Budget
            {
                Category = "Entertainment",
                BudgetLimit = 95.76m,
                AmountSpent = 190.76m,
                IconEmoji = "🎬",
                ColorHex = "#FF4757"
            },
            new Budget
            {
                Category = "Shopping",
                BudgetLimit = 43.39m,
                AmountSpent = 86.39m,
                IconEmoji = "🛍️",
                ColorHex = "#FF4757"
            },
            new Budget
            {
                Category = "Food",
                BudgetLimit = 100,
                AmountSpent = 86.37m,
                IconEmoji = "🥑",
                ColorHex = "#FFAA33"
            },
            new Budget
            {
                Category = "Restaurants",
                BudgetLimit = 77.17m,
                AmountSpent = 54.83m,
                IconEmoji = "🍔",
                ColorHex = "#FFBB33"
            }
        };

        RecentLoans = new ObservableCollection<LoanRequest>
        {
            new LoanRequest
            {
                Id = "1",
                UserName = "John Doe",
                Amount = 5000,
                Description = "Car Loan",
                Status = LoanStatus.Active,
                FundedPercentage = 100,
                AmountFunded = 5000,
                Category = "Auto",
                InterestRate = 5.5m,
                DurationMonths = 36
            },
            new LoanRequest
            {
                Id = "2",
                UserName = "Jane Smith",
                Amount = 3000,
                Description = "Home Improvement",
                Status = LoanStatus.Funded,
                FundedPercentage = 75,
                AmountFunded = 2250,
                Category = "Home",
                InterestRate = 4.2m,
                DurationMonths = 24
            }
        };

        // Calculate totals
        TotalBudget = Budgets.Sum(b => b.BudgetLimit);
        TotalSpent = Budgets.Sum(b => b.AmountSpent);
        TotalRemaining = TotalBudget - TotalSpent;
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
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] LoadUserDataAsync error: {ex.Message}");
            EnableDemoMode();
        }
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
}

