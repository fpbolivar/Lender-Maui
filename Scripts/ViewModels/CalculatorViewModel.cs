using System.Windows.Input;
using Lender;
using Microsoft.Maui.Controls;
using Lender.Services;

namespace Lender.ViewModels;

public class CalculatorViewModel
{
    public ICommand BackCommand { get; }
    public ICommand OpenSimpleInterestCommand { get; }
    public ICommand OpenCompoundInterestCommand { get; }
    public ICommand OpenAmortizedCommand { get; }
    public ICommand OpenMortgageCommand { get; }
    public ICommand OpenAutoLoanCommand { get; }
    public ICommand OpenSavingsCommand { get; }
    public ICommand OpenInvestmentCommand { get; }
    public ICommand NavigateToTransactionsCommand { get; }
    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToRequestLoanCommand { get; }
    public ICommand NavigateToCalculatorCommand { get; }
    public ICommand NavigateToProfileCommand { get; }

    public CalculatorViewModel()
    {
        BackCommand = new Command(async () => await NavBarNavigation.GoToDashboardAsync());
        OpenSimpleInterestCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(SimpleInterestPage), animate: true));
        OpenCompoundInterestCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(CompoundInterestPage), animate: true));
        OpenAmortizedCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(AmortizedLoanPage), animate: true));
        OpenMortgageCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(MortgagePage), animate: true));
        OpenAutoLoanCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(AutoLoanPage), animate: true));
        OpenSavingsCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(SavingsPage), animate: true));
        OpenInvestmentCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(InvestmentPage), animate: true));
        NavigateToTransactionsCommand = new Command(async () => await Shell.Current.GoToAsync("///transactions"));
        NavigateToDashboardCommand = new Command(async () => await NavBarNavigation.GoToDashboardAsync());
        NavigateToRequestLoanCommand = new Command(async () => await Shell.Current.DisplayAlertAsync("Request/Send Loan", "Navigate to Request/Send Loan page", "OK"));
        NavigateToCalculatorCommand = new Command(async () => await NavBarNavigation.GoToCalculatorAsync());
        NavigateToProfileCommand = new Command(async () => await NavBarNavigation.GoToProfileAsync());
    }
}
