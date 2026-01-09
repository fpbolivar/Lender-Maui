using Microsoft.Maui.Controls;

namespace Lender.Views;

public partial class BottomNavView : ContentView
{
    public static readonly BindableProperty SelectedTabProperty = BindableProperty.Create(
        nameof(SelectedTab),
        typeof(string),
        typeof(BottomNavView),
        defaultValue: string.Empty);

    public string SelectedTab
    {
        get => (string)GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    public BottomNavView()
    {
        InitializeComponent();
        BindingContextChanged += (s, e) =>
        {
            // Propagate binding context to children if needed
        };
    }
}
