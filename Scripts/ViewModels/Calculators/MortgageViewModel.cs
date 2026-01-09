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

public class MortgageViewModel : INotifyPropertyChanged
{
    private string _homePrice = string.Empty;
    private string _downPayment = string.Empty;
    private string _rate = string.Empty;
    private string _termYears = string.Empty;
    private string _propertyTax = string.Empty;
    private string _insurance = string.Empty;
    private string _pmi = string.Empty;
    private string _hoa = string.Empty;
    private string _result = string.Empty;
    private string _errorMessage = string.Empty;
    private PaymentFrequencyOption _selectedFrequency = PaymentFrequencyOption.Default;

    public string HomePrice { get => _homePrice; set => SetProperty(ref _homePrice, value); }
    public string DownPayment { get => _downPayment; set => SetProperty(ref _downPayment, value); }
    public string Rate { get => _rate; set => SetProperty(ref _rate, value); }
    public string TermYears { get => _termYears; set => SetProperty(ref _termYears, value); }
    public string PropertyTax { get => _propertyTax; set => SetProperty(ref _propertyTax, value); }
    public string Insurance { get => _insurance; set => SetProperty(ref _insurance, value); }
    public string PMI { get => _pmi; set => SetProperty(ref _pmi, value); }
    public string HOA { get => _hoa; set => SetProperty(ref _hoa, value); }

    public string Result { get => _result; set => SetProperty(ref _result, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public PaymentFrequencyOption SelectedFrequency { get => _selectedFrequency; set => SetProperty(ref _selectedFrequency, value); }
    public IReadOnlyList<PaymentFrequencyOption> PaymentFrequencyOptions { get; } = PaymentFrequencyOption.All;

    public ICommand CalculateCommand { get; }
    public ICommand BackCommand { get; }

    public MortgageViewModel()
    {
        CalculateCommand = new Command(Calculate);
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private void Calculate()
    {
        ErrorMessage = string.Empty;
        Result = string.Empty;

        if (!TryParseDecimal(HomePrice, out var homePrice) || homePrice <= 0m)
        { ErrorMessage = "Enter a valid home price."; return; }
        TryParseDecimal(DownPayment, out var downPayment);
        if (!TryParseDecimal(Rate, out var ratePercent) || ratePercent < 0m)
        { ErrorMessage = "Enter a valid rate."; return; }
        if (!int.TryParse(TermYears, out var years) || years <= 0)
        { ErrorMessage = "Enter term (years)."; return; }
        TryParseDecimal(PropertyTax, out var propertyTax);
        TryParseDecimal(Insurance, out var insurance);
        TryParseDecimal(PMI, out var pmi);
        TryParseDecimal(HOA, out var hoa);

        var paymentsPerYear = SelectedFrequency?.PaymentsPerYear ?? PaymentFrequencyOption.Default.PaymentsPerYear;
        var result = MortgageCalculator.CalculateMortgage(homePrice, downPayment, ratePercent, years, propertyTax, insurance, pmi, hoa, paymentsPerYear);
        var principal = homePrice - downPayment;

        Result = $"Principal: ${principal:N2}\nPayment ({SelectedFrequency?.Label ?? PaymentFrequencyOption.Default.Label}): ${result.PaymentPerPeriod:N2}\nP&I: ${result.PrincipalAndInterest:N2}\nTaxes: ${result.Taxes:N2}\nInsurance: ${result.Insurance:N2}\nPMI: ${result.PMI:N2}\nHOA: ${result.HOA:N2}\nPayments: {result.TotalPayments}\nTotal Interest: ${result.TotalInterest:N2}\nTotal Paid: ${result.TotalPayment:N2}";
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
