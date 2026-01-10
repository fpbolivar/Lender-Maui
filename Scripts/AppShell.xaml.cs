using System.Linq;
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
		
		// Immediately lock and close the flyout
		FlyoutBehavior = FlyoutBehavior.Locked;
		FlyoutIsPresented = false;
		FlyoutWidth = 0;
		
		// Register routes for navigation
		Routing.RegisterRoute("profile", typeof(ProfilePage));
		Routing.RegisterRoute("calculator", typeof(CalculatorPage));
		Routing.RegisterRoute("loanform", typeof(LoanFormPage));
		Routing.RegisterRoute("loansummary", typeof(LoanSummaryPage));
		Routing.RegisterRoute("loanrequesterinfo", typeof(LoanRequesterInfoPage));
		Routing.RegisterRoute("loanfinalsummary", typeof(LoanFinalSummaryPage));
		Routing.RegisterRoute(nameof(TransactionDetailPage), typeof(TransactionDetailPage));
		
		// Register calculator detail routes with modal presentation for slide-up animation
		Routing.RegisterRoute(nameof(SimpleInterestPage), typeof(SimpleInterestPage));
		Routing.RegisterRoute(nameof(CompoundInterestPage), typeof(CompoundInterestPage));
		Routing.RegisterRoute(nameof(AmortizedLoanPage), typeof(AmortizedLoanPage));
		Routing.RegisterRoute(nameof(MortgagePage), typeof(MortgagePage));
		Routing.RegisterRoute(nameof(AutoLoanPage), typeof(AutoLoanPage));
		Routing.RegisterRoute(nameof(SavingsPage), typeof(SavingsPage));
		Routing.RegisterRoute(nameof(InvestmentPage), typeof(InvestmentPage));

		// Prevent navigation to protected routes when unauthenticated (demo mode)
		Navigating += OnShellNavigating;
		
		// Prevent flyout from ever being presented
		PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(FlyoutIsPresented) && FlyoutIsPresented)
			{
				FlyoutIsPresented = false;
			}
		};
	}

	private void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
	{
		try
		{
			var target = e.Target?.Location?.OriginalString ?? string.Empty;
			var authService = ServiceHelper.GetService<IAuthenticationService>();

			// If not authenticated (demo mode) block navigation to protected areas
			if (authService != null && !authService.IsAuthenticated)
			{
				string[] protectedRoutes =
				{
					"profile",
					"calculator",
					nameof(SimpleInterestPage),
					nameof(CompoundInterestPage),
					nameof(AmortizedLoanPage),
					nameof(MortgagePage),
					nameof(AutoLoanPage),
					nameof(SavingsPage),
					nameof(InvestmentPage)
				};

				var isProtected = protectedRoutes.Any(r => target.Contains(r, StringComparison.OrdinalIgnoreCase));
				if (isProtected)
				{
					e.Cancel();
					MainThread.BeginInvokeOnMainThread(async () =>
					{
						var page = Application.Current?.Windows[0]?.Page;
						if (page != null)
							await page.DisplayAlertAsync("Demo mode", "Navigation is disabled in demo mode. Sign in to access this section.", "OK");
					});
				}
			}
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Shell navigation guard error: {ex.Message} - AppShell.xaml.cs:88");
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
			System.Diagnostics.Debug.WriteLine($"Auth check error: {ex.Message} - AppShell.xaml.cs:119");
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
			System.Diagnostics.Debug.WriteLine($"Sign out error: {ex.Message} - AppShell.xaml.cs:139");
		}
	}
}
