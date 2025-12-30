# Data Flow and Firestore Collection Structure Guide

## Overview
This document explains how user data flows from the sign-up form through the app and into Firestore collections in a professional, organized structure.

## Sign-Up Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    SIGN-UP UI (LoginPage.xaml)                   │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │ First Name    ├─────────────────┐                          │  │
│  │ Last Name     ├─────────────────┤                          │  │
│  │ Date of Birth ├─────────────────┼─→ LoginViewModel         │  │
│  │ Phone Number  ├─────────────────┤    (Validation)          │  │
│  │ Email         ├─────────────────┤                          │  │
│  │ Password      ├─────────────────┘                          │  │
│  └────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
          SignUpCommand.Execute() Triggered
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│            VALIDATION (LoginViewModel.SignUpAsync)               │
│                                                                   │
│  • Email format: must contain @                                  │
│  • Password: minimum 6 characters                                │
│  • First Name: required, non-empty                               │
│  • Last Name: required, non-empty                                │
│  • Date of Birth: must be 13-120 years old                       │
│  • Phone Number: optional (any format)                           │
└─────────────────────────────────────────────────────────────────┘
                              ↓
         ✅ All Validation Passed
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│    AUTHENTICATION (AuthenticationService.SignUpAsync)            │
│                                                                   │
│  1. Create Firebase Auth Account                                 │
│     - Email: john@example.com                                    │
│     - Password: SecurePass123                                    │
│     - Returns: idToken, localId (Firebase UID)                  │
│                                                                   │
│  2. Save Auth Token Locally                                      │
│     - Stored in SecureStorage                                    │
│     - Used for future API calls                                  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
          Firebase Auth Success, Got User ID
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│    CREATE USER MODEL (AuthenticationService)                     │
│                                                                   │
│  Populate User object with sign-up data:                         │
│                                                                   │
│  var newUser = new User                                          │
│  {                                                               │
│    Id = "firebase-uid-12345",          // Firebase UID          │
│    Email = "john@example.com",                                   │
│    FullName = "John Doe",              // firstName + lastName  │
│    PhoneNumber = "+1-555-123-4567",    // Optional              │
│    DateOfBirth = DateTime(1995, 5, 15),// Parsed from picker    │
│    Balance = 0m,                       // Initial balance       │
│    CreditScore = 0m,                   // Not yet verified      │
│    Status = UserStatus.Active,         // Account status        │
│    JoinDate = DateTime.UtcNow,         // Registration timestamp│
│    IsVerified = false,                 // Needs verification    │
│    LoansGiven = 0,                     // Initial counters      │
│    LoansReceived = 0,                  │
│    TotalLent = 0m,                     │
│    TotalBorrowed = 0m                  │
│  };                                                              │
└─────────────────────────────────────────────────────────────────┘
                              ↓
       Call FirestoreService.SaveUserAsync(newUser)
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│    FIRESTORE SAVE (FirestoreService.SaveUserAsync)               │
│                                                                   │
│  1. Construct REST API URL:                                      │
│     https://firestore.googleapis.com/v1/projects/lender-d0412/   │
│     databases/(default)/documents/users/{userId}                 │
│                                                                   │
│  2. Serialize User to Firestore Format:                          │
│     {                                                             │
│       "fields": {                                                │
│         "fullName": { "stringValue": "John Doe" },              │
│         "email": { "stringValue": "john@example.com" },         │
│         "phoneNumber": { "stringValue": "+1-555-123-4567" },    │
│         "dateOfBirth": {                                         │
│           "timestampValue": "1995-05-15T00:00:00Z"              │
│         },                                                        │
│         "balance": { "doubleValue": 0.0 },                       │
│         "creditScore": { "doubleValue": 0.0 },                   │
│         "loansGiven": { "integerValue": 0 },                     │
│         "loansReceived": { "integerValue": 0 },                  │
│         "joinDate": {                                            │
│           "timestampValue": "2025-12-30T15:30:45Z"              │
│         },                                                        │
│         "status": { "stringValue": "Active" },                   │
│         "isVerified": { "booleanValue": false },                 │
│         "totalLent": { "doubleValue": 0.0 },                     │
│         "totalBorrowed": { "doubleValue": 0.0 },                 │
│         "lastUpdated": {                                         │
│           "timestampValue": "2025-12-30T15:30:45Z"              │
│         }                                                         │
│       }                                                           │
│     }                                                             │
│                                                                   │
│  3. Send PATCH request to Firestore REST API                     │
│     - Method: PATCH                                              │
│     - Endpoint: .../users/{userId}?key={apiKey}                 │
│     - Body: JSON (serialized above)                              │
│     - Content-Type: application/json                             │
│                                                                   │
│  4. Handle Response:                                             │
│     ✅ Success (200): Document created/updated                   │
│     ❌ Error: Log error, return false                             │
└─────────────────────────────────────────────────────────────────┘
                              ↓
              Document Saved to Firestore
                              ↓
           ✅ User successfully registered!
