using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using System.Threading.Tasks;
using Lender.Helpers;
using Lender.Models;
using Lender.Services;

namespace Lender.ViewModels;

/// <summary>
/// Wrapper class for displaying transaction details in UI
/// </summary>
public class TransactionDisplayModel : INotifyPropertyChanged
{
    private readonly Transaction _transaction;
    private readonly LoanRequest? _loanRequest;
    private readonly User? _otherUser;
    private readonly string _currentUserId;

    public TransactionDisplayModel(Transaction transaction, LoanRequest? loanRequest, User? otherUser, string currentUserId)
    {
        _transaction = transaction;
        _loanRequest = loanRequest;
        _otherUser = otherUser;
        _currentUserId = currentUserId;
    }

    public string TransactionTypeDisplay => _transaction.Type switch
    {
        TransactionType.Funding => "Lend",
        TransactionType.Repayment => "Borrow",
        TransactionType.Interest => "Interest Payment",
        TransactionType.Transfer => "Transfer",
        TransactionType.Withdrawal => "Withdrawal",
        TransactionType.Deposit => "Deposit",
        _ => _transaction.Type.ToString()
    };

    public decimal Amount => _transaction.Amount;

    public string OtherPartyName => _otherUser?.FullName ?? "Unknown";

    public string Status => _transaction.Status.ToString();

    public Color AmountTextColor => _transaction.Type == TransactionType.Funding
        ? Color.FromArgb("#1ABC9C")  // Green for lending
        : Color.FromArgb("#FF9F43"); // Orange for borrowing

    public decimal InterestRate => _loanRequest?.InterestRate ?? 0;

    public string InterestRateDisplay => $"{InterestRate:F2}%";

    public bool HasCollateral => !string.IsNullOrEmpty(_loanRequest?.Category);

    public string CollateralName => _loanRequest?.Category ?? string.Empty;

    public bool HasNextPayment => _loanRequest?.DueDate > DateTime.UtcNow;

    public string NextPaymentDateDisplay => _loanRequest?.DueDate.ToString("MMM dd, yyyy") ?? "N/A";

    // Calculate next payment amount based on loan terms
    public decimal NextPaymentAmount => CalculatePaymentAmount();

    public string NextPaymentAmountDisplay => NextPaymentAmount > 0 ? $"${NextPaymentAmount:F2}" : "N/A";

    private decimal CalculatePaymentAmount()
    {
        if (_loanRequest == null) return 0;

        // Simple calculation: (Principal + (Principal * Rate / 100)) / Duration
        decimal totalWithInterest = _loanRequest.Amount * (1 + _loanRequest.InterestRate / 100);
        return totalWithInterest / _loanRequest.DurationMonths;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TransactionViewModel : NavigableViewModel
{
    private ObservableCollection<TransactionDisplayModel> _allTransactions = new();
    private ObservableCollection<TransactionDisplayModel> _filteredTransactions = new();
    private bool _isAllSelected = true;
    private bool _isLendsSelected = false;
    private bool _isBorrowsSelected = false;
    private readonly IAuthenticationService _authService;
    private readonly FirestoreService _firestoreService;

    public ICommand FilterByAllCommand { get; }
    public ICommand FilterByLendsCommand { get; }
    public ICommand FilterByBorrowsCommand { get; }

    public TransactionViewModel()
    {
        _authService = ServiceHelper.GetService<IAuthenticationService>() ??
            throw new InvalidOperationException("AuthenticationService not registered");
        _firestoreService = FirestoreService.Instance;

        FilterByAllCommand = new Command(() => FilterByAll());
        FilterByLendsCommand = new Command(() => FilterByLends());
        FilterByBorrowsCommand = new Command(() => FilterByBorrows());
    }

    public ObservableCollection<TransactionDisplayModel> FilteredTransactions
    {
        get => _filteredTransactions;
        set
        {
            if (_filteredTransactions != value)
            {
                _filteredTransactions = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNoTransactions));
            }
        }
    }

