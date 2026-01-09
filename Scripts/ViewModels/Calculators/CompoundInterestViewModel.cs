using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lender.Calculators;
using Lender.Helpers;

namespace Lender.ViewModels.Calculators;

public class CompoundInterestViewModel : INotifyPropertyChanged
{
    private string _principal = "10000";
    private string _rate = "6";
    private string _compoundsPerYear = "12";
    private string _years = "2";
    private string _result = string.Empty;
    private string _errorMessage = string.Empty;
    private PaymentFrequencyOption _selectedFrequency = PaymentFrequencyOption.Default;

    public string Principal { get => _principal; set { _principal = value; OnPropertyChanged(); } }
    public string Rate { get => _rate; set { _rate = value; OnPropertyChanged(); } }
    public string CompoundsPerYear { get => _compoundsPerYear; set { _compoundsPerYear = value; OnPropertyChanged(); } }
    public string Years { get => _years; set { _years = value; OnPropertyChanged(); } }
    public string Result { get => _result; set { _result = value; OnPropertyChanged(); } }
    public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }
    public PaymentFrequencyOption SelectedFrequency { get => _selectedFrequency; set { _selectedFrequency = value; OnPropertyChanged(); } }
    public IReadOnlyList<PaymentFrequencyOption> PaymentFrequencyOptions { get; } = PaymentFrequencyOption.All;

    public ICommand CalculateCommand { get; }
    public ICommand BackCommand { get; }

    public CompoundInterestViewModel()
    {
        CalculateCommand = new Command(Calculate);
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private void Calculate()
    {
        ErrorMessage = string.Empty;
        if (!TryParseDecimal(Principal, out var p) || p < 0)
        { ErrorMessage = "Enter a valid principal"; return; }
        if (!TryParseDecimal(Rate, out var r) || r < 0)
        { ErrorMessage = "Enter a valid rate"; return; }
        if (!int.TryParse(CompoundsPerYear, out var c) || c <= 0)
        { ErrorMessage = "Enter compounds per year"; return; }
        if (!int.TryParse(Years, out var y) || y <= 0)
        { ErrorMessage = "Enter years"; return; }

        var result = LoanCalculator.CalculateCompoundInterest(p, r, c, y);

        var paymentsPerYear = SelectedFrequency?.PaymentsPerYear ?? PaymentFrequencyOption.Default.PaymentsPerYear;
        var totalPeriods = y * paymentsPerYear;
        var payoutPerPeriod = totalPeriods > 0 ? result.FutureValue / totalPeriods : result.FutureValue;

        Result = $"Future Value: ${result.FutureValue:F2}\nTotal Interest: ${result.TotalInterest:F2}\n{SelectedFrequency?.Label ?? PaymentFrequencyOption.Default.Label} Amount: ${payoutPerPeriod:F2}";
    }

    private static bool TryParseDecimal(string input, out decimal value)
        => decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
