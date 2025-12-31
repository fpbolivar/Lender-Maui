using Lender.ViewModels.Calculators;
using Microsoft.Maui.Controls;
using Lender.ViewModels;

namespace Lender;

public partial class SavingsPage : ContentPage
{
    public SavingsPage()
    {
        InitializeComponent();
        BindingContext = new SavingsViewModel();
        BottomNav.BindingContext = new CalculatorViewModel();
    }
}
