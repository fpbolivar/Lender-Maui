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
		#pragma warning disable CS0618
		Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(shell, false);
		#pragma warning restore CS0618
		
		return new Window(shell);
	}
}