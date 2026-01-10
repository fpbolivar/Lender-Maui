using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Lender.Helpers;

namespace Lender.ViewModels;

public class LoanFormViewModel : NavigableViewModel
{
    public ICommand BackCommand { get; }
    public ICommand UploadCollateralImageCommand { get; }
    public ICommand SubmitCommand { get; }
    public ICommand SetRequestModeCommand { get; }
    public ICommand SetSendModeCommand { get; }
    public ICommand SetInterestTypeCommand { get; }
    public ICommand SelectPaymentFrequencyCommand { get; }

    private string _amountText = "";
    public string AmountText 
    { 
        get => _amountText; 
        set 
        {
            if (value == _amountText) return;
            
            // Only allow numbers and one decimal point
            var filtered = string.Empty;
            var hasDecimal = false;
            
            foreach (var c in value ?? string.Empty)
            {
                if (char.IsDigit(c))
                {
                    filtered += c;
                }
                else if (c == '.' && !hasDecimal)
                {
                    filtered += c;
                    hasDecimal = true;
                }
            }
            
            SetProperty(ref _amountText, filtered);
        }
    }

    private string _interestRateText = "0";
    public string InterestRateText { get => _interestRateText; set => SetProperty(ref _interestRateText, value ?? "0"); }

    // Mode: Request or Send
    private string _mode = "Request";
    public string Mode { get => _mode; private set => SetProperty(ref _mode, value); }

    private bool _isRequestSelected = true;
    public bool IsRequestSelected { get => _isRequestSelected; set => SetProperty(ref _isRequestSelected, value); }

    private bool _isSendSelected = false;
    public bool IsSendSelected { get => _isSendSelected; set => SetProperty(ref _isSendSelected, value); }

    // Simple form fields
    private DateTime _paybackDate = DateTime.Today.AddDays(7);
    public DateTime PaybackDate { get => _paybackDate; set => SetProperty(ref _paybackDate, value); }

    // Interest type selection
    public enum InterestTypeOption
    {
        None,
        Simple,
        Compound
    }

    private InterestTypeOption _interestType = InterestTypeOption.None;
    public InterestTypeOption InterestType { get => _interestType; set => SetProperty(ref _interestType, value); }

private bool _isNoInterestSelected = false;
    public bool IsNoInterestSelected { get => _isNoInterestSelected; set => SetProperty(ref _isNoInterestSelected, value); }

    private bool _isSimpleInterestSelected = false;
    public bool IsSimpleInterestSelected { get => _isSimpleInterestSelected; set => SetProperty(ref _isSimpleInterestSelected, value); }

    private bool _isCompoundInterestSelected = false;
    public bool IsCompoundInterestSelected { get => _isCompoundInterestSelected; set => SetProperty(ref _isCompoundInterestSelected, value); }

    // Interest details shown only when InterestType != None
    private decimal _interestRate = 0m;
    public decimal InterestRate { get => _interestRate; set => SetProperty(ref _interestRate, value); }

    // Payment frequency
    public IReadOnlyList<PaymentFrequencyOption> PaymentFrequencies => PaymentFrequencyOption.All;
    private PaymentFrequencyOption _selectedPaymentFrequency = PaymentFrequencyOption.Default;
    public PaymentFrequencyOption SelectedPaymentFrequency { get => _selectedPaymentFrequency; set => SetProperty(ref _selectedPaymentFrequency, value); }

    private string _selectedPaymentFrequencyLabel = PaymentFrequencyOption.Default.Label;
    public string SelectedPaymentFrequencyLabel { get => _selectedPaymentFrequencyLabel; set => SetProperty(ref _selectedPaymentFrequencyLabel, value); }

    // Payment frequency selection flags
    private bool _isMonthlySelected = true;
    public bool IsMonthlySelected { get => _isMonthlySelected; set => SetProperty(ref _isMonthlySelected, value); }
    
    private bool _isDailySelected = false;
    public bool IsDailySelected { get => _isDailySelected; set => SetProperty(ref _isDailySelected, value); }
    
    private bool _isWeeklySelected = false;
    public bool IsWeeklySelected { get => _isWeeklySelected; set => SetProperty(ref _isWeeklySelected, value); }
    
    private bool _isBiweekSelected = false;
    public bool IsBiweekSelected { get => _isBiweekSelected; set => SetProperty(ref _isBiweekSelected, value); }
    
    private bool _isHalfMonthSelected = false;
    public bool IsHalfMonthSelected { get => _isHalfMonthSelected; set => SetProperty(ref _isHalfMonthSelected, value); }

