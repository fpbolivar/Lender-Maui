using Lender.ViewModels;

namespace Lender;

public partial class LoanSummaryPage : ContentPage
{
    public LoanSummaryPage()
    {
        InitializeComponent();
        // Will receive ViewModel from navigation
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // ViewModel will be set via navigation parameters
    }
}
