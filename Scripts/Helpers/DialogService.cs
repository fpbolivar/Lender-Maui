namespace Lender.Helpers;

/// <summary>
/// Service for displaying alerts and dialogs with consistent styling
/// </summary>
public static class DialogService
{
    /// <summary>
    /// Shows an error alert
    /// </summary>
    public static async Task ShowErrorAsync(string message, string title = "Error")
    {
        var page = Application.Current?.Windows[0]?.Page;
        if (page != null)
            await page.DisplayAlert(title, message, "OK");
    }

    /// <summary>
    /// Shows a success alert
    /// </summary>
    public static async Task ShowSuccessAsync(string message, string title = "Success")
    {
        var page = Application.Current?.Windows[0]?.Page;
        if (page != null)
            await page.DisplayAlert(title, message, "OK");
    }

    /// <summary>
    /// Shows an info alert
    /// </summary>
    public static async Task ShowInfoAsync(string message, string title = "Info")
    {
        var page = Application.Current?.Windows[0]?.Page;
        if (page != null)
            await page.DisplayAlert(title, message, "OK");
    }

    /// <summary>
    /// Shows a confirmation dialog
    /// </summary>
    public static async Task<bool> ShowConfirmAsync(string message, string title, string accept = "Yes", string cancel = "No")
    {
        var page = Application.Current?.Windows[0]?.Page;
        if (page != null)
            return await page.DisplayAlert(title, message, accept, cancel);
        return false;
    }

    /// <summary>
    /// Shows a prompt dialog
    /// </summary>
    public static async Task<string?> ShowPromptAsync(string message, string title, string placeholder = "", string initialValue = "")
    {
        var page = Application.Current?.Windows[0]?.Page;
        if (page != null)
            return await page.DisplayPromptAsync(title, message, placeholder: placeholder, initialValue: initialValue);
        return null;
    }
}
