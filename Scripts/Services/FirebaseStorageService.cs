using System.Text.Json;
using Lender.Services.Constants;
using Lender.Helpers;
using Lender.Services.Interfaces;

namespace Lender.Services;

public class FirebaseStorageService
{
    // Use static HttpClient for better performance and to avoid socket exhaustion
    private static readonly HttpClient _httpClient = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    /// <summary>
    /// Reconstructs the full storage path from a user email and image ID.
    /// Expected format: users/{userEmail}/collateral/{imageId}
    /// </summary>
    public static string BuildStoragePath(string userEmail, string imageId)
    {
        return $"users/{userEmail}/collateral/{imageId}";
    }

    public async Task<string?> UploadImageAsync(byte[] bytes, string objectName, string contentType = "image/jpeg")
    {
        // Try with current token, retry once with fresh token if 403
        for (int attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                // Check network connectivity first
                var current = Connectivity.Current;
                System.Diagnostics.Debug.WriteLine($"Network Status: {current.NetworkAccess} - FirebaseStorageService.cs:34");
                
                if (current.NetworkAccess != NetworkAccess.Internet)
                {
                    System.Diagnostics.Debug.WriteLine($"Firebase Storage upload failed: No internet connection - FirebaseStorageService.cs:38");
                    throw new HttpRequestException("No internet connection. Please check your network and try again.");
                }

                // Get token - force refresh on retry attempt
                var authService = ServiceHelper.GetService<IAuthenticationService>();
                string? token = null;
                
                if (authService is AuthenticationService realAuth)
                {
                    if (attempt == 1)
                    {
                        // Force token refresh on retry
                        var refreshToken = await SecureStorage.GetAsync(AuthenticationConstants.FirebaseRefreshTokenKey);
                        if (!string.IsNullOrWhiteSpace(refreshToken))
                        {
                            System.Diagnostics.Debug.WriteLine($"[Storage] Forcing token refresh on upload retry - FirebaseStorageService.cs:54");
                            token = await realAuth.RefreshTokenExplicitly(refreshToken);
                        }
                    }
                    else
                    {
                        token = await realAuth.GetValidIdTokenAsync();
                    }
                    System.Diagnostics.Debug.WriteLine($"[Storage] Token obtained from AuthenticationService (attempt {attempt + 1}) - FirebaseStorageService.cs:62");
                }
                else
                {
                    token = await SecureStorage.GetAsync(AuthenticationConstants.FirebaseTokenKey);
                    System.Diagnostics.Debug.WriteLine($"[Storage] Token obtained from SecureStorage - FirebaseStorageService.cs:67");
                }
                
                if (string.IsNullOrWhiteSpace(token))
                {
                    System.Diagnostics.Debug.WriteLine($"Firebase Storage upload failed: No token available - FirebaseStorageService.cs:72");
                    throw new InvalidOperationException("Not authenticated. Please sign in to upload collateral.");
                }
                
                System.Diagnostics.Debug.WriteLine($"[Storage] Token length: {token.Length}, first 30 chars: {token.Substring(0, Math.Min(30, token.Length))} - FirebaseStorageService.cs:76");

                // Use Firebase Storage REST API (accepts Firebase Auth ID token)
                var url = $"https://firebasestorage.googleapis.com/v0/b/{StorageConstants.Bucket}/o?uploadType=media&name={Uri.EscapeDataString(objectName)}";

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new ByteArrayContent(bytes)
                };
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                // Add App Check token if available
                var appCheckService = ServiceHelper.GetService<IAppCheckService>();
                if (appCheckService != null)
                {
                    try
                    {
                        var appCheckToken = await appCheckService.GetAppCheckTokenAsync();
                        if (!string.IsNullOrWhiteSpace(appCheckToken))
                        {
                            request.Headers.Add("X-Firebase-AppCheck", appCheckToken);
                            System.Diagnostics.Debug.WriteLine($"[Storage] App Check token added to request - FirebaseStorageService.cs:98");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[Storage] App Check token unavailable - FirebaseStorageService.cs:102");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Storage] Failed to get App Check token: {ex.Message} - FirebaseStorageService.cs:107");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"[Storage] Authorization header set, URL: {url} - FirebaseStorageService.cs:111");
                System.Diagnostics.Debug.WriteLine($"Firebase Storage URL: {url} - FirebaseStorageService.cs:112");
                System.Diagnostics.Debug.WriteLine($"Sending request to Firebase Storage - FirebaseStorageService.cs:113");

                var response = await _httpClient.SendAsync(request);
                System.Diagnostics.Debug.WriteLine($"Firebase Storage response received: {response.StatusCode} - FirebaseStorageService.cs:116");
                
                // If 403 and first attempt, retry with fresh token
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden && attempt == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[Storage] Got 403 on upload, retrying with fresh token - FirebaseStorageService.cs:121");
                    continue;
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Firebase Storage upload error: {response.StatusCode} {err} - FirebaseStorageService.cs:128");
                    System.Diagnostics.Debug.WriteLine($"[Storage] Full error response body: {err} - FirebaseStorageService.cs:129");
                    var reason = response.ReasonPhrase ?? "Unknown";
                    throw new HttpRequestException($"Storage upload failed ({(int)response.StatusCode} {reason}). Details: {err}");
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Firebase Storage upload response: {json} - FirebaseStorageService.cs:135");
                var doc = JsonSerializer.Deserialize<JsonElement>(json);
                if (doc.TryGetProperty("name", out var nameProp))
                {
                    var name = nameProp.GetString();
                    System.Diagnostics.Debug.WriteLine($"Firebase Storage upload success: {name} - FirebaseStorageService.cs:140");
                    return name;
                }
                System.Diagnostics.Debug.WriteLine($"Firebase Storage upload success (no name in response, returning objectName): {objectName} - FirebaseStorageService.cs:143");
                return objectName;
            }
            catch (HttpRequestException)
            {
                // Don't retry on HTTP exceptions (network issues, server errors)
                throw;
            }
            catch (InvalidOperationException)
            {
                // Don't retry on auth issues
                throw;
            }
            catch (Exception ex)
            {
                string exceptionType = ex.GetType().Name;
                System.Diagnostics.Debug.WriteLine($"Firebase Storage upload exception [{exceptionType}]: {ex.Message} - FirebaseStorageService.cs:159");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message} - FirebaseStorageService.cs:162");
                }
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace} - FirebaseStorageService.cs:164");
                throw;
            }
        }
        
        // Should not reach here, but just in case
        throw new InvalidOperationException("Upload failed after retry");
    }

    public async Task<(byte[]? bytes, string? error)> GetImageBytesAsync(string objectName)
    {
        // Try with current token, retry once with fresh token if 403
        for (int attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                var authService = ServiceHelper.GetService<IAuthenticationService>();
                string? token = null;
                
                if (authService is AuthenticationService realAuth)
                {
                    if (attempt == 1)
                    {
                        // Force token refresh on retry
                        var refreshToken = await SecureStorage.GetAsync(AuthenticationConstants.FirebaseRefreshTokenKey);
                        if (!string.IsNullOrWhiteSpace(refreshToken))
                        {
                            System.Diagnostics.Debug.WriteLine($"[Storage] Forcing token refresh on retry - FirebaseStorageService.cs:191");
                            token = await realAuth.RefreshTokenExplicitly(refreshToken);
                        }
                    }
                    else
                    {
                        token = await realAuth.GetValidIdTokenAsync();
                    }
                    System.Diagnostics.Debug.WriteLine($"[Storage] Token obtained from AuthenticationService (attempt {attempt + 1}) - FirebaseStorageService.cs:199");
                }
                else
                {
                    token = await SecureStorage.GetAsync(AuthenticationConstants.FirebaseTokenKey);
                    System.Diagnostics.Debug.WriteLine($"[Storage] Token obtained from SecureStorage - FirebaseStorageService.cs:204");
                }
                
                if (string.IsNullOrWhiteSpace(token))
                {
                    System.Diagnostics.Debug.WriteLine($"Firebase Storage download failed: No token available - FirebaseStorageService.cs:209");
                    return (null, "Not authenticated. Please sign in to download.");
                }

                System.Diagnostics.Debug.WriteLine($"Firebase Storage downloading: {objectName} - FirebaseStorageService.cs:213");

                // Use Firebase Storage REST API for downloads
                var url = $"https://firebasestorage.googleapis.com/v0/b/{StorageConstants.Bucket}/o/{Uri.EscapeDataString(objectName)}?alt=media";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                System.Diagnostics.Debug.WriteLine($"Firebase Storage URL: {url} - FirebaseStorageService.cs:220");

                var response = await _httpClient.SendAsync(request);
                
                // If 403 and first attempt, retry with fresh token
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden && attempt == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[Storage] Got 403, retrying with fresh token - FirebaseStorageService.cs:227");
                    continue;
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    var errorMsg = $"Server error {(int)response.StatusCode}: {response.ReasonPhrase}";
                    System.Diagnostics.Debug.WriteLine($"Firebase Storage download error: {response.StatusCode} {err} - FirebaseStorageService.cs:235");
                    return (null, errorMsg);
                }
                
                var bytes = await response.Content.ReadAsByteArrayAsync();
                System.Diagnostics.Debug.WriteLine($"Firebase Storage download success: {bytes.Length} bytes - FirebaseStorageService.cs:240");
                return (bytes, null);
            }
            catch (Exception ex)
            {
                string exceptionType = ex.GetType().Name;
                System.Diagnostics.Debug.WriteLine($"Firebase Storage download exception [{exceptionType}]: {ex.Message} - FirebaseStorageService.cs:246");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message} - FirebaseStorageService.cs:249");
                }
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace} - FirebaseStorageService.cs:251");
                
                // Don't retry on exceptions, return error
                return (null, $"Connection error: {ex.Message}");
            }
        }
        
        // Should not reach here, but just in case
        return (null, "Download failed after retry");
    }
}
