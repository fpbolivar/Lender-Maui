using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace Lender;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var shell = new AppShell();
		
		// Force all pages to ignore safe area
		Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(shell, false);
		
		return new Window(shell);
	}
}