# Firestore Data Verification Checklist

## How to Verify Data is Being Saved to Firestore

### Step 1: Run the App and Check Console Logs

1. **Build and run** the app in iOS Simulator
2. **Open the Debug Console** in VS Code (View → Debug Console)
3. **Create a new account** with the sign-up form
4. **Look for these log messages:**

```
[FirestoreService] Starting SaveUserAsync for user: john@example.com (ID: xxx)
[FirestoreService] URL: https://firestore.googleapis.com/v1/projects/lender-d0412/databases/(default)/documents/users/xxx
[FirestoreService] Payload: {"fields":{...}}
[FirestoreService] Response Status: 200
[FirestoreService] Success! Response: {...}
[FirestoreService] User john@example.com saved to Firestore successfully
```

✅ **If you see these logs**, data is being sent to Firestore!

### Step 2: Check Firebase Console

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Select project: **lender-d0412**
3. Navigate to **Firestore Database**
4. Look for **Collections** panel on the left
5. Click on **users** collection
6. You should see your new user document with the Firebase UID as the document ID

**Expected user document:**
```
Document ID: firebase-uid-12345
Fields:
  ├── fullName: "John Doe"
  ├── email: "john@example.com"
  ├── phoneNumber: "+1-555-123-4567"
  ├── dateOfBirth: "1995-05-15T00:00:00Z"
  ├── balance: 0
  ├── creditScore: 0
  ├── status: "Active"
  └── joinDate: "2025-12-30T15:30:45Z"
```

### Step 3: Check Network Tab (If Available)

1. In your network inspection tool, look for requests to:
   ```
   https://firestore.googleapis.com/v1/projects/lender-d0412/databases/(default)/documents/users/{userId}
   ```

2. The request should be:
   - **Method:** PATCH
   - **Status:** 200 OK
   - **Headers:** Content-Type: application/json
   - **Body:** Contains the user data in Firestore format

### Step 4: Troubleshooting if Data is NOT Being Saved

#### Issue 1: Logs show "Error saving user: 400 Bad Request"
**Cause:** JSON format is incorrect  
**Solution:**
1. Check that all fields have proper Firestore types
2. Verify timestamps are in ISO 8601 format
3. Check that string values are properly quoted

#### Issue 2: Logs show "Error saving user: 401 Unauthorized"
**Cause:** API key is invalid  
**Solution:**
1. Get new API key from Firebase Console
2. Update in `FirestoreService.cs` line 15:
   ```csharp
   private readonly string _apiKey = "YOUR_NEW_API_KEY_HERE";
   ```
3. Rebuild and test

#### Issue 3: Logs show "Error saving user: 403 Forbidden"
**Cause:** Firestore Security Rules are blocking access  
**Solution:**
1. Go to Firebase Console → Firestore → Rules
2. Temporarily set rules to allow all (for testing only!):
   ```
   match /{document=**} {
     allow read, write: if true;
   }
   ```
3. Publish the rules and test again

#### Issue 4: No logs appear at all
**Cause:** SaveUserAsync might not be called  
**Solution:**
1. Check if SignUpAsync in AuthenticationService is being called
2. Add breakpoints and debug
3. Verify LoginViewModel is calling the right method

#### Issue 5: Logs show success but data not in Firebase Console
**Cause:** Might be saving to wrong Firebase project  
**Solution:**
1. Verify project ID in Firebase Console (should be **lender-d0412**)
2. Check FirestoreService.cs line 14:
   ```csharp
   private readonly string _projectId = "lender-d0412";
   ```
3. Verify URL in logs shows correct project ID

---

## Data Saved Per Sign-Up

When a user creates an account, the following data is automatically saved:

### User Collection (`users/{userId}`)
```
✅ fullName          - Automatically combined from FirstName + LastName
✅ email             - From Firebase Auth
✅ phoneNumber       - From optional phone field
✅ dateOfBirth       - From date picker
✅ balance           - Initialized to 0
✅ creditScore       - Initialized to 0
✅ loansGiven        - Initialized to 0
✅ loansReceived     - Initialized to 0
✅ status            - Set to "Active"
✅ isVerified        - Set to false
✅ totalLent         - Initialized to 0
✅ totalBorrowed     - Initialized to 0
✅ joinDate          - Current timestamp
✅ lastUpdated       - Current timestamp
```

