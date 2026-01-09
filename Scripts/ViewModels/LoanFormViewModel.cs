using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lender.Calculators;
using Lender.Helpers;
using Lender.Models;
using Lender.Models.Enums;
using Lender.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Lender.ViewModels;

public class LoanFormViewModel : INotifyPropertyChanged
{
    public ICommand BackCommand { get; }
    public ICommand UploadCollateralImageCommand { get; }
    public ICommand SubmitCommand { get; }
    public ICommand SetRequestModeCommand { get; }
    public ICommand SetSendModeCommand { get; }
    
    // Navigation commands for BottomNavView
    public ICommand NavigateToTransactionsCommand { get; }
    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToLoansCommand { get; }
    public ICommand NavigateToCalculatorCommand { get; }
    public ICommand NavigateToProfileCommand { get; }

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

    public event PropertyChangedEventHandler? PropertyChanged;

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
        
        // Navigation commands
        NavigateToTransactionsCommand = new Command(async () => await Shell.Current.GoToAsync("//transactions"));
        NavigateToDashboardCommand = new Command(async () => await Shell.Current.GoToAsync("//mainpage"));
        NavigateToLoansCommand = new Command(async () => await Shell.Current.GoToAsync("//loanform"));
        NavigateToCalculatorCommand = new Command(async () => await Shell.Current.GoToAsync("//calculator"));
        NavigateToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("//profile"));
        
        Debug.WriteLine("LoanFormViewModel constructor completed");
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        return false;
    }
}

