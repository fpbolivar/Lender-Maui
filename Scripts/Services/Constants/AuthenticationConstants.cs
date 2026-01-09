namespace Lender.Services.Constants;

/// <summary>
/// Centralized authentication-related constants.
/// NOTE: Storing sensitive keys in source is not recommended for production. Consider using secure storage or environment variables.
/// </summary>
internal static class AuthenticationConstants
{
    public const string FirebaseWebApiKey = "AIzaSyBiRfWl6FILfLl2-jMv0ENpQFVNH2YYwLI"; // Firebase Web API Key from GoogleService-Info.plist
    public const string GoogleClientId = "225480243312-e8p7119p1holglcu3a2n6gtrbvr8jbvb.apps.googleusercontent.com"; // iOS Client ID
    public const string ReversedClientId = "com.googleusercontent.apps.225480243312-e8p7119p1holglcu3a2n6gtrbvr8jbvb";
    public const string FirebaseAuthUrl = "https://identitytoolkit.googleapis.com/v1";
    public const string FirebaseProjectId = "lender-d0412";
    public const string FirebaseTokenKey = "firebase_auth_token";
    public const string FirebaseUserIdKey = "firebase_user_id";
    public const string FirebaseUserEmailKey = "firebase_user_email";
}
