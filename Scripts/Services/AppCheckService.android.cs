#if ANDROID
using System.Diagnostics;
using Android.App;
using Java.Util.Concurrent;

namespace Lender.Services;

/// <summary>
/// Android-specific implementation of App Check token retrieval using native Firebase SDK.
/// Calls FirebaseAppCheck.getInstance().getAppCheckToken() via Java interop.
/// </summary>
public partial class AppCheckService
{
    private static string? _cachedToken = null;
    private static DateTime _cachedTokenExpiry = DateTime.MinValue;
    private const int TOKEN_CACHE_SECONDS = 240; // Cache for 4 minutes (Firebase tokens valid ~1 hour)

    public partial async Task<string?> GetAppCheckTokenAsync()
    {
        try
        {
            Debug.WriteLine("[AppCheck] Android: GetAppCheckTokenAsync called - AppCheckService.android.cs:18");
            
            // Check cache first
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _cachedTokenExpiry)
            {
                Debug.WriteLine($"[AppCheck] Android: Returning cached token - AppCheckService.android.cs:22");
                return _cachedToken;
            }

            // Get token from Firebase SDK
            return await GetTokenFromFirebaseSDKAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AppCheck] Android exception: {ex.GetType().Name}: {ex.Message} - AppCheckService.android.cs:31");
            return null;
        }
    }

    /// <summary>
    /// Calls native Firebase App Check SDK: FirebaseAppCheck.getInstance().getAppCheckToken()
    /// This returns a Task<AppCheckToken> that we need to wait for and extract the token string.
    /// </summary>
    private static async Task<string?> GetTokenFromFirebaseSDKAsync()
    {
        try
        {
            Debug.WriteLine("[AppCheck] Android: Attempting Firebase SDK call - AppCheckService.android.cs:46");

            // Get FirebaseAppCheck class via Java interop
            var appCheckClass = Java.Lang.Class.ForName("com.google.firebase.appcheck.FirebaseAppCheck");
            if (appCheckClass == null)
            {
                Debug.WriteLine("[AppCheck] Android: FirebaseAppCheck class not found - ensure Firebase SDK is installed - AppCheckService.android.cs:51");
                return null;
            }

            // Get getInstance() static method
            var getInstanceMethod = appCheckClass.GetMethod("getInstance", null);
            if (getInstanceMethod == null)
            {
                Debug.WriteLine("[AppCheck] Android: getInstance() method not found - AppCheckService.android.cs:57");
                return null;
            }

            // Call getInstance() to get singleton
            var appCheckInstance = getInstanceMethod.Invoke(null, null);
            if (appCheckInstance == null)
            {
                Debug.WriteLine("[AppCheck] Android: FirebaseAppCheck singleton not available - AppCheckService.android.cs:64");
                return null;
            }

            // Get getAppCheckToken() method - returns Task<AppCheckToken>
            var getTokenMethod = appCheckClass.GetMethod("getAppCheckToken", null);
            if (getTokenMethod == null)
            {
                Debug.WriteLine("[AppCheck] Android: getAppCheckToken() method not found - AppCheckService.android.cs:71");
                return null;
            }

            // Call getAppCheckToken() - returns a Task
            var tokenTask = getTokenMethod.Invoke(appCheckInstance, null) as Java.Util.Concurrent.IFuture;
            if (tokenTask == null)
            {
                Debug.WriteLine("[AppCheck] Android: getAppCheckToken() returned null task - AppCheckService.android.cs:79");
                return null;
            }

            // Wait for the task to complete (with timeout)
            var maxWaitMs = 5000; // 5 second timeout
            try
            {
                var appCheckToken = tokenTask.Get(maxWaitMs, Java.Util.Concurrent.TimeUnit.Milliseconds);
                if (appCheckToken == null)
                {
                    Debug.WriteLine("[AppCheck] Android: App Check token task returned null - AppCheckService.android.cs:89");
                    return null;
                }

                // Extract token string from AppCheckToken object
                var tokenClass = appCheckToken.Class;
                var getTokenMethod2 = tokenClass.GetMethod("getToken", null);
                if (getTokenMethod2 == null)
                {
                    Debug.WriteLine("[AppCheck] Android: getToken() method on AppCheckToken not found - AppCheckService.android.cs:97");
                    return null;
                }

                var tokenString = getTokenMethod2.Invoke(appCheckToken, null) as string;
                if (!string.IsNullOrEmpty(tokenString))
                {
                    _cachedToken = tokenString;
                    _cachedTokenExpiry = DateTime.UtcNow.AddSeconds(TOKEN_CACHE_SECONDS);
                    Debug.WriteLine($"[AppCheck] Android: Got token from SDK (length: {tokenString.Length}) - AppCheckService.android.cs:106");
                    return tokenString;
                }

                Debug.WriteLine("[AppCheck] Android: Token string is empty - AppCheckService.android.cs:109");
                return null;
            }
            catch (Java.Util.Concurrent.TimeoutException)
            {
                Debug.WriteLine("[AppCheck] Android: getAppCheckToken() timed out after 5 seconds - AppCheckService.android.cs:113");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AppCheck] Android SDK call failed: {ex.GetType().Name}: {ex.Message} - AppCheckService.android.cs:118");
            Debug.WriteLine($"[AppCheck] Android Stack trace: {ex.StackTrace} - AppCheckService.android.cs:119");
            return null;
        }
    }
}

#endif

