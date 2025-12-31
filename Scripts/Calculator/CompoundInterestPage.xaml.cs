using Lender.ViewModels.Calculators;
using Lender.ViewModels;

namespace Lender;

public partial class CompoundInterestPage : ContentPage
{
    public CompoundInterestPage()
    {
        InitializeComponent();
        BindingContext = new CompoundInterestViewModel();
        BottomNav.BindingContext = new CalculatorViewModel();
    }
}
