using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Lender.Models;
using Lender.Services.Constants;

namespace Lender.Services;

public class AuthenticationService : IAuthenticationService
{
    // Authentication constants are centralized in AuthenticationConstants
    private bool _isAuthenticated;
    private string? _currentUserId;
    private string? _currentUserEmail;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set => SetProperty(ref _isAuthenticated, value);
    }

    public string? CurrentUserId
    {
        get => _currentUserId;
        private set => SetProperty(ref _currentUserId, value);
    }

    public string? CurrentUserEmail
    {
        get => _currentUserEmail;
        private set => SetProperty(ref _currentUserEmail, value);
    }

    public AuthenticationService()
    {
        // Initialize on creation
        InitializeAsync().ConfigureAwait(false);
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Try to restore previous session
            await RestoreSessionAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Authentication initialization failed: {ex.Message} - AuthenticationService.cs:53");
        }
    }

    public async Task<bool> SignUpAsync(string email, string password, string firstName, string lastName, string? phoneNumber = null, DateTime? dateOfBirth = null)
    {
        try
        {
            using var client = new HttpClient();
            var requestUri = $"{AuthenticationConstants.FirebaseAuthUrl}/accounts:signUp?key={AuthenticationConstants.FirebaseWebApiKey}";
            
            Debug.WriteLine($"SignUp request URI: {requestUri} - AuthenticationService.cs:64");
            Debug.WriteLine($"Email: {email}, FirstName: {firstName}, LastName: {lastName}, DateOfBirth: {dateOfBirth:yyyyMMdd}, Password length: {password.Length} - AuthenticationService.cs:65");
            
            var payload = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            Debug.WriteLine($"SignUp payload: {jsonPayload} - AuthenticationService.cs:75");
            
            var jsonContent = new StringContent(
                jsonPayload,
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(requestUri, jsonContent);
            
            Debug.WriteLine($"SignUp response status: {response.StatusCode} - AuthenticationService.cs:85");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"SignUp response content: {content} - AuthenticationService.cs:90");
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                
                if (result.TryGetProperty("idToken", out var idToken) &&
                    result.TryGetProperty("localId", out var localId) &&
                    result.TryGetProperty("email", out var userEmail))
                {
                    CurrentUserId = localId.GetString();
                    CurrentUserEmail = userEmail.GetString();
                    IsAuthenticated = true;
                    
                    string refreshToken = result.TryGetProperty("refreshToken", out var rt) ? (rt.GetString() ?? "") : "";
                    await SaveAuthDataAsync(
                        idToken.GetString() ?? "",
                        localId.GetString() ?? "",
                        userEmail.GetString() ?? "",
                        refreshToken
                    );

                    // Create user document in Firestore with complete profile
                    var newUser = new User
                    {
                        Id = localId.GetString() ?? "",
                        Email = userEmail.GetString() ?? "",
                        FullName = $"{firstName} {lastName}".Trim(),
                        PhoneNumber = phoneNumber ?? "",
                        DateOfBirth = dateOfBirth ?? DateTime.UtcNow,
                        Balance = 0,
                        Status = UserStatus.Active,
                        JoinDate = DateTime.UtcNow
                    };
                    
                    Debug.WriteLine($"[AuthenticationService] Creating user in Firestore: {newUser.Email} - AuthenticationService.cs:120");
                    try
                    {
                        var firebaseService = FirestoreService.Instance;
                        var saveSuccess = await firebaseService.SaveUserAsync(newUser);
                        
                        if (saveSuccess)
                        {
                            Debug.WriteLine($"[AuthenticationService] ✅ User {newUser.Email} successfully created in Firestore with full profile - AuthenticationService.cs:128");
                            return true;
                        }
                        else
                        {
                            Debug.WriteLine($"[AuthenticationService] ⚠️ User created in Firebase Auth but FAILED to save to Firestore! - AuthenticationService.cs:133");
                            // Still return true because Firebase Auth worked
                            return true;
                        }
                    }
                    catch (Exception firestoreEx)
                    {
                        Debug.WriteLine($"[AuthenticationService] EXCEPTION saving to Firestore: {firestoreEx.Message} - AuthenticationService.cs:140");
                        Debug.WriteLine($"[AuthenticationService] Stack: {firestoreEx.StackTrace} - AuthenticationService.cs:141");
                        return true; // Auth still succeeded
                    }
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Sign up HTTP error {response.StatusCode}: {errorContent} - AuthenticationService.cs:149");
                
                // Try to parse Firebase error message
                try
                {
                    var errorResult = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errorResult.TryGetProperty("error", out var error) && 
                        error.TryGetProperty("message", out var message))
                    {
                        Debug.WriteLine($"Firebase error message: {message.GetString()} - AuthenticationService.cs:158");
                    }
                }
                catch { }
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Sign up exception: {ex.Message} - AuthenticationService.cs:168");
            Debug.WriteLine($"Stack trace: {ex.StackTrace} - AuthenticationService.cs:169");
            return false;
        }
    }

    public async Task<bool> SignInAsync(string email, string password)
    {
        try
        {
            using var client = new HttpClient();
            var requestUri = $"{AuthenticationConstants.FirebaseAuthUrl}/accounts:signInWithPassword?key={AuthenticationConstants.FirebaseWebApiKey}";
            
            var payload = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(requestUri, jsonContent);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                
                if (result.TryGetProperty("idToken", out var idToken) &&
                    result.TryGetProperty("localId", out var localId) &&
                    result.TryGetProperty("email", out var userEmail))
                {
                    CurrentUserId = localId.GetString();
                    CurrentUserEmail = userEmail.GetString();
                    IsAuthenticated = true;
                    
                    string refreshToken = result.TryGetProperty("refreshToken", out var rt) ? (rt.GetString() ?? "") : "";
                    await SaveAuthDataAsync(
                        idToken.GetString() ?? "",
                        localId.GetString() ?? "",
                        userEmail.GetString() ?? "",
                        refreshToken
                    );
                    
                    return true;
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Sign in error: {error} - AuthenticationService.cs:221");
            }

            IsAuthenticated = false;
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Sign in exception: {ex.Message} - AuthenticationService.cs:229");
            return false;
        }
    }

    public async Task<bool> SignInWithGoogleAsync()
    {
        try
        {
            Debug.WriteLine("Starting native Google SignIn - AuthenticationService.cs:238");
            
            var result = await GoogleSignInService.SignInAsync();
            
            if (result != null && !string.IsNullOrEmpty(result.IdToken))
            {
                Debug.WriteLine($"Got Google ID token for: {result.Email} - AuthenticationService.cs:244");
                return await SignInWithGoogleIdToken(result.IdToken);
            }
            else
            {
                Debug.WriteLine("Google SignIn returned null or no token - AuthenticationService.cs:249");
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Google sign in exception: {ex.Message} - AuthenticationService.cs:256");
            Debug.WriteLine($"Stack trace: {ex.StackTrace} - AuthenticationService.cs:257");
            return false;
        }
    }

    private async Task<string?> ExchangeGoogleCodeForFirebaseToken(string authCode, string redirectUri)
    {
        try
        {
            Debug.WriteLine($"Exchanging auth code for token... - AuthenticationService.cs:266");
            
            // Exchange code for tokens with Google
            using var client = new HttpClient();
            var tokenUrl = "https://oauth2.googleapis.com/token";
            
            var parameters = new Dictionary<string, string>
            {
                { "code", authCode },
                { "client_id", AuthenticationConstants.GoogleClientId },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(tokenUrl, content);
            
            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Token exchange response: {response.StatusCode}  {json} - AuthenticationService.cs:284");
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(json);
                
                if (result.TryGetProperty("id_token", out var idToken))
                {
                    Debug.WriteLine("Successfully got ID token - AuthenticationService.cs:292");
                    return idToken.GetString();
                }
                else
                {
                    Debug.WriteLine("No id_token in response - AuthenticationService.cs:297");
                }
            }
            else
            {
                Debug.WriteLine($"Token exchange failed with status: {response.StatusCode} - AuthenticationService.cs:302");
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Token exchange exception: {ex.Message} - AuthenticationService.cs:309");
            return null;
        }
    }

    private async Task<bool> SignInWithGoogleIdToken(string idToken)
    {
        try
        {
            using var client = new HttpClient();
            var requestUri = $"{AuthenticationConstants.FirebaseAuthUrl}/accounts:signInWithIdp?key={AuthenticationConstants.FirebaseWebApiKey}";
            
            var payload = new
            {
                postBody = $"id_token={idToken}&providerId=google.com",
                requestUri = "http://localhost",
                returnSecureToken = true,
                returnIdpCredential = true
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(requestUri, jsonContent);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                
                if (result.TryGetProperty("idToken", out var firebaseToken) &&
                    result.TryGetProperty("localId", out var localId) &&
                    result.TryGetProperty("email", out var userEmail))
                {
                    CurrentUserId = localId.GetString();
                    CurrentUserEmail = userEmail.GetString();
                    IsAuthenticated = true;
                    
                    string refreshToken = result.TryGetProperty("refreshToken", out var rt) ? (rt.GetString() ?? "") : "";
                    await SaveAuthDataAsync(
                        firebaseToken.GetString() ?? "",
                        localId.GetString() ?? "",
                        userEmail.GetString() ?? "",
                        refreshToken
                    );
                    
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Firebase Google sign in failed: {ex.Message} - AuthenticationService.cs:364");
            return false;
        }
    }

    public async Task SignOutAsync()
    {
        try
        {
            // Clear in-memory state
            CurrentUserId = null;
            CurrentUserEmail = null;
            IsAuthenticated = false;
            
            // Clear all SecureStorage data (tokens, user ID, email, etc.)
            SecureStorage.Remove(AuthenticationConstants.FirebaseTokenKey);
            SecureStorage.Remove(AuthenticationConstants.FirebaseUserIdKey);
            SecureStorage.Remove(AuthenticationConstants.FirebaseUserEmailKey);
            
            // Clear all SecureStorage to ensure nothing is left behind
            SecureStorage.RemoveAll();
            
            // Clear any preferences/settings
            Preferences.Clear();
            
            Debug.WriteLine("[AuthenticationService] ✅ Sign out completed  all auth data cleared - AuthenticationService.cs:389");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AuthenticationService] ❌ Sign out failed: {ex.Message} - AuthenticationService.cs:393");
            Debug.WriteLine($"[AuthenticationService] Stack trace: {ex.StackTrace} - AuthenticationService.cs:394");
        }
    }

    public async Task<bool> RestoreSessionAsync()
    {
        try
        {
            var token = SecureStorage.GetAsync(AuthenticationConstants.FirebaseTokenKey).Result;
            var userId = SecureStorage.GetAsync(AuthenticationConstants.FirebaseUserIdKey).Result;
            var email = SecureStorage.GetAsync(AuthenticationConstants.FirebaseUserEmailKey).Result;

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
            {
                CurrentUserId = userId;
                CurrentUserEmail = email;
                IsAuthenticated = true;
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Session restoration failed: {ex.Message} - AuthenticationService.cs:416");
        }

        return false;
    }

    private async Task SaveAuthDataAsync(string token, string userId, string email, string refreshToken)
    {
        try
        {
            await SecureStorage.SetAsync(AuthenticationConstants.FirebaseTokenKey, token);
            await SecureStorage.SetAsync(AuthenticationConstants.FirebaseUserIdKey, userId);
            await SecureStorage.SetAsync(AuthenticationConstants.FirebaseUserEmailKey, email);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await SecureStorage.SetAsync(AuthenticationConstants.FirebaseRefreshTokenKey, refreshToken);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save auth data: {ex.Message} - AuthenticationService.cs:432");
        }
    }

    public async Task<string?> GetValidIdTokenAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync(AuthenticationConstants.FirebaseTokenKey);
            var refreshToken = await SecureStorage.GetAsync(AuthenticationConstants.FirebaseRefreshTokenKey);

            // If we don't have a token but have a refresh token, refresh
            if (string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(refreshToken))
            {
                return await RefreshIdTokenAsync(refreshToken);
            }

            // Optionally: decode JWT to check expiry; for simplicity, refresh if we have a refresh token
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                // Proactively refresh to avoid 401/403 due to expiry
                var newToken = await RefreshIdTokenAsync(refreshToken);
                if (!string.IsNullOrWhiteSpace(newToken))
                {
                    return newToken;
                }
            }
            return token;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetValidIdToken failed: {ex.Message} - AuthenticationService.cs:XXX");
            return await SecureStorage.GetAsync(AuthenticationConstants.FirebaseTokenKey);
        }
    }

    private async Task<string?> RefreshIdTokenAsync(string refreshToken)
    {
        try
        {
            using var client = new HttpClient();
            var url = $"{AuthenticationConstants.FirebaseSecureTokenUrl}?key={AuthenticationConstants.FirebaseWebApiKey}";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            });

            var response = await client.PostAsync(url, content);
            var body = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Refresh token response: {response.StatusCode} {body} - AuthenticationService.cs:YYY");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = JsonSerializer.Deserialize<JsonElement>(body);
            var newIdToken = json.TryGetProperty("id_token", out var idt) ? idt.GetString() : null;
            var newRefreshToken = json.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
            var userId = json.TryGetProperty("user_id", out var uid) ? uid.GetString() : CurrentUserId;

            if (!string.IsNullOrWhiteSpace(newIdToken))
            {
                await SecureStorage.SetAsync(AuthenticationConstants.FirebaseTokenKey, newIdToken);
            }
            if (!string.IsNullOrWhiteSpace(newRefreshToken))
            {
                await SecureStorage.SetAsync(AuthenticationConstants.FirebaseRefreshTokenKey, newRefreshToken);
            }
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await SecureStorage.SetAsync(AuthenticationConstants.FirebaseUserIdKey, userId);
            }

            return newIdToken;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RefreshIdToken exception: {ex.Message} - AuthenticationService.cs:ZZZ");
            return null;
        }
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            if (string.IsNullOrEmpty(CurrentUserEmail))
                return false;

            using var client = new HttpClient();
            
            // First, re-authenticate with current password
            var signInUri = $"{AuthenticationConstants.FirebaseAuthUrl}/accounts:signInWithPassword?key={AuthenticationConstants.FirebaseWebApiKey}";
            var signInPayload = new
            {
                email = CurrentUserEmail,
                password = currentPassword,
                returnSecureToken = true
            };

            var signInContent = new StringContent(
                JsonSerializer.Serialize(signInPayload),
                Encoding.UTF8,
                "application/json"
            );

            var signInResponse = await client.PostAsync(signInUri, signInContent);
            
            if (!signInResponse.IsSuccessStatusCode)
            {
                Debug.WriteLine("Reauthentication failed  invalid current password - AuthenticationService.cs:473");
                return false;
            }

            var signInResult = JsonSerializer.Deserialize<JsonElement>(
                await signInResponse.Content.ReadAsStringAsync());
            
            if (!signInResult.TryGetProperty("idToken", out var idToken))
                return false;

            // Now update password
            var updateUri = $"{AuthenticationConstants.FirebaseAuthUrl}/accounts:update?key={AuthenticationConstants.FirebaseWebApiKey}";
            var updatePayload = new
            {
                idToken = idToken.GetString(),
                password = newPassword,
                returnSecureToken = true
            };

            var updateContent = new StringContent(
                JsonSerializer.Serialize(updatePayload),
                Encoding.UTF8,
                "application/json"
            );

            var updateResponse = await client.PostAsync(updateUri, updateContent);
            
            if (updateResponse.IsSuccessStatusCode)
            {
                var updateResult = JsonSerializer.Deserialize<JsonElement>(
                    await updateResponse.Content.ReadAsStringAsync());
                
                if (updateResult.TryGetProperty("idToken", out var newToken))
                {
                    // Save new token
                    await SecureStorage.SetAsync(AuthenticationConstants.FirebaseTokenKey, newToken.GetString() ?? "");
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Change password failed: {ex.Message} - AuthenticationService.cs:517");
            return false;
        }
    }

    public async Task<bool> ChangeEmailAsync(string newEmail)
    {
        try
        {
            if (string.IsNullOrEmpty(CurrentUserEmail))
                return false;

            using var client = new HttpClient();
            var token = await SecureStorage.GetAsync(AuthenticationConstants.FirebaseTokenKey);
            
            if (string.IsNullOrEmpty(token))
                return false;

            var updateUri = $"{AuthenticationConstants.FirebaseAuthUrl}/accounts:update?key={AuthenticationConstants.FirebaseWebApiKey}";
            var updatePayload = new
            {
                idToken = token,
                email = newEmail,
                returnSecureToken = true
            };

            var updateContent = new StringContent(
                JsonSerializer.Serialize(updatePayload),
                Encoding.UTF8,
                "application/json"
            );

            var updateResponse = await client.PostAsync(updateUri, updateContent);
            
            if (updateResponse.IsSuccessStatusCode)
            {
                var updateResult = JsonSerializer.Deserialize<JsonElement>(
                    await updateResponse.Content.ReadAsStringAsync());
                
                if (updateResult.TryGetProperty("idToken", out var newToken) &&
                    updateResult.TryGetProperty("email", out var email))
                {
                    // Update stored auth data
                    await SecureStorage.SetAsync(AuthenticationConstants.FirebaseTokenKey, newToken.GetString() ?? "");
                    await SecureStorage.SetAsync(AuthenticationConstants.FirebaseUserEmailKey, email.GetString() ?? "");
                    CurrentUserEmail = email.GetString();
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Change email failed: {ex.Message} - AuthenticationService.cs:571");
            return false;
        }
    }

    public async Task<bool> DeleteAccountAsync(string password)
    {
        try
        {
            if (string.IsNullOrEmpty(CurrentUserEmail))
                return false;

            using var client = new HttpClient();
            
            // First, re-authenticate with password
            var signInUri = $"{AuthenticationConstants.FirebaseAuthUrl}/accounts:signInWithPassword?key={AuthenticationConstants.FirebaseWebApiKey}";
            var signInPayload = new
            {
                email = CurrentUserEmail,
                password,
                returnSecureToken = true
            };

            var signInContent = new StringContent(
                JsonSerializer.Serialize(signInPayload),
                Encoding.UTF8,
                "application/json"
            );

            var signInResponse = await client.PostAsync(signInUri, signInContent);
            
            if (!signInResponse.IsSuccessStatusCode)
            {
                Debug.WriteLine("Reauthentication failed  invalid password - AuthenticationService.cs:604");
                return false;
            }

            var signInResult = JsonSerializer.Deserialize<JsonElement>(
                await signInResponse.Content.ReadAsStringAsync());
            
            if (!signInResult.TryGetProperty("idToken", out var idToken))
                return false;

            // Now delete account
            var deleteUri = $"{AuthenticationConstants.FirebaseAuthUrl}/accounts:delete?key={AuthenticationConstants.FirebaseWebApiKey}";
            var deletePayload = new
            {
                idToken = idToken.GetString()
            };

            var deleteContent = new StringContent(
                JsonSerializer.Serialize(deletePayload),
                Encoding.UTF8,
                "application/json"
            );

            var deleteResponse = await client.PostAsync(deleteUri, deleteContent);
            
            if (deleteResponse.IsSuccessStatusCode)
            {
                // Clear auth data
                await SignOutAsync();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Delete account failed: {ex.Message} - AuthenticationService.cs:640");
            return false;
        }
    }
}