```

## Firestore Collection Structure

### 1. **users** Collection
The users collection stores complete user profiles with personal and financial information.

**Document ID:** Firebase UID (e.g., `firebase-uid-12345`)

**Fields:**
```
users/{userId}
├── fullName: string                    // "John Doe"
├── email: string                       // "john@example.com"
├── phoneNumber: string                 // "+1-555-123-4567"
├── dateOfBirth: timestamp              // 1995-05-15T00:00:00Z
├── balance: number                     // 0 (wallet balance in USD)
├── creditScore: number                 // 0-850 rating
├── loansGiven: integer                 // Number of loans lent out
├── loansReceived: integer              // Number of loans received
├── status: string                      // "Active", "Inactive", "Suspended"
├── isVerified: boolean                 // KYC/AML verification status
├── totalLent: number                   // Total amount lent (all time)
├── totalBorrowed: number               // Total amount borrowed (all time)
├── joinDate: timestamp                 // Account creation time
└── lastUpdated: timestamp              // Last modification time
```

**Example Document:**
```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "phoneNumber": "+1-555-123-4567",
  "dateOfBirth": "1995-05-15T00:00:00Z",
  "balance": 5000.00,
  "creditScore": 750,
  "loansGiven": 3,
  "loansReceived": 1,
  "status": "Active",
  "isVerified": true,
  "totalLent": 15000.00,
  "totalBorrowed": 5000.00,
  "joinDate": "2025-12-30T15:30:45Z",
  "lastUpdated": "2025-12-30T15:30:45Z"
}
```

---

### 2. **loans** Collection
Peer-to-peer loan requests and details.

**Document ID:** Auto-generated by Firestore

**Fields:**
```
loans/{loanId}
├── userId: string (FK)                 // Borrower's ID (links to users/{userId})
├── userName: string                    // Borrower's name (denormalized)
├── description: string                 // Loan purpose
├── amount: number                      // Requested amount (USD)
├── interestRate: number                // Annual interest rate (%)
├── durationMonths: integer             // Loan term in months
├── category: string                    // Purpose (Home, Auto, Education, etc.)
├── status: string                      // Pending|Active|Funded|Repaying|Completed|Defaulted
├── fundedAmount: number                // Amount funded so far
├── fundedPercentage: integer           // Funding progress (0-100%)
├── minCreditScore: number              // Minimum credit requirement
├── riskRating: string                  // Low|Medium|High
├── createdDate: timestamp              // When loan was posted
├── dueDate: timestamp                  // Expected repayment date
└── fundedDate: timestamp               // When fully funded (null if pending)
```

**Example Document:**
```json
{
  "userId": "firebase-uid-john",
  "userName": "John Doe",
  "description": "Home renovation project",
  "amount": 10000.00,
  "interestRate": 8.5,
  "durationMonths": 24,
  "category": "Home Improvement",
  "status": "Funded",
  "fundedAmount": 10000.00,
  "fundedPercentage": 100,
  "minCreditScore": 650,
  "riskRating": "Low",
  "createdDate": "2025-12-15T10:00:00Z",
  "dueDate": "2027-12-15T10:00:00Z",
  "fundedDate": "2025-12-20T14:30:00Z"
}
```

---

### 3. **transactions** Collection
All monetary movements (funding, repayments, interest, transfers).

**Document ID:** Auto-generated by Firestore

**Fields:**
```
transactions/{transactionId}
├── loanRequestId: string (FK)          // Associated loan (links to loans/{loanId})
├── fromUserId: string (FK)             // Sender (links to users/{userId})
├── toUserId: string (FK)               // Recipient (links to users/{userId})
├── amount: number                      // Total transaction amount (USD)
├── principalAmount: number             // Principal portion (for loan payments)
├── interestAmount: number              // Interest portion (for loan payments)
├── type: string                        // Funding|Repayment|Interest|Transfer|Withdrawal|Deposit
├── status: string                      // Pending|Processing|Completed|Failed|Cancelled|Refunded
├── description: string                 // Transaction details
├── merchant: string                    // Payment method/provider
├── category: string                    // Transaction category
├── paymentNumber: integer              // Monthly payment number (for loans)
├── referenceNumber: string             // Tracking reference
├── createdDate: timestamp              // Transaction initiation time
└── completedDate: timestamp            // When transaction completed (null if pending)
```

**Example Document:**
```json
{
  "loanRequestId": "loan-12345",
  "fromUserId": "firebase-uid-jane",
  "toUserId": "firebase-uid-john",
  "amount": 10000.00,
  "principalAmount": 10000.00,
  "interestAmount": 0.00,
  "type": "Funding",
  "status": "Completed",
  "description": "Initial funding for home renovation",
  "merchant": "Bank Transfer",
  "category": "Loan Funding",
  "paymentNumber": 0,
  "referenceNumber": "FUND-2025-12-20-001",
  "createdDate": "2025-12-20T10:00:00Z",
  "completedDate": "2025-12-20T14:30:00Z"
}
```

---

### 4. **investments** Collection
Investor positions and returns tracking.

**Document ID:** Auto-generated by Firestore

**Fields:**
```
investments/{investmentId}
├── loanRequestId: string (FK)          // Loan being invested in
├── investorUserId: string (FK)         // Investor's ID
├── investmentAmount: number            // Amount invested (USD)
├── interestRate: number                // Interest rate for this investment
├── expectedReturn: number              // Expected total return (principal + interest)
├── amountReturned: number              // Amount returned so far
├── status: string                      // Active|Completed|Defaulted|Pending|Cancelled
├── paymentsCompleted: integer          // Payments received so far
├── totalPaymentsExpected: integer      // Total expected payments
├── investmentDate: timestamp           // When investment was made
├── nextPaymentDate: timestamp          // Next expected payment date
└── amountReturned: number              // Total returned amount
```

**Example Document:**
```json
{
  "loanRequestId": "loan-12345",
  "investorUserId": "firebase-uid-jane",
  "investmentAmount": 10000.00,
  "interestRate": 8.5,
  "expectedReturn": 11700.00,
  "amountReturned": 0.00,
  "status": "Active",
  "paymentsCompleted": 0,
  "totalPaymentsExpected": 24,
  "investmentDate": "2025-12-20T14:30:00Z",
  "nextPaymentDate": "2026-01-20T10:00:00Z"
}
```

---

### 5. **budgets** Collection
User spending categories and tracking.

**Document ID:** Auto-generated by Firestore

**Fields:**
```
budgets/{budgetId}
├── userId: string (FK)                 // Owner (links to users/{userId})
├── category: string                    // Category name
├── budgetLimit: number                 // Monthly limit (USD)
├── amountSpent: number                 // Amount spent this period
├── iconEmoji: string                   // Visual emoji (e.g., "🏠")
├── colorHex: string                    // Color code (e.g., "#FF6B6B")
├── periodMonthYear: string             // Period (e.g., "2025-12")
├── isActive: boolean                   // Whether currently tracking
├── warningThreshold: integer           // Alert percentage (e.g., 80%)
└── createdDate: timestamp              // Creation date
```

**Example Document:**
```json
{
  "userId": "firebase-uid-john",
  "category": "Housing",
  "budgetLimit": 1500.00,
  "amountSpent": 1200.00,
  "iconEmoji": "🏠",
  "colorHex": "#FF6B6B",
  "periodMonthYear": "2025-12",
  "isActive": true,
  "warningThreshold": 80,
  "createdDate": "2025-12-01T00:00:00Z"
}
```

---

## Data Relationships

### Foreign Key Relationships
```
users (1) ──────→ (Many) loans
users (1) ──────→ (Many) transactions (as fromUserId or toUserId)
users (1) ──────→ (Many) investments
users (1) ──────→ (Many) budgets

