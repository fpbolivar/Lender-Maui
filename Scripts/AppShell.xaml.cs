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