    // Helper method to check if a payment frequency is selected
    public bool IsPaymentFrequencySelected(string label) => label == SelectedPaymentFrequencyLabel;

    // Section visibility state
    private bool _isSimpleFormVisible = true;
    public bool IsSimpleFormVisible { get => _isSimpleFormVisible; set => SetProperty(ref _isSimpleFormVisible, value); }

    private bool _isInterestTypeVisible = true;
    public bool IsInterestTypeVisible { get => _isInterestTypeVisible; set => SetProperty(ref _isInterestTypeVisible, value); }

    private bool _isInterestDetailsVisible = false;
    public bool IsInterestDetailsVisible { get => _isInterestDetailsVisible; set => SetProperty(ref _isInterestDetailsVisible, value); }

    private bool _isPaymentFrequencyVisible = false;
    public bool IsPaymentFrequencyVisible { get => _isPaymentFrequencyVisible; set => SetProperty(ref _isPaymentFrequencyVisible, value); }

    // Computed outputs
    private decimal _periodicPayment = 0;
    public decimal PeriodicPayment { get => _periodicPayment; private set => SetProperty(ref _periodicPayment, value); }

    private decimal _totalInterest = 0;
    public decimal TotalInterest { get => _totalInterest; private set => SetProperty(ref _totalInterest, value); }

    private decimal _totalPayment = 0;
    public decimal TotalPayment { get => _totalPayment; private set => SetProperty(ref _totalPayment, value); }

    private int _totalPayments = 0;
    public int TotalPayments { get => _totalPayments; private set => SetProperty(ref _totalPayments, value); }

    public LoanFormViewModel()
    {
        Debug.WriteLine("LoanFormViewModel constructor started - LoanFormViewModel.cs:79");
        BackCommand = new Command(async () => 
        {
            Debug.WriteLine("BackCommand executing - LoanFormViewModel.cs:82");
            await Shell.Current.GoToAsync("..");
        });
        UploadCollateralImageCommand = new Command(async () => await Task.CompletedTask);
        SubmitCommand = new Command(async () => await Task.CompletedTask, () => true);
        SetRequestModeCommand = new Command(() => {
            Mode = "Request";
            IsRequestSelected = true;
            IsSendSelected = false;
            IsSimpleFormVisible = true;
            IsInterestTypeVisible = true;
            IsInterestDetailsVisible = InterestType != InterestTypeOption.None;
            IsPaymentFrequencyVisible = InterestType != InterestTypeOption.None;
        });
        SetSendModeCommand = new Command(() => {
            Mode = "Send";
            IsRequestSelected = false;
            IsSendSelected = true;
            IsSimpleFormVisible = true;
            IsInterestTypeVisible = true;
            IsInterestDetailsVisible = InterestType != InterestTypeOption.None;
            IsPaymentFrequencyVisible = InterestType != InterestTypeOption.None;
        });

        SetInterestTypeCommand = new Command<object?>(param =>
        {
            var val = (param as string) ?? string.Empty;
            InterestType = val switch
            {
                "None" => InterestTypeOption.None,
                "Simple" => InterestTypeOption.Simple,
                "Compound" => InterestTypeOption.Compound,
                _ => InterestTypeOption.None
            };

            // Update selection flags
            IsNoInterestSelected = InterestType == InterestTypeOption.None;
            IsSimpleInterestSelected = InterestType == InterestTypeOption.Simple;
            IsCompoundInterestSelected = InterestType == InterestTypeOption.Compound;

            // Toggle interest details visibility based on type
            IsInterestDetailsVisible = InterestType != InterestTypeOption.None;
            IsPaymentFrequencyVisible = true;
        });

        SelectPaymentFrequencyCommand = new Command<object?>(param =>
        {
            if (param is PaymentFrequencyOption p)
            {
                SelectedPaymentFrequency = p;
                SelectedPaymentFrequencyLabel = p.Label;
                
                // Update selection flags
                IsDailySelected = p.Label == "Daily";
                IsWeeklySelected = p.Label == "Weekly";
                IsBiweekSelected = p.Label == "Bi-week";
                IsHalfMonthSelected = p.Label == "Half Month";
                IsMonthlySelected = p.Label == "Monthly";
            }
        });
        
        // Initialize selection flags
        IsRequestSelected = true;
        
        Debug.WriteLine("LoanFormViewModel constructor completed - LoanFormViewModel.cs:126");
    }
}