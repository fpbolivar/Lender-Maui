# Firestore Database Integration Guide

## Overview
Successfully integrated Firestore database into the Lender MAUI application using the **Firestore REST API** instead of native SDKs. This approach is more reliable and requires no platform-specific bindings.

## Architecture

### Service Layer
**Location:** `Scripts/Services/FirestoreService.cs`

The `FirestoreService` is a singleton that provides:
- Direct HTTP communication with Firestore REST API
- Automatic JSON serialization/deserialization
- Collection management for: users, loans, transactions, investments, budgets
- Comprehensive error logging and debugging

### Integration Point
**Location:** `Scripts/Services/AuthenticationService.cs`

When a user signs up:
1. Email/password account created in Firebase Authentication
2. User document automatically created in Firestore `users` collection
3. User object initialized with default values (name, phone, balance, credit score, etc.)

## Data Models

### 1. User Collection
**File:** `Scripts/Models/User.cs`

```csharp
public class User
{
    // Identity
    public string Id { get; set; }              // Firebase UID
    public string Email { get; set; }           // Unique email
    public string FullName { get; set; }        // User's full name
    public string PhoneNumber { get; set; }     // Contact phone
    
    // Financial
    public decimal Balance { get; set; }        // Wallet balance
    public decimal CreditScore { get; set; }    // Credit rating (0-850)
    public decimal TotalLent { get; set; }      // Total amount lent
    public decimal TotalBorrowed { get; set; }  // Total amount borrowed
    
    // Metadata
    public int LoansGiven { get; set; }         // Number of loans given
    public int LoansReceived { get; set; }      // Number of loans received
    public DateTime JoinDate { get; set; }      // Account creation date
    public UserStatus Status { get; set; }      // Active/Inactive/Suspended
    public bool IsVerified { get; set; }        // Email verified
}

public enum UserStatus { Active, Inactive, Suspended }
```

**Usage Example:**
```csharp
var user = new User
{
    Id = "user123",
    Email = "user@example.com",
    FullName = "John Doe",
    Balance = 1000m,
    CreditScore = 750m,
    Status = UserStatus.Active
};
await FirestoreService.Instance.SaveUserAsync(user);
```

### 2. LoanRequest Collection
**File:** `Scripts/Models/LoanRequest.cs`

```csharp
public class LoanRequest
{
    // Identity & References
    public string Id { get; set; }              // Firestore document ID
    public string UserId { get; set; }          // Borrower ID (FK to users)
    public string UserName { get; set; }        // Borrower name
    
    // Loan Details
    public decimal Amount { get; set; }         // Requested loan amount
    public decimal InterestRate { get; set; }   // Annual interest rate (%)
    public int DurationMonths { get; set; }     // Loan duration in months
    public string Description { get; set; }     // Loan purpose
    public string Category { get; set; }        // Purpose category (Home, Auto, etc.)
    
    // Funding
    public decimal AmountFunded { get; set; }   // Amount funded so far
    public int FundedPercentage { get; set; }   // Funding progress (0-100%)
    
    // Risk & Status
    public LoanStatus Status { get; set; }      // Pending/Active/Funded/Repaying/Completed
    public decimal MinCreditScore { get; set; } // Minimum credit requirement
    public string RiskRating { get; set; }      // Low/Medium/High
    
    // Dates
    public DateTime CreatedDate { get; set; }   // When loan was posted
    public DateTime DueDate { get; set; }       // Expected repayment date
    public DateTime? FundedDate { get; set; }   // When fully funded
}

public enum LoanStatus 
{ 
    Pending,    // Waiting for funding
    Active,     // Being funded
    Funded,     // Fully funded, ready for repayment
    Repaying,   // In repayment phase
    Completed,  // Successfully repaid
    Defaulted,  // Payment defaulted
    Rejected    // Request rejected
}
```

### 3. Transaction Collection
**File:** `Scripts/Models/Transaction.cs`

