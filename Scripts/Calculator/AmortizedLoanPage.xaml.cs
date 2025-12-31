using Lender.ViewModels.Calculators;
using Lender.ViewModels;

namespace Lender;

public partial class AmortizedLoanPage : ContentPage
{
    public AmortizedLoanPage()
    {
        InitializeComponent();
        BindingContext = new AmortizedLoanViewModel();
        BottomNav.BindingContext = new CalculatorViewModel();
    }
}
