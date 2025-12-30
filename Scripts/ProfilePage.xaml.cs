using Lender.ViewModels;

namespace Lender;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        BindingContext = new ProfileViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProfileViewModel viewModel)
        {
            _ = viewModel.RefreshAsync();
        }
    }
}
