using Lender.ViewModels.Calculators;
using Microsoft.Maui.Controls;
using Lender.ViewModels;

namespace Lender;

public partial class MortgagePage : ContentPage
{
    public MortgagePage()
    {
        InitializeComponent();
        BindingContext = new MortgageViewModel();
        BottomNav.BindingContext = new CalculatorViewModel();
    }
}
