using Lender.ViewModels.Calculators;
using Lender.ViewModels;

namespace Lender;

public partial class SimpleInterestPage : ContentPage
{
    public SimpleInterestPage()
    {
        InitializeComponent();
        BindingContext = new SimpleInterestViewModel();
        BottomNav.BindingContext = new CalculatorViewModel();
    }
}
