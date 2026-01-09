using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Lender.Services;

namespace Lender.ViewModels;

public class LoansViewModel : INotifyPropertyChanged
{
    public ICommand StartRequestCommand { get; }
    public ICommand StartSendCommand { get; }
    public ICommand NavigateToTransactionsCommand { get; }
    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToLoansCommand { get; }
    public ICommand NavigateToCalculatorCommand { get; }
    public ICommand NavigateToProfileCommand { get; }

    public LoansViewModel()
    {
        // Navigate directly to unified loan form
        StartRequestCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("//loanform");
        });
        
        StartSendCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("//loanform?mode=send");
        });
        
        NavigateToTransactionsCommand = new Command(async () => await Shell.Current.GoToAsync("//transactions"));
        NavigateToDashboardCommand = new Command(async () => await Shell.Current.GoToAsync("//mainpage"));
        NavigateToLoansCommand = new Command(async () => await Shell.Current.GoToAsync("//loanform"));
        NavigateToCalculatorCommand = new Command(async () => await Shell.Current.GoToAsync("//calculator"));
        NavigateToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("//profile"));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
