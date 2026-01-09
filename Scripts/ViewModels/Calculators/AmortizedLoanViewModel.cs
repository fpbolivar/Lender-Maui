using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lender.Calculators;
using Lender.Helpers;

namespace Lender.ViewModels.Calculators;

public class AmortizedLoanViewModel : INotifyPropertyChanged
{
    private string _principal = "25000";
    private string _rate = "5.5";
    private string _termMonths = "60";
    private string _result = string.Empty;
    private string _errorMessage = string.Empty;
    private PaymentFrequencyOption _selectedFrequency = PaymentFrequencyOption.Default;

    public string Principal { get => _principal; set { _principal = value; OnPropertyChanged(); } }
    public string Rate { get => _rate; set { _rate = value; OnPropertyChanged(); } }
    public string TermMonths { get => _termMonths; set { _termMonths = value; OnPropertyChanged(); } }
    public string Result { get => _result; set { _result = value; OnPropertyChanged(); } }
    public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }
    public PaymentFrequencyOption SelectedFrequency { get => _selectedFrequency; set { _selectedFrequency = value; OnPropertyChanged(); } }
    public IReadOnlyList<PaymentFrequencyOption> PaymentFrequencyOptions { get; } = PaymentFrequencyOption.All;

    public ICommand CalculateCommand { get; }
    public ICommand BackCommand { get; }

    public AmortizedLoanViewModel()
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
        if (!int.TryParse(TermMonths, out var t) || t <= 0)
        { ErrorMessage = "Enter term months"; return; }

        var paymentsPerYear = SelectedFrequency?.PaymentsPerYear ?? PaymentFrequencyOption.Default.PaymentsPerYear;
        var result = LoanCalculator.CalculateAmortizedPayment(p, r, t, paymentsPerYear);
        Result = $"Payment ({SelectedFrequency?.Label ?? PaymentFrequencyOption.Default.Label}): ${result.PeriodicPayment:F2}\nTotal Interest: ${result.TotalInterest:F2}\nTotal Payment: ${result.TotalPayment:F2}\nPayments: {result.TotalPayments}";
    }

    private static bool TryParseDecimal(string input, out decimal value)
        => decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