```csharp
public class Transaction
{
    // Identity
    public string Id { get; set; }              // Transaction ID
    public string LoanRequestId { get; set; }   // Associated loan (FK)
    
    // Parties
    public string FromUserId { get; set; }      // Sender (FK to users)
    public string ToUserId { get; set; }        // Recipient (FK to users)
    
    // Amount Breakdown
    public decimal Amount { get; set; }         // Total transaction amount
    public decimal PrincipalAmount { get; set; } // Principal portion
    public decimal InterestAmount { get; set; } // Interest portion
    
    // Classification
    public TransactionType Type { get; set; }   // Type of transaction
    public TransactionStatus Status { get; set; } // Current status
    public string Description { get; set; }     // Transaction note
    
    // Metadata
    public DateTime CreatedDate { get; set; }   // Creation timestamp
    public DateTime? CompletedDate { get; set; } // Completion timestamp
    public int PaymentNumber { get; set; }      // Monthly payment #
    public string ReferenceNumber { get; set; } // Reference for tracking
    public string Merchant { get; set; }        // Payment method
    public string Category { get; set; }        // Transaction category
}

public enum TransactionType 
{ 
    Funding,      // User funding a loan
    Repayment,    // Borrower repaying
    Interest,     // Interest payment
    Transfer,     // User to user transfer
    Withdrawal,   // Wallet withdrawal
    Deposit       // Wallet deposit
}

public enum TransactionStatus
{
    Pending,      // Awaiting processing
    Processing,   // Currently processing
    Completed,    // Successfully completed
    Failed,       // Transaction failed
    Cancelled,    // Cancelled by user
    Refunded      // Refunded back
}
```

### 4. LoanInvestment Collection
**File:** `Scripts/Models/LoanInvestment.cs`

```csharp
public class LoanInvestment
{
    // Identity
    public string Id { get; set; }                  // Investment ID
    
    // References
    public string LoanRequestId { get; set; }       // Loan being invested in (FK)
    public string InvestorUserId { get; set; }      // Investor user ID (FK)
    
    // Investment Details
    public decimal InvestmentAmount { get; set; }   // Amount invested
    public decimal InterestRate { get; set; }       // Interest rate for this investment
    public decimal ExpectedReturn { get; set; }     // Expected total return
    public decimal AmountReturned { get; set; }     // Amount returned so far
    
    // Payment Tracking
    public int PaymentsCompleted { get; set; }      // Number of payments received
    public int TotalPaymentsExpected { get; set; }  // Expected payment count
    public DateTime? NextPaymentDate { get; set; }  // Next payment due
    
    // Status
    public InvestmentStatus Status { get; set; }    // Current status
    
    // Dates
    public DateTime InvestmentDate { get; set; }    // When investment was made
}

public enum InvestmentStatus
{
    Pending,      // Investment pending approval
    Active,       // Investment active and accruing
    Completed,    // Investment completed and repaid
    Defaulted,    // Loan defaulted
    Cancelled     // Investment cancelled
}
```

### 5. Budget Collection
**File:** `Scripts/Models/Budget.cs`

```csharp
public class Budget
{
    // Identity
    public string Id { get; set; }          // Budget ID
    public string UserId { get; set; }      // Owner (FK to users)
    
    // Budget Details
    public string Category { get; set; }    // Category name (Housing, Food, etc.)
    public decimal BudgetLimit { get; set; } // Monthly limit
    public decimal AmountSpent { get; set; } // Amount spent this month
    
    // Customization
    public string IconEmoji { get; set; }    // Category icon (e.g., "🏠")
    public string ColorHex { get; set; }     // Display color
    
    // Tracking
    public string PeriodMonthYear { get; set; } // Format: "2025-01"
    public bool IsActive { get; set; }       // Currently tracking
    public int WarningThreshold { get; set; } // Alert at % of limit
    
    // Dates
    public DateTime CreatedDate { get; set; } // Creation date
    
    // Helper Methods
    public int GetPercentageUsed()
    public bool IsOverBudget()
    public bool ShouldShowWarning()
}
```

## API Reference

### FirestoreService Methods

#### User Operations
```csharp
// Save or update a user
Task<bool> SaveUserAsync(User user)

// Get user by ID
Task<User?> GetUserAsync(string userId)
```

#### Loan Operations
```csharp
// Create a new loan request
Task<string?> CreateLoanAsync(LoanRequest loan)

// Get a specific loan
Task<LoanRequest?> GetLoanAsync(string loanId)

// Update a loan (status, funding, etc.)
Task<bool> UpdateLoanAsync(LoanRequest loan)
```

#### Transaction Operations
```csharp
// Record a transaction (funding, repayment, etc.)
Task<string?> CreateTransactionAsync(Transaction transaction)
```

