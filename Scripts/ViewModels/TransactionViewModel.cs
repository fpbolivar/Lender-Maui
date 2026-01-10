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
using Lender;

namespace Lender.ViewModels;

/// <summary>
/// Wrapper class for displaying transaction details in UI
/// </summary>
public class TransactionDisplayModel : INotifyPropertyChanged
{
    private readonly Transaction _transaction;
    private readonly string _currentUserId;

    // Precomputed other party email for fallbacks
    private readonly string _otherPartyEmail;

    public TransactionDisplayModel(Transaction transaction, string currentUserId)
    {
        _transaction = transaction;
        _currentUserId = currentUserId;

        // Determine the other party email once to reuse in bindings and fallbacks
        _otherPartyEmail = _transaction.FromUserId == _currentUserId
            ? _transaction.ToUserId
            : _transaction.FromUserId;
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

    // Determine other party name from transaction data
    public string OtherPartyName
    {
        get
        {
            // If current user is the requester, other party is petitioner (lender)
            if (_transaction.RequesterEmail == _currentUserId && !string.IsNullOrWhiteSpace(_transaction.PetitionerName))
                return _transaction.PetitionerName;
            
            // If current user is the petitioner, other party is requester (borrower)
            if (_transaction.PetitionerEmail == _currentUserId && !string.IsNullOrWhiteSpace(_transaction.RequesterName))
                return _transaction.RequesterName;
            
            // Fallback to email or "Unknown"
            return !string.IsNullOrWhiteSpace(_otherPartyEmail) ? _otherPartyEmail : "Unknown";
        }
    }

    public string OtherPartyEmail => _otherPartyEmail;

    public string Status => _transaction.Status.ToString();

    public Color AmountTextColor => _transaction.Type == TransactionType.Funding
        ? Color.FromArgb("#1ABC9C")  // Green for lending
        : Color.FromArgb("#FF9F43"); // Orange for borrowing

    public decimal InterestRate => _transaction.InterestRate;

    public string InterestRateDisplay => $"{InterestRate:F2}%";

    public bool HasCollateral => _transaction.HasCollateral;

    public string CollateralName => _transaction.CollateralDescription ?? string.Empty;

    public string? CollateralImageId => _transaction.CollateralImageId;

    public string? CollateralOwnerEmail => _transaction.PetitionerEmail ?? _transaction.FromUserId;

    // Payment details from transaction
    public decimal PeriodicPayment => _transaction.PeriodicPayment;
    public string PeriodicPaymentDisplay => PeriodicPayment > 0 ? $"${PeriodicPayment:F2}" : "N/A";
    
    public int TotalPayments => _transaction.TotalPayments;
    public string PaymentFrequencyLabel => _transaction.PaymentFrequencyLabel ?? "N/A";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Transaction UnderlyingTransaction => _transaction;
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

    public ICommand OpenTransactionCommand { get; }

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
        OpenTransactionCommand = new Command<TransactionDisplayModel>(async (tx) => await OpenTransactionAsync(tx));
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
                InterestAmount = 425m,
                InterestRate = 8.5m,
                PeriodicPayment = 450m,
                TotalPayments = 12,
                PaymentFrequencyLabel = "Monthly",
                RequesterName = "Carlos Ruiz",
                RequesterEmail = "carlos@example.com",
                PetitionerName = "Demo User",
                PetitionerEmail = "demo@example.com"
            },
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
                InterestAmount = 100m,
                InterestRate = 6.0m,
                PeriodicPayment = 210m,
                TotalPayments = 10,
                PaymentFrequencyLabel = "Monthly",
                RequesterName = "Demo User",
                RequesterEmail = "demo@example.com",
                PetitionerName = "Sarah Johnson",
                PetitionerEmail = "sarah@example.com"
            },
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
                InterestAmount = 35m,
                InterestRate = 8.5m,
                PeriodicPayment = 35m,
                TotalPayments = 12,
                PaymentFrequencyLabel = "Monthly",
                RequesterName = "Demo User",
                RequesterEmail = "demo@example.com",
                PetitionerName = "Carlos Martinez",
                PetitionerEmail = "carlos@example.com"
            },
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

            // Load canonical loan agreement transactions for the current user
            var transactions = await _firestoreService.GetUserTransactionsAsync(currentUserEmail);

            _allTransactions.Clear();

            // Process each transaction - no need to fetch user data, it's denormalized in transaction
            foreach (var transaction in transactions)
            {
                var displayModel = new TransactionDisplayModel(transaction, currentUserEmail);
                _allTransactions.Add(displayModel);
            }

            // Apply current filter
            ApplyCurrentFilter();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading transactions: {ex.Message} - TransactionViewModel.cs:342");
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

    private async Task OpenTransactionAsync(TransactionDisplayModel? tx)
    {
        if (tx == null) return;

        try
        {
            await Shell.Current.Navigation.PushAsync(new TransactionDetailPage(tx));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TransactionViewModel] Navigation to detail failed: {ex.Message} - TransactionViewModel.cs:401");
        }
    }
}
