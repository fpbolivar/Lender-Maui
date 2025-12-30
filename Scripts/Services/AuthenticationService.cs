using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Lender.Models;

namespace Lender.Services;

public interface IAuthenticationService : INotifyPropertyChanged
{
    bool IsAuthenticated { get; }
    string? CurrentUserId { get; }
    string? CurrentUserEmail { get; }
    Task<bool> SignUpAsync(string email, string password, string firstName, string lastName, string? phoneNumber = null);
    Task<bool> SignInAsync(string email, string password);
    Task<bool> SignInWithGoogleAsync();
    Task SignOutAsync();
    Task<bool> RestoreSessionAsync();
}

public class AuthenticationService : IAuthenticationService
{
    private const string FirebaseWebApiKey = "AIzaSyBiRfWl6FILfLl2-jMv0ENpQFVNH2YYwLI"; // Firebase Web API Key from GoogleService-Info.plist
    private const string GoogleClientId = "225480243312-e8p7119p1holglcu3a2n6gtrbvr8jbvb.apps.googleusercontent.com"; // iOS Client ID
    private const string ReversedClientId = "com.googleusercontent.apps.225480243312-e8p7119p1holglcu3a2n6gtrbvr8jbvb";
    private const string FirebaseAuthUrl = "https://identitytoolkit.googleapis.com/v1";
    private const string FirebaseProjectId = "lender-d0412";
    private const string FirebaseTokenKey = "firebase_auth_token";
    private const string FirebaseUserIdKey = "firebase_user_id";
    private const string FirebaseUserEmailKey = "firebase_user_email";

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

    public async Task<bool> SignUpAsync(string email, string password, string firstName, string lastName, string? phoneNumber = null)
    {
        try
        {
            using var client = new HttpClient();
            var requestUri = $"{FirebaseAuthUrl}/accounts:signUp?key={FirebaseWebApiKey}";
            
            Debug.WriteLine($"SignUp request URI: {requestUri}");
            Debug.WriteLine($"Email: {email}, FirstName: {firstName}, LastName: {lastName}, Password length: {password.Length}");
            
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
                        Balance = 0,
                        CreditScore = 0,
                        Status = UserStatus.Active,
                        JoinDate = DateTime.UtcNow
                    };
                    
                    var firebaseService = FirestoreService.Instance;
                    await firebaseService.SaveUserAsync(newUser);
                    Debug.WriteLine($"User {newUser.Email} created in Firestore with full profile");
                    
                    return true;
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
            var requestUri = $"{FirebaseAuthUrl}/accounts:signInWithPassword?key={FirebaseWebApiKey}";
            
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
                { "client_id", GoogleClientId },
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
            var requestUri = $"{FirebaseAuthUrl}/accounts:signInWithIdp?key={FirebaseWebApiKey}";
            
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
            SecureStorage.Remove(FirebaseTokenKey);
            SecureStorage.Remove(FirebaseUserIdKey);
            SecureStorage.Remove(FirebaseUserEmailKey);
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
            var token = SecureStorage.GetAsync(FirebaseTokenKey).Result;
            var userId = SecureStorage.GetAsync(FirebaseUserIdKey).Result;
            var email = SecureStorage.GetAsync(FirebaseUserEmailKey).Result;

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
            await SecureStorage.SetAsync(FirebaseTokenKey, token);
            await SecureStorage.SetAsync(FirebaseUserIdKey, userId);
            await SecureStorage.SetAsync(FirebaseUserEmailKey, email);
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
}
