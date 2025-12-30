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
}
