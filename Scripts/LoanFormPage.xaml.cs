using Lender.ViewModels;
using System.Diagnostics;

namespace Lender;

public partial class LoanFormPage : ContentPage
{
    public LoanFormPage()
    {
        Debug.WriteLine("LoanFormPage constructor");
        InitializeComponent();
        BindingContext = new LoanFormViewModel();
        Debug.WriteLine("LoanFormPage initialized");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("OnBackClicked");
        await Shell.Current.GoToAsync("..");
    }
}