#### Investment Operations
```csharp
// Create an investment when user funds a loan
Task<string?> CreateInvestmentAsync(LoanInvestment investment)
```

#### Budget Operations
```csharp
// Save or update a budget
Task<bool> SaveBudgetAsync(Budget budget)
```

## Usage Examples

### Example 1: Creating a New Loan Request
```csharp
var loan = new LoanRequest
{
    UserId = currentUser.Id,
    UserName = currentUser.FullName,
    Amount = 5000m,
    InterestRate = 8.5m,
    DurationMonths = 24,
    Description = "Need funds for home renovation",
    Category = "Home Improvement",
    MinCreditScore = 650m,
    RiskRating = "Medium",
    Status = LoanStatus.Pending
};

var loanId = await FirestoreService.Instance.CreateLoanAsync(loan);
if (loanId != null)
{
    Debug.WriteLine($"Loan created: {loanId}");
}
```

### Example 2: Recording a Payment
```csharp
var payment = new Transaction
{
    LoanRequestId = loanId,
    FromUserId = borrowerId,
    ToUserId = investorId,
    Amount = 250m,
    PrincipalAmount = 200m,
    InterestAmount = 50m,
    Type = TransactionType.Repayment,
    Status = TransactionStatus.Completed,
    PaymentNumber = 1,
    Description = "Monthly loan repayment"
};

await FirestoreService.Instance.CreateTransactionAsync(payment);
```

### Example 3: Creating an Investment
```csharp
var investment = new LoanInvestment
{
    LoanRequestId = loanId,
    InvestorUserId = investorId,
    InvestmentAmount = 1000m,
    InterestRate = loan.InterestRate,
    ExpectedReturn = 1170m,
    TotalPaymentsExpected = 24,
    Status = InvestmentStatus.Active
};

var investmentId = await FirestoreService.Instance.CreateInvestmentAsync(investment);
```

### Example 4: Setting Up a Budget
```csharp
var budget = new Budget
{
    UserId = userId,
    Category = "Housing",
    BudgetLimit = 1500m,
    IconEmoji = "🏠",
    ColorHex = "#FF6B6B",
    PeriodMonthYear = DateTime.Now.ToString("yyyy-MM"),
    IsActive = true,
    WarningThreshold = 80
};

await FirestoreService.Instance.SaveBudgetAsync(budget);
```

## Firestore Collection Structure

### users
```
/users/{userId}
├── fullName: string
├── email: string
├── phoneNumber: string
├── balance: number
├── creditScore: number
├── loansGiven: number
├── loansReceived: number
├── joinDate: timestamp
├── status: string (enum)
├── isVerified: boolean
├── totalLent: number
├── totalBorrowed: number
└── lastUpdated: timestamp
```

### loans
```
/loans/{loanId}
├── userId: string (FK)
├── userName: string
├── description: string
├── amount: number
├── interestRate: number
├── durationMonths: number
├── status: string (enum)
├── createdDate: timestamp
├── dueDate: timestamp
├── category: string
├── fundedPercentage: number
├── amountFunded: number
├── fundedDate: timestamp
├── minCreditScore: number
└── riskRating: string
```

### transactions
```
/transactions/{transactionId}
├── loanRequestId: string (FK)
├── fromUserId: string (FK)
├── toUserId: string (FK)
├── amount: number
├── type: string (enum)
├── status: string (enum)
├── createdDate: timestamp
├── completedDate: timestamp
├── description: string
├── merchant: string
├── category: string
├── paymentNumber: number
├── principalAmount: number
├── interestAmount: number
└── referenceNumber: string
```

### investments
```
/investments/{investmentId}
├── loanRequestId: string (FK)
├── investorUserId: string (FK)
├── investmentAmount: number
├── investmentDate: timestamp
├── expectedReturn: number
├── amountReturned: number
├── status: string (enum)
├── interestRate: number
├── nextPaymentDate: timestamp
├── paymentsCompleted: number
└── totalPaymentsExpected: number
```

### budgets
```
/budgets/{budgetId}
├── userId: string (FK)
├── category: string
├── budgetLimit: number
├── amountSpent: number
├── iconEmoji: string
├── colorHex: string
├── createdDate: timestamp
├── periodMonthYear: string
├── isActive: boolean
└── warningThreshold: number
```

## Technical Implementation Details

### REST API Communication
The service uses Firestore REST API endpoints:
- **Base URL:** `https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents`
- **Project ID:** `lender-d0412`
- **API Key:** Requires Firebase Web API Key

