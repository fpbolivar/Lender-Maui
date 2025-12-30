using Foundation;
using UIKit;

namespace Lender;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
	{
		System.Console.WriteLine("AppDelegate.FinishedLaunching called - AppDelegate.cs:11");
		// Keep default status bar handling (avoids obsolete API usage)
		
		return base.FinishedLaunching(application, launchOptions);
	}

	public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
	{
		// Handle deep links if needed
		return base.OpenUrl(application, url, options);
	}

	protected override MauiApp CreateMauiApp()
	{
		System.Console.WriteLine("AppDelegate.CreateMauiApp called - AppDelegate.cs:27");
		return MauiProgram.CreateMauiApp();
	}
}
