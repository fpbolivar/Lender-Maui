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

        // Disable flyout/menu gestures on the login screen
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Ensure flyout is closed/disabled when showing login
        Shell.Current.FlyoutIsPresented = false;
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Restore flyout to automatic for other pages
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
    }
}
