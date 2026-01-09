using System.Diagnostics;
using System.Windows.Input;

namespace Lender.ViewModels;

public class LoanFormViewModel : NavigableViewModel
{
    public ICommand BackCommand { get; }
    public ICommand UploadCollateralImageCommand { get; }
    public ICommand SubmitCommand { get; }
    public ICommand SetRequestModeCommand { get; }
    public ICommand SetSendModeCommand { get; }

    private string _amountText = "0";
    public string AmountText { get => _amountText; set => SetProperty(ref _amountText, value ?? "0"); }

    private string _interestRateText = "0";
    public string InterestRateText { get => _interestRateText; set => SetProperty(ref _interestRateText, value ?? "0"); }

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
        Debug.WriteLine("LoanFormViewModel constructor started");
        BackCommand = new Command(async () => 
        {
            Debug.WriteLine("BackCommand executing");
            await Shell.Current.GoToAsync("..");
        });
        UploadCollateralImageCommand = new Command(async () => await Task.CompletedTask);
        SubmitCommand = new Command(async () => await Task.CompletedTask, () => true);
        SetRequestModeCommand = new Command(() => { });
        SetSendModeCommand = new Command(() => { });
        
        Debug.WriteLine("LoanFormViewModel constructor completed");
    }
}