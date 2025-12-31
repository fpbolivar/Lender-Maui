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

public class AutoLoanViewModel : INotifyPropertyChanged
{
    private string _vehiclePrice = string.Empty;
    private string _downPayment = string.Empty;
    private string _rate = string.Empty;
    private string _termMonths = string.Empty;
    private string _salesTaxPercent = string.Empty;
    private string _fees = string.Empty;
    private string _result = string.Empty;
    private string _errorMessage = string.Empty;
    private PaymentFrequencyOption _selectedFrequency = PaymentFrequencyOption.Default;

    public string VehiclePrice { get => _vehiclePrice; set => SetProperty(ref _vehiclePrice, value); }
    public string DownPayment { get => _downPayment; set => SetProperty(ref _downPayment, value); }
    public string Rate { get => _rate; set => SetProperty(ref _rate, value); }
    public string TermMonths { get => _termMonths; set => SetProperty(ref _termMonths, value); }
    public string SalesTaxPercent { get => _salesTaxPercent; set => SetProperty(ref _salesTaxPercent, value); }
    public string Fees { get => _fees; set => SetProperty(ref _fees, value); }

    public string Result { get => _result; set => SetProperty(ref _result, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public PaymentFrequencyOption SelectedFrequency { get => _selectedFrequency; set => SetProperty(ref _selectedFrequency, value); }
    public IReadOnlyList<PaymentFrequencyOption> PaymentFrequencyOptions { get; } = PaymentFrequencyOption.All;

    public ICommand CalculateCommand { get; }
    public ICommand BackCommand { get; }

    public AutoLoanViewModel()
    {
        CalculateCommand = new Command(Calculate);
        BackCommand = new Command(async () => await Shell.Current.GoToAsync("//calculator"));
    }

    private void Calculate()
    {
        ErrorMessage = string.Empty;
        Result = string.Empty;

        if (!TryParseDecimal(VehiclePrice, out var vehiclePrice) || vehiclePrice <= 0m)
        { ErrorMessage = "Enter a valid vehicle price."; return; }
        TryParseDecimal(DownPayment, out var downPayment);
        if (!TryParseDecimal(Rate, out var ratePercent) || ratePercent < 0m)
        { ErrorMessage = "Enter a valid rate."; return; }
        if (!int.TryParse(TermMonths, out var months) || months <= 0)
        { ErrorMessage = "Enter term (months)."; return; }
        TryParseDecimal(SalesTaxPercent, out var taxPercent);
        TryParseDecimal(Fees, out var fees);

        var paymentsPerYear = SelectedFrequency?.PaymentsPerYear ?? PaymentFrequencyOption.Default.PaymentsPerYear;
        var result = AutoLoanCalculator.CalculateAutoLoan(vehiclePrice, downPayment, 0m, taxPercent, fees, ratePercent, months, paymentsPerYear);

        Result = $"Financed: ${result.AmountFinanced:N2}\nPayment ({SelectedFrequency?.Label ?? PaymentFrequencyOption.Default.Label}): ${result.PaymentPerPeriod:N2}\nPayments: {result.TotalPayments}\nTotal Interest: ${result.TotalInterest:N2}\nTotal Cost: ${result.TotalPayment:N2}";
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
