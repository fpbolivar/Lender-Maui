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
    public ICommand SetByDateCommand { get; }
    public ICommand SetByTimeCommand { get; }
    public ICommand SetDaysCommand { get; }
    public ICommand SetMonthsCommand { get; }
    public ICommand SetYesCollateralCommand { get; }
    public ICommand SetNoCollateralCommand { get; }
    public ICommand SetInterestMethodTotalCommand { get; }
    public ICommand SetInterestMethodAPRCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand ContinueToRequesterInfoCommand { get; }
    public ICommand SetNotificationTypeCommand { get; }
    public ICommand ContinueToFinalSummaryCommand { get; }
    public ICommand SubmitLoanRequestCommand { get; }

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
            // Update validation when amount changes
            OnPropertyChanged(nameof(IsLoanAmountValid));
        }
    }

    public bool IsLoanAmountValid
    {
        get
        {
            return decimal.TryParse(AmountText, out decimal amount) && amount > 0;
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
    private DateTime _paybackDate = DateTime.Today.AddDays(1);
    public DateTime PaybackDate { get => _paybackDate; set => SetProperty(ref _paybackDate, value); }

    public DateTime MinimumPaybackDate => DateTime.Today.AddDays(1);
    public DateTime MaximumPaybackDate => DateTime.Today.AddYears(10);

    // Payback type: By Date or By Time
    private bool _isByDateSelected = true;
    public bool IsByDateSelected { get => _isByDateSelected; set => SetProperty(ref _isByDateSelected, value); }

    private bool _isByTimeSelected = false;
    public bool IsByTimeSelected { get => _isByTimeSelected; set => SetProperty(ref _isByTimeSelected, value); }

    private bool _showDatePicker = true;
    public bool ShowDatePicker { get => _showDatePicker; set => SetProperty(ref _showDatePicker, value); }

    private bool _showTimeInput = false;
    public bool ShowTimeInput { get => _showTimeInput; set => SetProperty(ref _showTimeInput, value); }

    // Time duration
    private bool _isDaysSelected = true;
    public bool IsDaysSelected { get => _isDaysSelected; set => SetProperty(ref _isDaysSelected, value); }

    private bool _isMonthsSelected = false;
    public bool IsMonthsSelected { get => _isMonthsSelected; set => SetProperty(ref _isMonthsSelected, value); }

    private string _timeDuration = "";
    public string TimeDuration { get => _timeDuration; set => SetProperty(ref _timeDuration, value); }

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

    // Interest calculation method: Total or APR
    private bool _isInterestMethodTotalSelected = true;
    public bool IsInterestMethodTotalSelected { get => _isInterestMethodTotalSelected; set => SetProperty(ref _isInterestMethodTotalSelected, value); }

    private bool _isInterestMethodAPRSelected = false;
    public bool IsInterestMethodAPRSelected { get => _isInterestMethodAPRSelected; set => SetProperty(ref _isInterestMethodAPRSelected, value); }

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
    private bool _isOneTimeSelected = false;
    public bool IsOneTimeSelected { get => _isOneTimeSelected; set => SetProperty(ref _isOneTimeSelected, value); }
    
    private bool _isTwoPaymentsSelected = false;
    public bool IsTwoPaymentsSelected { get => _isTwoPaymentsSelected; set => SetProperty(ref _isTwoPaymentsSelected, value); }
    
    private bool _isMonthlySelected = false;
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

    private bool _isCollateralVisible = false;
    public bool IsCollateralVisible { get => _isCollateralVisible; set => SetProperty(ref _isCollateralVisible, value); }

    private bool _isYesCollateralSelected = false;
    public bool IsYesCollateralSelected { get => _isYesCollateralSelected; set => SetProperty(ref _isYesCollateralSelected, value); }

    private bool _isNoCollateralSelected = false;
    public bool IsNoCollateralSelected { get => _isNoCollateralSelected; set => SetProperty(ref _isNoCollateralSelected, value); }

    private bool _showNextButton = false;
    public bool ShowNextButton { get => _showNextButton; set => SetProperty(ref _showNextButton, value); }

    // Requester Information
    private string _requesterName = "";
    public string RequesterName { get => _requesterName; set => SetProperty(ref _requesterName, value); }

    private string _requesterPhone = "";
    public string RequesterPhone { get => _requesterPhone; set => SetProperty(ref _requesterPhone, value); }

    private string _requesterEmail = "";
    public string RequesterEmail { get => _requesterEmail; set => SetProperty(ref _requesterEmail, value); }

    private string _requesterAddress = "";
    public string RequesterAddress { get => _requesterAddress; set => SetProperty(ref _requesterAddress, value); }

    private string _requesterCity = "";
    public string RequesterCity { get => _requesterCity; set => SetProperty(ref _requesterCity, value); }

    private string _requesterState = "";
    public string RequesterState { get => _requesterState; set => SetProperty(ref _requesterState, value); }

    private string _requesterZipCode = "";
    public string RequesterZipCode { get => _requesterZipCode; set => SetProperty(ref _requesterZipCode, value); }

    private string _requesterIdNumber = "";
    public string RequesterIdNumber { get => _requesterIdNumber; set => SetProperty(ref _requesterIdNumber, value); }

    // Notification Type
    private bool _isNoNotificationSelected = true;
    public bool IsNoNotificationSelected { get => _isNoNotificationSelected; set => SetProperty(ref _isNoNotificationSelected, value); }

    private bool _isSMSNotificationSelected = false;
    public bool IsSMSNotificationSelected { get => _isSMSNotificationSelected; set => SetProperty(ref _isSMSNotificationSelected, value); }

    private bool _isEmailNotificationSelected = false;
    public bool IsEmailNotificationSelected { get => _isEmailNotificationSelected; set => SetProperty(ref _isEmailNotificationSelected, value); }

    // Computed outputs
    private decimal _periodicPayment = 0;
    public decimal PeriodicPayment { get => _periodicPayment; private set => SetProperty(ref _periodicPayment, value); }

    private decimal _totalInterest = 0;
    public decimal TotalInterest { get => _totalInterest; private set => SetProperty(ref _totalInterest, value); }

    private decimal _totalPayment = 0;
    public decimal TotalPayment { get => _totalPayment; private set => SetProperty(ref _totalPayment, value); }

    private int _totalPayments = 0;
    public int TotalPayments { get => _totalPayments; private set => SetProperty(ref _totalPayments, value); }

    public string InterestTypeDisplay => InterestType switch
    {
        InterestTypeOption.None => "No Interest",
        InterestTypeOption.Simple => "Simple Interest",
        InterestTypeOption.Compound => "Compound Interest",
        _ => "None"
    };

    public string PaybackDisplay
    {
        get
        {
            if (IsByDateSelected)
                return PaybackDate.ToString("MMM dd, yyyy");
            else if (IsByTimeSelected)
                return $"{TimeDuration} {(IsDaysSelected ? "Days" : "Months")}";
            return "Not set";
        }
    }

    public string TimeDurationLabel => IsDaysSelected ? "Days" : "Months";

    public string CollateralDisplay => IsYesCollateralSelected ? "Yes" : "No";

    public string InterestMethodDisplay => IsInterestMethodTotalSelected ? "Total %" : "APR (Annual)";

    public string NotificationTypeDisplay
    {
        get
        {
            if (IsSMSNotificationSelected) return "SMS";
            if (IsEmailNotificationSelected) return "Email";
            return "None";
        }
    }

    public bool HasAddress => !string.IsNullOrWhiteSpace(RequesterAddress) || !string.IsNullOrWhiteSpace(RequesterCity) ||
                              !string.IsNullOrWhiteSpace(RequesterState) || !string.IsNullOrWhiteSpace(RequesterZipCode);

    public string FullAddress
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(RequesterAddress)) parts.Add(RequesterAddress);
            if (!string.IsNullOrWhiteSpace(RequesterCity)) parts.Add(RequesterCity);
            if (!string.IsNullOrWhiteSpace(RequesterState)) parts.Add(RequesterState);
            if (!string.IsNullOrWhiteSpace(RequesterZipCode)) parts.Add(RequesterZipCode);
            return string.Join(", ", parts);
        }
    }

    public bool HasIdNumber => !string.IsNullOrWhiteSpace(RequesterIdNumber);

    public LoanFormViewModel()
    {
        Debug.WriteLine("LoanFormViewModel constructor started - LoanFormViewModel.cs:266");
        BackCommand = new Command(async () => 
        {
            Debug.WriteLine("BackCommand executing - LoanFormViewModel.cs:269");
            await Shell.Current.GoToAsync("..");
        });
        UploadCollateralImageCommand = new Command(async () => await Task.CompletedTask);
        SubmitCommand = new Command(async () =>
        {
            // Navigate to requester info page
            var requesterInfoPage = new LoanRequesterInfoPage();
            requesterInfoPage.BindingContext = this;
            await Shell.Current.Navigation.PushAsync(requesterInfoPage);
        });
        
        ContinueToRequesterInfoCommand = new Command(async () =>
        {
            // Navigate to requester info page
            var requesterInfoPage = new LoanRequesterInfoPage();
            requesterInfoPage.BindingContext = this;
            await Shell.Current.Navigation.PushAsync(requesterInfoPage);
        });
        
        SetNotificationTypeCommand = new Command<string>((type) =>
        {
            IsNoNotificationSelected = type == "None";
            IsSMSNotificationSelected = type == "SMS";
            IsEmailNotificationSelected = type == "Email";
        });
        
        ContinueToFinalSummaryCommand = new Command(async () =>
        {
            // Navigate to final summary page
            var finalSummaryPage = new LoanFinalSummaryPage();
            finalSummaryPage.BindingContext = this;
            await Shell.Current.Navigation.PushAsync(finalSummaryPage);
        });
        
        SubmitLoanRequestCommand = new Command(async () =>
        {
            // TODO: Implement actual loan request submission
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlertAsync("Success", "Loan request submitted successfully!", "OK");
            }
            await Shell.Current.GoToAsync("//loans");
        });
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
                IsOneTimeSelected = p.Label == "One Time";
                IsTwoPaymentsSelected = p.Label == "Two Payments";
                IsDailySelected = p.Label == "Daily";
                IsWeeklySelected = p.Label == "Weekly";
                IsBiweekSelected = p.Label == "Bi-week";
                IsHalfMonthSelected = p.Label == "Half Month";
                IsMonthlySelected = p.Label == "Monthly";
                
                // Show collateral options after frequency selection
                IsCollateralVisible = true;
            }
        });

        SetByDateCommand = new Command(() =>
        {
            IsByDateSelected = true;
            IsByTimeSelected = false;
            ShowDatePicker = true;
            ShowTimeInput = false;
        });

        SetByTimeCommand = new Command(() =>
        {
            IsByDateSelected = false;
            IsByTimeSelected = true;
            ShowDatePicker = false;
            ShowTimeInput = true;
        });

        SetDaysCommand = new Command(() =>
        {
            IsDaysSelected = true;
            IsMonthsSelected = false;
        });

        SetMonthsCommand = new Command(() =>
        {
            IsDaysSelected = false;
            IsMonthsSelected = true;
        });

        SetYesCollateralCommand = new Command(() =>
        {
            IsYesCollateralSelected = true;
            IsNoCollateralSelected = false;
            ShowNextButton = true;
        });

        SetNoCollateralCommand = new Command(() =>
        {
            IsYesCollateralSelected = false;
            IsNoCollateralSelected = true;
            ShowNextButton = true;
        });

        SetInterestMethodTotalCommand = new Command(() =>
        {
            IsInterestMethodTotalSelected = true;
            IsInterestMethodAPRSelected = false;
        });

        SetInterestMethodAPRCommand = new Command(() =>
        {
            IsInterestMethodTotalSelected = false;
            IsInterestMethodAPRSelected = true;
        });

        NextCommand = new Command(async () =>
        {
            // Calculate loan details before navigating
            CalculateLoanDetails();
            
            // Navigate to summary page - the page will use the same ViewModel instance
            var summaryPage = new LoanSummaryPage();
            summaryPage.BindingContext = this;
            await Shell.Current.Navigation.PushAsync(summaryPage);
        });
        
        // Initialize selection flags
        IsRequestSelected = true;
        IsInterestMethodTotalSelected = true;
        
        Debug.WriteLine("LoanFormViewModel constructor completed - LoanFormViewModel.cs:435");
    }

    private void CalculateLoanDetails()
    {
        // Parse amount
        if (!decimal.TryParse(AmountText, out decimal principal) || principal <= 0)
        {
            principal = 0;
            TotalPayments = 0;
            PeriodicPayment = 0;
            TotalInterest = 0;
            TotalPayment = 0;
            return;
        }

        // Determine number of periods
        int periods = 0;
        
        // Special handling for One Time and Two Payments
        if (SelectedPaymentFrequency.Label == "One Time")
        {
            periods = 1;
        }
        else if (SelectedPaymentFrequency.Label == "Two Payments")
        {
            periods = 2;
        }
        else if (IsByDateSelected)
        {
            var timeSpan = PaybackDate - DateTime.Today;
            periods = Math.Max(1, (int)(timeSpan.TotalDays / (365.0 / SelectedPaymentFrequency.PaymentsPerYear)));
        }
        else if (IsByTimeSelected && decimal.TryParse(TimeDuration, out decimal duration))
        {
            if (IsDaysSelected)
            {
                periods = Math.Max(1, (int)((double)duration / (365.0 / SelectedPaymentFrequency.PaymentsPerYear)));
            }
            else // Months
            {
                periods = Math.Max(1, (int)((double)duration * SelectedPaymentFrequency.PaymentsPerYear / 12.0));
            }
        }

        TotalPayments = Math.Max(1, periods);

        // Calculate based on interest type
        if (InterestType == InterestTypeOption.None)
        {
            TotalInterest = 0;
            TotalPayment = principal;
            PeriodicPayment = principal / TotalPayments;
        }
        else
        {
            decimal rate = InterestRate / 100m;
            
            if (IsInterestMethodTotalSelected)
            {
                // Total interest method: rate applies to entire loan period
                if (InterestType == InterestTypeOption.Simple)
                {
                    // Simple interest: Total interest = Principal * Rate
                    TotalInterest = principal * rate;
                    TotalPayment = principal + TotalInterest;
                    PeriodicPayment = TotalPayment / TotalPayments;
                }
                else // Compound
                {
                    // Compound interest: Final Amount = Principal * (1 + rate)
                    TotalPayment = principal * (decimal)Math.Pow((double)(1 + rate), 1);
                    TotalInterest = TotalPayment - principal;
                    PeriodicPayment = TotalPayment / TotalPayments;
                }
            }
            else // APR method
            {
                // APR method: rate is annual, calculate based on loan duration
                if (InterestType == InterestTypeOption.Simple)
                {
                    // Simple interest: I = P * r * t (where t is in years)
                    decimal years = TotalPayments / (decimal)SelectedPaymentFrequency.PaymentsPerYear;
                    TotalInterest = principal * rate * years;
                    TotalPayment = principal + TotalInterest;
                    PeriodicPayment = TotalPayment / TotalPayments;
                }
                else // Compound
                {
                    // Compound interest with periodic payments (amortization formula)
                    decimal periodicRate = rate / SelectedPaymentFrequency.PaymentsPerYear;
                    if (periodicRate > 0)
                    {
                        PeriodicPayment = principal * (periodicRate * (decimal)Math.Pow((double)(1 + periodicRate), TotalPayments)) /
                                          ((decimal)Math.Pow((double)(1 + periodicRate), TotalPayments) - 1);
                        TotalPayment = PeriodicPayment * TotalPayments;
                        TotalInterest = TotalPayment - principal;
                    }
                    else
                    {
                        PeriodicPayment = principal / TotalPayments;
                        TotalPayment = principal;
                        TotalInterest = 0;
                    }
                }
            }
        }
    }
}