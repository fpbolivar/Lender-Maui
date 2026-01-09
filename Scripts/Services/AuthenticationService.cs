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
            Debug.WriteLine($"Authentication initialization failed: {ex.Message} - AuthenticationService.cs:71");
        }
    }

    public async Task<bool> SignUpAsync(string email, string password, string firstName, string lastName, string? phoneNumber = null, DateTime? dateOfBirth = null)
    {
        try
        {
            using var client = new HttpClient();
            var requestUri = $"{AuthenticationConstants.FirebaseAuthUrl}/accounts:signUp?key={AuthenticationConstants.FirebaseWebApiKey}";
            
            Debug.WriteLine($"SignUp request URI: {requestUri}");
            Debug.WriteLine($"Email: {email}, FirstName: {firstName}, LastName: {lastName}, DateOfBirth: {dateOfBirth:yyyy-MM-dd}, Password length: {password.Length}");
            
            var payload = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            Debug.WriteLine($"SignUp payload: {jsonPayload}");
            
            var jsonContent = new StringContent(
                jsonPayload,
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(requestUri, jsonContent);
            
            Debug.WriteLine($"SignUp response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"SignUp response content: {content}");
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                
                if (result.TryGetProperty("idToken", out var idToken) &&
                    result.TryGetProperty("localId", out var localId) &&
                    result.TryGetProperty("email", out var userEmail))
                {
                    CurrentUserId = localId.GetString();
                    CurrentUserEmail = userEmail.GetString();
                    IsAuthenticated = true;
                    
                    await SaveAuthDataAsync(
                        idToken.GetString() ?? "",
                        localId.GetString() ?? "",
                        userEmail.GetString() ?? ""
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
                    
                    Debug.WriteLine($"[AuthenticationService] Creating user in Firestore: {newUser.Email}");
                    try
                    {
                        var firebaseService = FirestoreService.Instance;
                        var saveSuccess = await firebaseService.SaveUserAsync(newUser);
                        
                        if (saveSuccess)
                        {
                            Debug.WriteLine($"[AuthenticationService] ✅ User {newUser.Email} successfully created in Firestore with full profile");
                            return true;
                        }
                        else
                        {
                            Debug.WriteLine($"[AuthenticationService] ⚠️ User created in Firebase Auth but FAILED to save to Firestore!");
                            // Still return true because Firebase Auth worked
                            return true;
                        }
                    }
                    catch (Exception firestoreEx)
                    {
                        Debug.WriteLine($"[AuthenticationService] EXCEPTION saving to Firestore: {firestoreEx.Message}");
                        Debug.WriteLine($"[AuthenticationService] Stack: {firestoreEx.StackTrace}");
                        return true; // Auth still succeeded
                    }
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Sign up HTTP error {response.StatusCode}: {errorContent}");
                
                // Try to parse Firebase error message
                try
                {
                    var errorResult = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errorResult.TryGetProperty("error", out var error) && 
                        error.TryGetProperty("message", out var message))
                    {
                        Debug.WriteLine($"Firebase error message: {message.GetString()}");
                    }
                }
                catch { }
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Sign up exception: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
                    
                    await SaveAuthDataAsync(
                        idToken.GetString() ?? "",
                        localId.GetString() ?? "",
                        userEmail.GetString() ?? ""
                    );
                    
                    return true;
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Sign in error: {error} - AuthenticationService.cs:181");
            }

            IsAuthenticated = false;
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Sign in exception: {ex.Message} - AuthenticationService.cs:189");
            return false;
        }
    }

    public async Task<bool> SignInWithGoogleAsync()
    {
        try
        {
            Debug.WriteLine("Starting native Google SignIn - AuthenticationService.cs:198");
            
            var result = await GoogleSignInService.SignInAsync();
            
            if (result != null && !string.IsNullOrEmpty(result.IdToken))
            {
                Debug.WriteLine($"Got Google ID token for: {result.Email} - AuthenticationService.cs:204");
                return await SignInWithGoogleIdToken(result.IdToken);
            }
            else
            {
                Debug.WriteLine("Google SignIn returned null or no token - AuthenticationService.cs:209");
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Google sign in exception: {ex.Message} - AuthenticationService.cs:216");
            Debug.WriteLine($"Stack trace: {ex.StackTrace} - AuthenticationService.cs:217");
            return false;
        }
    }

    private async Task<string?> ExchangeGoogleCodeForFirebaseToken(string authCode, string redirectUri)
    {
        try
        {
            Debug.WriteLine($"Exchanging auth code for token... - AuthenticationService.cs:226");
            
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
            Debug.WriteLine($"Token exchange response: {response.StatusCode}  {json} - AuthenticationService.cs:244");
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(json);
                
                if (result.TryGetProperty("id_token", out var idToken))
                {
                    Debug.WriteLine("Successfully got ID token - AuthenticationService.cs:252");
                    return idToken.GetString();
                }
                else
                {
                    Debug.WriteLine("No id_token in response - AuthenticationService.cs:257");
                }
            }
            else
            {
                Debug.WriteLine($"Token exchange failed with status: {response.StatusCode} - AuthenticationService.cs:262");
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Token exchange exception: {ex.Message} - AuthenticationService.cs:269");
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
                    
                    await SaveAuthDataAsync(
                        firebaseToken.GetString() ?? "",
                        localId.GetString() ?? "",
                        userEmail.GetString() ?? ""
                    );
                    
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Firebase Google sign in failed: {ex.Message} - AuthenticationService.cs:324");
            return false;
        }
    }

    public async Task SignOutAsync()
    {
        try
        {
            CurrentUserId = null;
            CurrentUserEmail = null;
            IsAuthenticated = false;
            
            // Clear saved auth data
            SecureStorage.Remove(AuthenticationConstants.FirebaseTokenKey);
            SecureStorage.Remove(AuthenticationConstants.FirebaseUserIdKey);
            SecureStorage.Remove(AuthenticationConstants.FirebaseUserEmailKey);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Sign out failed: {ex.Message} - AuthenticationService.cs:344");
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
            Debug.WriteLine($"Session restoration failed: {ex.Message} - AuthenticationService.cs:366");
        }

        return false;
    }

    private async Task SaveAuthDataAsync(string token, string userId, string email)
    {
        try
        {
            await SecureStorage.SetAsync(AuthenticationConstants.FirebaseTokenKey, token);
            await SecureStorage.SetAsync(AuthenticationConstants.FirebaseUserIdKey, userId);
            await SecureStorage.SetAsync(AuthenticationConstants.FirebaseUserEmailKey, email);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save auth data: {ex.Message} - AuthenticationService.cs:382");
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
                Debug.WriteLine("Re-authentication failed - invalid current password");
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
            Debug.WriteLine($"Change password failed: {ex.Message}");
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
            Debug.WriteLine($"Change email failed: {ex.Message}");
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
                Debug.WriteLine("Re-authentication failed - invalid password");
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
            Debug.WriteLine($"Delete account failed: {ex.Message}");
            return false;
        }
    }
}
