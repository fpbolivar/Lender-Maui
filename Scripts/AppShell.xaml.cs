using Lender.Helpers;
using Lender.Services;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Lender;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Register routes for navigation
		Routing.RegisterRoute("profile", typeof(ProfilePage));

		// Prevent navigation to protected routes when unauthenticated (demo mode)
		Navigating += OnShellNavigating;
	}

	private void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
	{
		try
		{
			var target = e.Target?.Location?.OriginalString ?? string.Empty;
			var authService = ServiceHelper.GetService<IAuthenticationService>();

			// If not authenticated (demo mode) block navigation to profile/settings
			if (authService != null && !authService.IsAuthenticated)
			{
				if (target.Contains("profile", StringComparison.OrdinalIgnoreCase))
				{
					e.Cancel();
					MainThread.BeginInvokeOnMainThread(async () =>
					{
						await Application.Current.MainPage.DisplayAlert("Demo mode", "Navigation is disabled in demo mode. Sign in to access this section.", "OK");
					});
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Shell navigation guard error: {ex.Message}");
		}
	}

	private async Task CheckAuthenticationAsync()
	{
		try
		{
			var authService = ServiceHelper.GetService<IAuthenticationService>();
			if (authService != null)
			{
				var isAuthenticated = await authService.RestoreSessionAsync();
				if (!isAuthenticated)
				{
					// Navigate to login
					await GoToAsync("//login", animate: false);
				}
				else
				{
					// Navigate to mainpage
					await GoToAsync("//mainpage", animate: false);
				}
			}
			else
			{
				// Service not available, go to login
				await GoToAsync("//login", animate: false);
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Auth check error: {ex.Message} - AppShell.xaml.cs:53");
			await GoToAsync("//login", animate: false);
		}
	}

	private async void OnSignOutClicked(object sender, EventArgs e)
	{
		try
		{
			var authService = ServiceHelper.GetService<IAuthenticationService>();
			if (authService != null)
			{
				await authService.SignOutAsync();
			}
			
			FlyoutIsPresented = false;
			await GoToAsync("//login", animate: true);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Sign out error: {ex.Message}");
		}
	}
}
