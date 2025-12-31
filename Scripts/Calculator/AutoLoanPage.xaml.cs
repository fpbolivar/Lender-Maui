using Lender.ViewModels.Calculators;
using Microsoft.Maui.Controls;
using Lender.ViewModels;

namespace Lender;

public partial class AutoLoanPage : ContentPage
{
    public AutoLoanPage()
    {
        InitializeComponent();
        BindingContext = new AutoLoanViewModel();
        BottomNav.BindingContext = new CalculatorViewModel();
    }
}
