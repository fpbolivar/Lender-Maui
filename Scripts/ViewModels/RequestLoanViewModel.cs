using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Lender.ViewModels;

public class LoansViewModel : NavigableViewModel
{
    public ICommand StartRequestCommand { get; }
    public ICommand StartSendCommand { get; }

    public LoansViewModel()
    {
        // Navigate directly to unified loan form
        StartRequestCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("//loanform");
        });
        
        StartSendCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("//loanform?mode=send");
        });
    }
}