### Additional Collections (Not Created Yet)
The following collections will be populated when users perform actions:

**loans** - Created when user posts a loan request  
**transactions** - Created when money is transferred  
**investments** - Created when user funds a loan  
**budgets** - Created when user sets up spending budgets  

---

## Collection Structure Summary

### ✅ users
- **When Created:** On sign-up
- **Who Can Create:** Every new user during registration
- **Purpose:** Store complete user profiles
- **Example Query:** Get all users → `db.collection('users').limit(100)`

### ⏳ loans
- **When Created:** When user posts a loan request
- **Who Can Create:** Users with verified accounts
- **Purpose:** Store loan requests and funding details
- **Relationships:** Links to users (borrower) and investments (investors)

### ⏳ transactions
- **When Created:** When money is moved (funding, repayment, transfer)
- **Who Can Create:** System (automatic when funds move)
- **Purpose:** Complete audit trail of all financial movements
- **Relationships:** Links to loans, users (sender/receiver), investments

### ⏳ investments
- **When Created:** When investor funds a loan
- **Who Can Create:** Users during loan funding process
- **Purpose:** Track investor positions and returns
- **Relationships:** Links to loans and users

### ⏳ budgets
- **When Created:** When user creates spending budget
- **Who Can Create:** Any user
- **Purpose:** Track spending by category
- **Relationships:** Links to users and transactions

---

## Data Validation Checklist

After signing up, verify each field is correct in Firebase Console:

- [ ] **fullName** matches "FirstName LastName" exactly
- [ ] **email** matches the registered email
- [ ] **phoneNumber** matches what was entered (or empty if optional)
- [ ] **dateOfBirth** is in correct ISO 8601 format (YYYY-MM-DDTHH:MM:SSZ)
- [ ] **balance** is 0
- [ ] **creditScore** is 0
- [ ] **status** is "Active"
- [ ] **joinDate** is recent (within minutes of signup)
- [ ] **lastUpdated** is same as joinDate for new accounts
- [ ] **isVerified** is false (not yet verified)
- [ ] All counters (loansGiven, loansReceived, etc.) are 0
- [ ] Document ID matches Firebase UID (not user's email)

---

## Quick Test Procedure

### To Test the Complete Sign-Up Flow:

1. **Launch the app**
   ```bash
   dotnet run -f net10.0-ios
   ```

2. **Switch to Sign Up mode** - Click "Don't have an account? Sign Up"

3. **Fill in test data:**
   - First Name: `John`
   - Last Name: `Test`
   - Date of Birth: Pick a date (e.g., May 15, 1990)
   - Phone Number: `555-123-4567` (or leave blank)
   - Email: `john.test@example.com`
   - Password: `TestPass123`

4. **Click "Create Account"**

5. **Monitor Debug Console** for Firestore logs

6. **Check Firebase Console:**
   - Go to Firestore Database
   - Click on `users` collection
   - Look for document with your Firebase UID
   - Verify all fields are present

7. **Celebrate! 🎉** Data is being saved successfully!

---

## Performance Notes

- **User creation:** ~2-3 seconds (includes Firebase Auth + Firestore write)
- **Firestore latency:** ~500-1000ms (depends on network)
- **Batch writes:** Not yet implemented (can be optimized later)

---

## Security Reminder

⚠️ **IMPORTANT:** Current Firestore Rules allow anyone to read/write!

For production, set proper security rules:
```firestore
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /users/{userId} {
      allow read: if request.auth.uid == userId;
      allow write: if request.auth.uid == userId;
    }
    
    match /loans/{document=**} {
      allow read: if request.auth != null;
      allow create: if request.auth.uid == request.resource.data.userId;
      allow update: if request.auth.uid == resource.data.userId;
    }
    
    match /transactions/{document=**} {
      allow read: if request.auth.uid in [
        resource.data.fromUserId,
        resource.data.toUserId
      ];
      allow create: if request.auth.uid == request.resource.data.fromUserId;
    }
    
    match /investments/{document=**} {
      allow read: if request.auth.uid in [
        resource.data.investorUserId
      ];
      allow create: if request.auth.uid == request.resource.data.investorUserId;
    }
    
    match /budgets/{document=**} {
      allow read, write: if request.auth.uid == resource.data.userId;
    }
  }
}
```
