# Lender - iOS Peer-to-Peer Lending Application

.NET MAUI application for iOS with Firebase backend.

## Quick Start

```bash
dotnet run -f net10.0-ios
```

## Features

### ✅ User Authentication
- Firebase email/password authentication
- Sign up with complete user profile
- Secure credential storage

### ✅ User Profile Collection
- First Name, Last Name
- Date of Birth (with native iOS picker)
- Phone Number (optional)
- Email & Password

### ✅ Firestore Database
- **Users Collection** - Complete user profiles with financial metrics
- **Loans Collection** - Peer-to-peer loan requests (ready for development)
- **Transactions Collection** - Financial movement tracking (ready)
- **Investments Collection** - Investor positions (ready)
- **Budgets Collection** - Spending management (ready)

## Firebase Configuration

- **Project ID:** lender-d0412
- **API Key:** AIzaSyBiRfWl6FILfLl2-jMv0ENpQFVNH2YYwLI
- **Auth:** identitytoolkit.googleapis.com/v1
- **Firestore:** firestore.googleapis.com/v1

## Project Structure

- **Scripts/** - All app code
  - **ViewModels/** - LoginViewModel, DashboardViewModel
  - **Services/** - AuthenticationService, FirestoreService
  - **Models/** - User, LoanRequest, Transaction, Budget, LoanInvestment
  - **Converters/** - ValueConverters
- **Platforms/iOS/** - iOS-specific code
- **Resources/** - Fonts, images, styles

## Recent Changes

### December 30, 2025
- Enhanced Firestore logging with detailed debug output
- Improved error handling for user sign-up
- Auto-creation of user documents in Firestore
- Added comprehensive profile data collection (first name, last name, DOB, phone)
- Implemented native iOS date picker
- Added age validation (13-120 years)
