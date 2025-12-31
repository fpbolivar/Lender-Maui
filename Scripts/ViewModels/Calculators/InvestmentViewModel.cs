using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lender.Calculators;
using Microsoft.Maui.Controls;
using Lender.Helpers;

namespace Lender.ViewModels.Calculators;

public class InvestmentViewModel : INotifyPropertyChanged
{
    private string _initialInvestment = string.Empty;
    private string _monthlyContribution = string.Empty;
    private string _rate = string.Empty;
    private string _years = string.Empty;
    private string _result = string.Empty;
    private string _errorMessage = string.Empty;
    private PaymentFrequencyOption _selectedFrequency = PaymentFrequencyOption.Default;

    public string InitialInvestment { get => _initialInvestment; set => SetProperty(ref _initialInvestment, value); }
    public string MonthlyContribution { get => _monthlyContribution; set => SetProperty(ref _monthlyContribution, value); }
    public string Rate { get => _rate; set => SetProperty(ref _rate, value); }
    public string Years { get => _years; set => SetProperty(ref _years, value); }

    public string Result { get => _result; set => SetProperty(ref _result, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public PaymentFrequencyOption SelectedFrequency { get => _selectedFrequency; set => SetProperty(ref _selectedFrequency, value); }
    public IReadOnlyList<PaymentFrequencyOption> PaymentFrequencyOptions { get; } = PaymentFrequencyOption.All;

    public ICommand CalculateCommand { get; }
    public ICommand BackCommand { get; }

    public InvestmentViewModel()
    {
        CalculateCommand = new Command(Calculate);
        BackCommand = new Command(async () => await Shell.Current.GoToAsync("//calculator"));
    }

    private void Calculate()
    {
        ErrorMessage = string.Empty;
        Result = string.Empty;

        if (!TryParseDecimal(InitialInvestment, out var initial) || initial < 0m)
        { ErrorMessage = "Enter a valid initial amount."; return; }
        TryParseDecimal(MonthlyContribution, out var monthlyContribution);
        if (!TryParseDecimal(Rate, out var ratePercent) || ratePercent < 0m)
        { ErrorMessage = "Enter a valid rate."; return; }
        if (!int.TryParse(Years, out var years) || years <= 0)
        { ErrorMessage = "Enter years."; return; }

        var paymentsPerYear = SelectedFrequency?.PaymentsPerYear ?? PaymentFrequencyOption.Default.PaymentsPerYear;
        var result = InvestmentCalculator.CalculateInvestment(initial, monthlyContribution, ratePercent, years, paymentsPerYear);
        var totalPeriods = years * paymentsPerYear;

        Result = $"Initial: ${initial:N2}\nEnding Balance: ${result.FutureValue:N2}\nContribution ({SelectedFrequency?.Label ?? PaymentFrequencyOption.Default.Label}): ${monthlyContribution:N2}\nPayments: {totalPeriods}\nTotal Contributions: ${result.TotalContribution:N2}\nTotal Growth: ${result.TotalGrowth:N2}";
    }

    private static bool TryParseDecimal(string input, out decimal value)
        => decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(name);
        return true;
    }
}
