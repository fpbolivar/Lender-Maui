using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lender.Helpers;
using Lender.Models;
using Lender.Services;

namespace Lender.ViewModels;

public class DashboardViewModel : INotifyPropertyChanged
{
    private decimal _totalBudget;
    private decimal _totalSpent;
    private decimal _totalRemaining;
    private ObservableCollection<LoanRequest> _recentLoans = null!;
    private ObservableCollection<Transaction> _recentTransactions = null!;
    private ObservableCollection<Budget> _budgets = null!;
    private readonly IAuthenticationService _authService;

    public ICommand SignOutCommand { get; }
    public ICommand MenuCommand { get; }

    public DashboardViewModel()
    {
        _authService = ServiceHelper.GetService<IAuthenticationService>() ?? 
            throw new InvalidOperationException("AuthenticationService not registered");
        
        SignOutCommand = new Command(SignOutAsync);
        MenuCommand = new Command(OnMenuClicked);
        InitializeData();
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