### Serialization
- **Request Format:** JSON with Firestore value types
  ```json
  {
    "fields": {
      "email": { "stringValue": "user@example.com" },
      "balance": { "doubleValue": 1000.50 },
      "status": { "stringValue": "Active" }
    }
  }
  ```

- **Response Parsing:** Extracts fields from Firestore response structure

### Error Handling
All operations include:
- Try-catch blocks with detailed logging
- Null-safe value extraction
- Safe enum parsing
- Debug output for troubleshooting

## Integration Workflow

### User Sign-Up Flow
1. User enters email/password in LoginPage
2. AuthenticationService.SignUpAsync() creates Firebase Auth account
3. User document automatically created in Firestore `users` collection
4. User navigated to MainPage (dashboard)
5. User data ready for subsequent operations

### Data Access Pattern
1. FirestoreService is a singleton (accessed via `FirestoreService.Instance`)
2. All async methods return Task or Task<T>
3. Null values indicate operation failures
4. Debug logging available for all operations

## Security Considerations

### Current Setup
- Firebase Security Rules: NOT YET CONFIGURED
- API Key: Public (Web API Key - expected to be public)
- Authentication: Required for sensitive operations

### Recommended Rules (Future)
```firestore
match /users/{userId} {
  allow read, write: if request.auth.uid == userId;
}

match /loans/{loanId} {
  allow read: if request.auth != null;
  allow create: if request.auth.uid == request.resource.data.userId;
  allow update: if request.auth.uid == resource.data.userId;
}

match /investments/{investmentId} {
  allow read: if request.auth != null;
  allow create: if request.auth.uid == request.resource.data.investorUserId;
}

match /transactions/{transactionId} {
  allow read: if request.auth.uid in [resource.data.fromUserId, resource.data.toUserId];
  allow create: if request.auth.uid == request.resource.data.fromUserId;
}

match /budgets/{budgetId} {
  allow read, write: if request.auth.uid == resource.data.userId;
}
```

## Next Steps

1. **Connect ViewModels to Firestore:**
   - DashboardViewModel: Load user data, active loans, investments
   - LoanViewModel: Create and manage loan requests
   - InvestmentViewModel: Browse and fund loans

2. **Create Additional Screens:**
   - User profile edit page
   - Loan creation wizard
   - Loan browsing/search page
   - Investment portfolio view
   - Transaction history

3. **Implement Business Logic:**
   - Loan search and filtering
   - Investment recommendations
   - Payment scheduling
   - Budget alerts

4. **Set Firebase Security Rules:**
   - Enable proper access control
   - Validate data on write
   - Implement rate limiting

5. **Add Error Handling UI:**
   - Show user-friendly error messages
   - Implement retry logic
   - Add offline caching (optional)

## Files Modified/Created

### New Files
- `Scripts/Services/FirestoreService.cs` (700+ lines)
- `Scripts/Models/LoanInvestment.cs` (55 lines)

### Modified Files
- `Scripts/Services/AuthenticationService.cs` - Added Firestore integration in SignUpAsync
- `Scripts/Models/User.cs` - Enhanced with additional fields
- `Scripts/Models/LoanRequest.cs` - Extended with funding and risk fields
- `Scripts/Models/Transaction.cs` - Added payment details (fixed duplicate enum)
- `Scripts/Models/Budget.cs` - Added period tracking and helper methods

### Git Commit
```
commit cdc83f6
feat: Add Firestore REST API integration with comprehensive database models and service
```

## Troubleshooting

### Issue: "Unable to connect to Firestore"
- Verify `NSAppTransportSecurity` in Info.plist allows `firestore.googleapis.com`
- Check Firebase API key is correct
- Verify project ID matches Firebase console

### Issue: "User document not created on signup"
- Check Network logs in debug output
- Verify `SaveUserAsync` returns true
- Check Firestore console for new documents

### Issue: "Serialization errors in JSON"
- Ensure all model properties have default values
- Check enum values match Firestore storage exactly
- Verify DateTime format is ISO 8601

## Resources
- [Firestore REST API Documentation](https://firebase.google.com/docs/firestore/use-rest-api)
- [Firestore Data Model](https://firebase.google.com/docs/firestore/data-model)
- [Firebase Console](https://console.firebase.google.com/)
- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
