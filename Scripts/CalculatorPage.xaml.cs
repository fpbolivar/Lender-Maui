using Lender.ViewModels;

namespace Lender;

public partial class CalculatorPage : ContentPage
{
    public CalculatorPage()
    {
        InitializeComponent();
        BindingContext = new CalculatorViewModel();
    }
}
