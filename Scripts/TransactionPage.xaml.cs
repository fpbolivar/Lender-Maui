using Lender.ViewModels;

namespace Lender;

public partial class TransactionPage : ContentPage
{
    public TransactionPage()
    {
        InitializeComponent();
        BindingContext = new TransactionViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TransactionViewModel viewModel)
        {
            viewModel.OnAppearingAsync();
        }
    }
}
