using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Lender.ViewModels;

/// <summary>
/// Base class for ViewModels with INotifyPropertyChanged implementation and common functionality
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the current page from the application
    /// </summary>
    protected Page? CurrentPage => Application.Current?.Windows[0]?.Page;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

/// <summary>
/// Base class for ViewModels with navigation capabilities
/// </summary>
public abstract class NavigableViewModel : BaseViewModel
{
    public ICommand NavigateToTransactionsCommand { get; }
    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToLoansCommand { get; }
    public ICommand NavigateToCalculatorCommand { get; }
    public ICommand NavigateToProfileCommand { get; }

    protected NavigableViewModel()
    {
        NavigateToTransactionsCommand = new Command(async () => await Shell.Current.GoToAsync("//transactions"));
        NavigateToDashboardCommand = new Command(async () => await Shell.Current.GoToAsync("//mainpage"));
        NavigateToLoansCommand = new Command(async () => await Shell.Current.GoToAsync("//loanform"));
        NavigateToCalculatorCommand = new Command(async () => await Shell.Current.GoToAsync("//calculator"));
        NavigateToProfileCommand = new Command(async () => await Shell.Current.GoToAsync("//profile"));
    }
}
