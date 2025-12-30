namespace Lender;

using Lender.ViewModels;
using Lender.Helpers;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        this.SafeAreaEdges = SafeAreaEdges.None;

		// Use DI for the view model so bindings are populated
		BindingContext = ServiceHelper.GetService<LoginViewModel>() ?? new LoginViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Attempt session restore; navigate to main page if successful
        if (BindingContext is LoginViewModel viewModel)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                var restored = await viewModel.RestoreSessionAsync();
                if (restored)
                {
                    await Shell.Current.GoToAsync("//mainpage", animate: false);
                }
            });
        }
    }
}
