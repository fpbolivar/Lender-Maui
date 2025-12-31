using Lender.ViewModels.Calculators;
using Microsoft.Maui.Controls;
using Lender.ViewModels;

namespace Lender;

public partial class InvestmentPage : ContentPage
{
    public InvestmentPage()
    {
        InitializeComponent();
        BindingContext = new InvestmentViewModel();
        BottomNav.BindingContext = new CalculatorViewModel();
    }
}
