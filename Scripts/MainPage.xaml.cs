namespace Lender;

using Lender.ViewModels;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		this.SafeAreaEdges = SafeAreaEdges.None;
		BindingContext = new DashboardViewModel();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is DashboardViewModel vm)
		{
			MainThread.BeginInvokeOnMainThread(async () => await vm.RefreshAsync());
		}
	}
}
