# Updates

## 2025-12-30
- Added detailed Firestore REST logging for easier debugging and payload inspection.
- Auto-create user documents on sign-up using email as the document ID.
- Expanded profile capture: first name, last name, phone (optional), date of birth.
- Added age validation (13-120 years) and native iOS date picker support.
- Hardened sign-up error handling and status messaging.

## 2025-12-15 — Authentication System
- Email/Password sign-up and sign-in with Firebase REST APIs.
- Automatic session restoration and secure token storage (Keychain).
- Sign-out flow and basic error handling.

### Related Files
- Scripts/Services/AuthenticationService.cs — Auth REST calls, token handling
- Scripts/ViewModels/LoginViewModel.cs — Login logic
- Scripts/LoginPage.xaml — Login UI
- Scripts/MauiProgram.cs — Service registration
- Scripts/AppShell.xaml — Auth routing
- Scripts/ViewModels/DashboardViewModel.cs — Sign-out

### Quick Test Flow
1) Sign up with email/password and full profile
2) Close/reopen app to confirm session restore
3) Sign out