loans (1) ──────→ (Many) transactions
loans (1) ──────→ (Many) investments

investments (Many) ←──── (1) loans
```

### Query Examples

**Get all loans for a user:**
```
Query: loans where userId == "{userId}"
Returns: All loans borrowed by the user
```

**Get all investments by a user:**
```
Query: investments where investorUserId == "{userId}"
Returns: All loans the user has funded
```

**Get all transactions for a loan:**
```
Query: transactions where loanRequestId == "{loanId}"
Returns: All payments and transfers for the loan
```

**Get active user budgets:**
```
Query: budgets where userId == "{userId}" AND isActive == true
Returns: Currently tracked spending categories
```

---

## Debugging Firestore Operations

### Logging Output
When saving/retrieving data, check the debug output for:

```
[FirestoreService] Starting SaveUserAsync for user: john@example.com (ID: firebase-uid-12345)
[FirestoreService] URL: https://firestore.googleapis.com/v1/projects/lender-d0412/databases/(default)/documents/users/firebase-uid-12345?key=...
[FirestoreService] Payload: {"fields":{...}}
[FirestoreService] Response Status: 200
[FirestoreService] Success! Response: {"name":"projects/lender-d0412/databases/(default)/documents/users/firebase-uid-12345",...}
[FirestoreService] User john@example.com saved to Firestore successfully
```

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| User created in Firebase but not in Firestore | Network error or API key invalid | Check debug logs for HTTP status |
| Document fields empty or null | JSON serialization error | Verify UserToJson format |
| 400 Bad Request | Invalid JSON format | Check field types (stringValue, doubleValue, etc.) |
| 401 Unauthorized | Invalid API key | Regenerate and update in FirestoreService |
| 403 Forbidden | Firestore Security Rules | Check Firebase console security rules |

---

## Professional Data Management Practices

### Naming Conventions
- **Collections**: Lowercase, plural (users, loans, transactions)
- **Documents**: Use Firebase UID for users, auto-generate for others
- **Fields**: camelCase (firstName, dateOfBirth, totalLent)
- **Enums**: PascalCase, stored as strings (Active, Pending)

### Timestamps
- Always use ISO 8601 format: `yyyy-MM-ddTHH:mm:ssZ`
- Store in UTC (Coordinated Universal Time)
- Include milliseconds when precision matters

### Financial Fields
- Store amounts as `number` (double) with 2 decimal precision
- Currency assumed to be USD
- Calculate interest programmatically (not stored)

### Auditing
- Every document has `lastUpdated` timestamp
- Track state changes in transactions
- Maintain history through transaction records

---

## Next Steps

1. **Monitor Firestore Console** to verify data is being saved
2. **Set Security Rules** to restrict unauthorized access
3. **Create Indexes** for commonly queried fields
4. **Implement Data Backup** strategy
5. **Add Validation Rules** in Firestore Security Rules
