using System.Collections.Generic;
using Lender.Helpers;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lender.Calculators;

namespace Lender.ViewModels.Calculators;

public class SimpleInterestViewModel : INotifyPropertyChanged
{
    private string _principal = "10000";
    private string _rate = "6";
    private string _years = "2";
    private string _result = string.Empty;
    private string _errorMessage = string.Empty;
    private PaymentFrequencyOption _selectedFrequency = PaymentFrequencyOption.Default;

    public string Principal
    {
        get => _principal;
        set { _principal = value; OnPropertyChanged(); }
    }
    public string Rate
    {
        get => _rate;
        set { _rate = value; OnPropertyChanged(); }
    }
    public string Years
    {
        get => _years;
        set { _years = value; OnPropertyChanged(); }
    }
    public string Result
    {
        get => _result;
        set { _result = value; OnPropertyChanged(); }
    }
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }
    public PaymentFrequencyOption SelectedFrequency
    {
        get => _selectedFrequency;
        set { _selectedFrequency = value; OnPropertyChanged(); }
    }
    public IReadOnlyList<PaymentFrequencyOption> PaymentFrequencyOptions { get; } = PaymentFrequencyOption.All;

    public ICommand CalculateCommand { get; }
    public ICommand BackCommand { get; }

    public SimpleInterestViewModel()
    {
        CalculateCommand = new Command(Calculate);
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private void Calculate()
    {
        ErrorMessage = string.Empty;
        if (!TryParseDecimal(Principal, out var p) || p < 0)
        {
            ErrorMessage = "Enter a valid principal";
            return;
        }
        if (!TryParseDecimal(Rate, out var r) || r < 0)
        {
            ErrorMessage = "Enter a valid rate";
            return;
        }
        if (!int.TryParse(Years, out var y) || y <= 0)
        {
            ErrorMessage = "Enter years";
            return;
        }

        var result = LoanCalculator.CalculateSimpleInterest(p, r, y);

        var paymentsPerYear = SelectedFrequency?.PaymentsPerYear ?? PaymentFrequencyOption.Default.PaymentsPerYear;
        var totalPeriods = y * paymentsPerYear;
        var paymentPerPeriod = totalPeriods > 0 ? result.TotalPayment / totalPeriods : result.TotalPayment;

        Result = $"Principal: ${p:F2}\nTotal Interest: ${result.TotalInterest:F2}\nTotal Payment: ${result.TotalPayment:F2}\nPayments: {totalPeriods}\n{SelectedFrequency?.Label ?? PaymentFrequencyOption.Default.Label} Payment: ${paymentPerPeriod:F2}";
    }

    private static bool TryParseDecimal(string input, out decimal value)
    {
        return decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
