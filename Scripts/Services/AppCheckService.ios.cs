#if IOS
using System.Diagnostics;
using System.Runtime.InteropServices;
using ObjCRuntime;
using Foundation;

namespace Lender.Services;

/// <summary>
/// iOS-specific implementation of App Check token retrieval using native Firebase SDK.
/// Uses P/Invoke to directly call Objective-C runtime and Firebase SDK methods.
/// For production, consider implementing via platform channels for full async support.
/// </summary>
public partial class AppCheckService
{
    private static string? _cachedToken = null;
    private static DateTime _cachedTokenExpiry = DateTime.MinValue;
    private const int TOKEN_CACHE_SECONDS = 240; // Cache for 4 minutes (Firebase tokens valid ~1 hour)

    // P/Invoke declarations for Objective-C runtime
    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr sel_getUid(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_NSString(IntPtr receiver, IntPtr selector);

    public partial async Task<string?> GetAppCheckTokenAsync()
    {
        try
        {
            Debug.WriteLine("[AppCheck] iOS: GetAppCheckTokenAsync called - AppCheckService.ios.cs:34");
            
            // Check cache first
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _cachedTokenExpiry)
            {
                Debug.WriteLine($"[AppCheck] iOS: Returning cached token - AppCheckService.ios.cs:38");
                return _cachedToken;
            }

            // Retrieve token from Firebase SDK on main thread
            return await MainThread.InvokeOnMainThreadAsync(() =>
            {
                return GetTokenFromFirebaseSDK();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AppCheck] iOS exception: {ex.GetType().Name}: {ex.Message} - AppCheckService.ios.cs:49");
            return null;
        }
    }

    /// <summary>
    /// Calls native Firebase App Check SDK via P/Invoke: [FIRAppCheck appCheck].getToken()
    /// </summary>
    private static string? GetTokenFromFirebaseSDK()
    {
        try
        {
            Debug.WriteLine("[AppCheck] iOS: Attempting Firebase SDK call via P/Invoke - AppCheckService.ios.cs:60");

            // Get FIRAppCheck class pointer
            var appCheckClassName = Marshal.StringToHGlobalAnsi("FIRAppCheck");
            var appCheckClass = objc_getClass("FIRAppCheck");
            Marshal.FreeHGlobal(appCheckClassName);
            
            if (appCheckClass == IntPtr.Zero)
            {
                Debug.WriteLine("[AppCheck] iOS: FIRAppCheck class not found - ensure Firebase SDK is properly installed - AppCheckService.ios.cs:68");
                return null;
            }

            // Get [FIRAppCheck appCheck] class method
            var appCheckSel = sel_getUid("appCheck");
            var appCheckInstance = objc_msgSend_IntPtr(appCheckClass, appCheckSel);
            
            if (appCheckInstance == IntPtr.Zero)
            {
                Debug.WriteLine("[AppCheck] iOS: FIRAppCheck singleton not available - AppCheckService.ios.cs:78");
                return null;
            }

            // Try to get token via getToken() method (synchronous, may be empty)
            var getTokenSel = sel_getUid("getToken");
            var tokenObj = objc_msgSend_NSString(appCheckInstance, getTokenSel);
            
            if (tokenObj != IntPtr.Zero)
            {
                // Convert NSString pointer to managed string
                var nsstring = Runtime.GetNSObject<NSString>(tokenObj);
                var token = nsstring?.ToString();
                
                if (!string.IsNullOrEmpty(token))
                {
                    _cachedToken = token;
                    _cachedTokenExpiry = DateTime.UtcNow.AddSeconds(TOKEN_CACHE_SECONDS);
                    Debug.WriteLine($"[AppCheck] iOS: Successfully retrieved token (length: {token.Length}) - AppCheckService.ios.cs:95");
                    return token;
                }
            }

            Debug.WriteLine("[AppCheck] iOS: getToken() returned empty - likely need async getTokenForcingRefresh:completion: - AppCheckService.ios.cs:99");
            Debug.WriteLine("[AppCheck] iOS: Recommend implementing platform channels for full async App Check integration - AppCheckService.ios.cs:100");
            
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AppCheck] iOS SDK call failed: {ex.GetType().Name}: {ex.Message} - AppCheckService.ios.cs:104");
            Debug.WriteLine($"[AppCheck] iOS Stack trace: {ex.StackTrace} - AppCheckService.ios.cs:105");
            return null;
        }
    }
}

#endif


