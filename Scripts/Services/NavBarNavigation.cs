using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Lender.Services;

public static class NavBarNavigation
{
    public static Task GoToDashboardAsync(bool isDemoMode = false)
        => NavigateAsync("mainpage", isDemoMode, blockInDemo: true);

    public static Task GoToCalculatorAsync(bool isDemoMode = false)
        => NavigateAsync("calculator", isDemoMode, blockInDemo: false);

    public static Task GoToProfileAsync(bool isDemoMode = false)
        => NavigateAsync("profile", isDemoMode, blockInDemo: false);

    private static async Task NavigateAsync(string route, bool isDemoMode, bool blockInDemo)
    {
        if (blockInDemo && isDemoMode)
        {
            await Shell.Current.DisplayAlertAsync(
                "Demo mode",
                "Navigation is disabled in demo mode. Sign in to access this section.",
                "OK");
            return;
        }

        try
        {
            await Shell.Current.GoToAsync(route, true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NavBarNavigation] Primary nav failed for route '{route}': {ex.Message}");
            // Fallback to absolute route to avoid crash
            await Shell.Current.GoToAsync($"//{route}", true);
        }
    }
}