    public bool IsAllSelected
    {
        get => _isAllSelected;
        set
        {
            if (_isAllSelected != value)
            {
                _isAllSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsLendsSelected
    {
        get => _isLendsSelected;
        set
        {
            if (_isLendsSelected != value)
            {
                _isLendsSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsBorrowsSelected
    {
        get => _isBorrowsSelected;
        set
        {
            if (_isBorrowsSelected != value)
            {
                _isBorrowsSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public bool HasNoTransactions => FilteredTransactions.Count == 0;

    public async void OnAppearingAsync()
    {
        // Check if in demo mode
        var currentUserEmail = _authService.CurrentUserEmail;
        if (string.IsNullOrEmpty(currentUserEmail))
        {
            LoadDemoData();
            return;
        }

        await LoadTransactionsAsync();
    }

    private void LoadDemoData()
    {
        _allTransactions.Clear();

        // Demo loan for lending
        var demoLoan1 = new LoanRequest
        {
            Id = "L-DEMO-001",
            Amount = 5000,
            InterestRate = 8.5m,
            DurationMonths = 12,
            DueDate = DateTime.UtcNow.AddMonths(12),
            Category = "Personal"
        };

        // Demo loan for borrowing
        var demoLoan2 = new LoanRequest
        {
            Id = "L-DEMO-002",
            Amount = 2000,
            InterestRate = 6.0m,
            DurationMonths = 10,
            DueDate = DateTime.UtcNow.AddMonths(10),
            Category = "Equipment"
        };

        // Demo user (other party)
        var demoUser1 = new User
        {
            FullName = "Carlos Ruiz",
            Email = "carlos@example.com"
        };

        var demoUser2 = new User
        {
            FullName = "Sarah Johnson",
            Email = "sarah@example.com"
        };

        // Demo transactions
        var transaction1 = new TransactionDisplayModel(
            new Transaction
            {
                Id = "T-DEMO-001",
                Amount = 5000m,
                Type = TransactionType.Funding,
                Status = TransactionStatus.Completed,
                FromUserId = "demo@example.com",
                ToUserId = "carlos@example.com",
                LoanRequestId = "L-DEMO-001",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                InterestAmount = 425m
            },
            demoLoan1,
            demoUser1,
            "demo@example.com"
        );

        var transaction2 = new TransactionDisplayModel(
            new Transaction
            {
                Id = "T-DEMO-002",
                Amount = 2000m,
                Type = TransactionType.Repayment,
                Status = TransactionStatus.Completed,
                FromUserId = "sarah@example.com",
                ToUserId = "demo@example.com",
                LoanRequestId = "L-DEMO-002",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                InterestAmount = 100m
            },
            demoLoan2,
            demoUser2,
            "demo@example.com"
        );

        var transaction3 = new TransactionDisplayModel(
            new Transaction
            {
                Id = "T-DEMO-003",
                Amount = 420m,
                Type = TransactionType.Funding,
                Status = TransactionStatus.Pending,
                FromUserId = "demo@example.com",
                ToUserId = "carlos@example.com",
                LoanRequestId = "L-DEMO-001",
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                InterestAmount = 35m
            },
            demoLoan1,
            demoUser1,
            "demo@example.com"
        );

        _allTransactions.Add(transaction1);
        _allTransactions.Add(transaction2);
        _allTransactions.Add(transaction3);

        // Apply current filter
        ApplyCurrentFilter();
    }

    private async Task LoadTransactionsAsync()
    {
        try
        {
            var currentUserEmail = _authService.CurrentUserEmail;
            if (string.IsNullOrEmpty(currentUserEmail)) return;

            // Load all transactions for the current user
            var transactions = await _firestoreService.GetUserTransactionsAsync(currentUserEmail);

            _allTransactions.Clear();

            // Process each transaction with related loan and user data
            foreach (var transaction in transactions)
            {
                var loanRequest = await _firestoreService.GetLoanAsync(transaction.LoanRequestId);
                
                // Determine the other party (for/from whom)
                var otherUserId = transaction.FromUserId == currentUserEmail ? transaction.ToUserId : transaction.FromUserId;
                var otherUser = await _firestoreService.GetUserAsync(otherUserId);

                var displayModel = new TransactionDisplayModel(transaction, loanRequest, otherUser, currentUserEmail);
                _allTransactions.Add(displayModel);
            }

            // Apply current filter
            ApplyCurrentFilter();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading transactions: {ex.Message}");
        }
    }

    private void FilterByAll()
    {
        IsAllSelected = true;
        IsLendsSelected = false;
        IsBorrowsSelected = false;

        FilteredTransactions = new ObservableCollection<TransactionDisplayModel>(_allTransactions);
    }

    private void FilterByLends()
    {
        IsAllSelected = false;
        IsLendsSelected = true;
        IsBorrowsSelected = false;

        var filtered = _allTransactions
            .Where(t => t.TransactionTypeDisplay == "Lend")
            .ToList();

        FilteredTransactions = new ObservableCollection<TransactionDisplayModel>(filtered);
    }

    private void FilterByBorrows()
    {
        IsAllSelected = false;
        IsLendsSelected = false;
        IsBorrowsSelected = true;

        var filtered = _allTransactions
            .Where(t => t.TransactionTypeDisplay == "Borrow")
            .ToList();

        FilteredTransactions = new ObservableCollection<TransactionDisplayModel>(filtered);
    }

    private void ApplyCurrentFilter()
    {
        if (IsLendsSelected)
            FilterByLends();
        else if (IsBorrowsSelected)
            FilterByBorrows();
        else
            FilterByAll();
    }
}
