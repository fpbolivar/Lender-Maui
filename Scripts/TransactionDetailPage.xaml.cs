using Lender.ViewModels;
using Microsoft.Maui.Controls;

namespace Lender;

public partial class TransactionDetailPage : ContentPage
{
    public TransactionDetailPage(TransactionDisplayModel displayModel)
    {
        InitializeComponent();
        BindingContext = new TransactionDetailViewModel(displayModel);
    }
}
