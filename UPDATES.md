# Authentication System Update

## What's New

Firebase authentication system with email/password login and session persistence.

### Key Features
- Email/Password Sign-Up & Sign-In
- Automatic Session Restoration
- Secure Token Storage (Keychain)
- Sign-Out Functionality
- Error Handling

### Setup

1. Get Firebase Web API Key from [Firebase Console](https://console.firebase.google.com)
2. Update in `Scripts/Services/AuthenticationService.cs` (line 12):
   ```csharp
   private const string FirebaseWebApiKey = "YOUR_KEY_HERE";
   ```
3. Build: `dotnet build Lender.csproj --framework net10.0-ios`

### Files Modified

- `Scripts/Services/AuthenticationService.cs` - Firebase REST API
- `Scripts/ViewModels/LoginViewModel.cs` - Login logic  
- `Scripts/LoginPage.xaml` - Login UI
- `Scripts/MauiProgram.cs` - Service registration
- `Scripts/AppShell.xaml` - Auth routing
- `Scripts/ViewModels/DashboardViewModel.cs` - Sign out

### Test

Sign-up → Sign-in → Verify persistence → Sign-out

