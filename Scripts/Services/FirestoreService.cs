using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Lender.Models;

namespace Lender.Services;

/// <summary>
/// Service for managing Firestore database operations via REST API
/// Handles CRUD operations for all collections
/// </summary>
public class FirestoreService
{
    private static FirestoreService? _instance;
    private readonly string _projectId = "lender-d0412";
    private readonly string _apiKey = "AIzaSyBiRfWl6FILfLl2-jMv0ENpQFVNH2YYwLI";
    private readonly string _baseUrl;
    private HttpClient _httpClient;

    // Collection names
    private const string UsersCollection = "users";
    private const string LoansCollection = "loans";
    private const string TransactionsCollection = "transactions";
    private const string InvestmentsCollection = "investments";
    private const string BudgetsCollection = "budgets";

    public static FirestoreService Instance => _instance ??= new FirestoreService();

    public FirestoreService()
    {
        _baseUrl = $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents";
        _httpClient = new HttpClient();
    }

    // ==================== USER OPERATIONS ====================

    /// <summary>
    /// Create or update a user document in Firestore
    /// </summary>
    public async Task<bool> SaveUserAsync(User user)
    {
        try
        {
            Debug.WriteLine($"[FirestoreService] ===== START SAVE USER =====");
            Debug.WriteLine($"[FirestoreService] Starting SaveUserAsync for user: {user.Email} (ID: {user.Id})");
            
            // Use email as document ID for easier access
            var documentId = Uri.EscapeDataString(user.Email);
            var url = $"{_baseUrl}/{UsersCollection}/{documentId}?key={_apiKey}";
            Debug.WriteLine($"[FirestoreService] URL: {url}");
            
            var payload = UserToJson(user);
            Debug.WriteLine($"[FirestoreService] Payload length: {payload.Length} chars");
            Debug.WriteLine($"[FirestoreService] Payload: {payload}");
            
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            Debug.WriteLine($"[FirestoreService] Sending PATCH request...");
            var response = await _httpClient.PatchAsync(url, content);
            Debug.WriteLine($"[FirestoreService] Response Status: {response.StatusCode} ({(int)response.StatusCode})");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[FirestoreService] Success! Response length: {responseContent.Length}");
                Debug.WriteLine($"[FirestoreService] Response: {responseContent}");
                Debug.WriteLine($"[FirestoreService] ✅ User {user.Email} saved to Firestore successfully");
                Debug.WriteLine($"[FirestoreService] ===== END SAVE USER SUCCESS =====");
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[FirestoreService] ❌ Error saving user - Status: {response.StatusCode}");
            Debug.WriteLine($"[FirestoreService] Error response length: {errorContent.Length}");
            Debug.WriteLine($"[FirestoreService] Error response: {errorContent}");
            Debug.WriteLine($"[FirestoreService] ===== END SAVE USER FAIL =====");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] ❌ EXCEPTION saving user: {ex.GetType().Name}");
            Debug.WriteLine($"[FirestoreService] Exception message: {ex.Message}");
            Debug.WriteLine($"[FirestoreService] Stack trace: {ex.StackTrace}");
            Debug.WriteLine($"[FirestoreService] ===== END SAVE USER EXCEPTION =====");
            return false;
        }
    }

    /// <summary>
    /// Get a user by email
    /// </summary>
    public async Task<User?> GetUserAsync(string email)
    {
        try
        {
            Debug.WriteLine($"[FirestoreService] Getting user: {email}");
            
            var documentId = Uri.EscapeDataString(email);
            var url = $"{_baseUrl}/{UsersCollection}/{documentId}?key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            
            Debug.WriteLine($"[FirestoreService] Get user response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[FirestoreService] Error getting user: {errorContent}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[FirestoreService] User data received, parsing...");
            return JsonToUser(email, content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] Exception getting user: {ex.Message}");
            return null;
        }
    }

    // ==================== LOAN OPERATIONS ====================

    /// <summary>
    /// Create a new loan request
    /// </summary>
    public async Task<string?> CreateLoanAsync(LoanRequest loan)
    {
        try
        {
            var url = $"{_baseUrl}/{LoansCollection}?key={_apiKey}";
            var payload = LoanToJson(loan);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var docId = ExtractDocumentId(responseContent);
                Debug.WriteLine($"Loan created with ID: {docId} - FirestoreService.cs:105");
                return docId;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating loan: {ex.Message} - FirestoreService.cs:113");
            return null;
        }
    }

    /// <summary>
    /// Get a loan by ID
    /// </summary>
    public async Task<LoanRequest?> GetLoanAsync(string loanId)
    {
        try
        {
            var url = $"{_baseUrl}/{LoansCollection}/{loanId}?key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonToLoan(loanId, content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting loan: {ex.Message} - FirestoreService.cs:135");
            return null;
        }
    }

    /// <summary>
    /// Update a loan's status and funded amount
    /// </summary>
    public async Task<bool> UpdateLoanAsync(LoanRequest loan)
    {
        try
        {
            var url = $"{_baseUrl}/{LoansCollection}/{loan.Id}?key={_apiKey}";
            var payload = LoanToJson(loan);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PatchAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating loan: {ex.Message} - FirestoreService.cs:156");
            return false;
        }
    }

    // ==================== TRANSACTION OPERATIONS ====================

    /// <summary>
    /// Record a transaction (funding, repayment, etc.)
    /// </summary>
    public async Task<string?> CreateTransactionAsync(Transaction transaction)
    {
        try
        {
            var url = $"{_baseUrl}/{TransactionsCollection}?key={_apiKey}";
            var payload = TransactionToJson(transaction);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var docId = ExtractDocumentId(responseContent);
                Debug.WriteLine($"Transaction created with ID: {docId} - FirestoreService.cs:179");
                return docId;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating transaction: {ex.Message} - FirestoreService.cs:187");
            return null;
        }
    }

    // ==================== INVESTMENT OPERATIONS ====================

    /// <summary>
    /// Create an investment record when user funds a loan
    /// </summary>
    public async Task<string?> CreateInvestmentAsync(LoanInvestment investment)
    {
        try
        {
            var url = $"{_baseUrl}/{InvestmentsCollection}?key={_apiKey}";
            var payload = InvestmentToJson(investment);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var docId = ExtractDocumentId(responseContent);
                Debug.WriteLine($"Investment created with ID: {docId} - FirestoreService.cs:210");
                return docId;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating investment: {ex.Message} - FirestoreService.cs:218");
            return null;
        }
    }

    // ==================== BUDGET OPERATIONS ====================

    /// <summary>
    /// Save or update a budget
    /// </summary>
    public async Task<bool> SaveBudgetAsync(Budget budget)
    {
        try
        {
            var url = $"{_baseUrl}/{BudgetsCollection}/{budget.Id}?key={_apiKey}";
            var payload = BudgetToJson(budget);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PatchAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving budget: {ex.Message} - FirestoreService.cs:241");
            return false;
        }
    }

    // ==================== JSON CONVERSION METHODS ====================

    private string UserToJson(User user)
    {
        var fields = new Dictionary<string, object>
        {
            // User Information
            { "id", new { stringValue = user.Id } },
            { "email", new { stringValue = user.Email } },
            { "fullName", new { stringValue = user.FullName } },
            { "phoneNumber", new { stringValue = user.PhoneNumber } },
            { "dateOfBirth", new { stringValue = user.DateOfBirth.ToString("yyyy-MM-dd") } },
            
            // Loan Information
            { "balance", new { doubleValue = (double)user.Balance } },
            { "creditScore", new { doubleValue = (double)user.CreditScore } },
            { "loansGiven", new { integerValue = user.LoansGiven } },
            { "loansReceived", new { integerValue = user.LoansReceived } },
            { "totalLent", new { doubleValue = (double)user.TotalLent } },
            { "totalBorrowed", new { doubleValue = (double)user.TotalBorrowed } },
            
            // Metadata
            { "status", new { stringValue = user.Status.ToString() } },
            { "isVerified", new { booleanValue = user.IsVerified } },
            { "joinDate", new { timestampValue = user.JoinDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") } },
            { "lastUpdated", new { timestampValue = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") } }
        };

        var document = new
        {
            fields = fields
        };

        var json = JsonSerializer.Serialize(document);
        Debug.WriteLine($"[FirestoreService] Generated User JSON: {json}");
        return json;
    }

    private User? JsonToUser(string email, string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var fields = root.GetProperty("fields");

            return new User
            {
                Id = GetStringValue(fields, "id") ?? email, // Use stored ID or email
                FullName = GetStringValue(fields, "fullName"),
                Email = GetStringValue(fields, "email"),
                PhoneNumber = GetStringValue(fields, "phoneNumber"),
                DateOfBirth = DateTime.TryParse(GetStringValue(fields, "dateOfBirth"), out var dob) ? dob : DateTime.MinValue,
                Balance = (decimal)GetDoubleValue(fields, "balance"),
                CreditScore = (decimal)GetDoubleValue(fields, "creditScore"),
                LoansGiven = GetIntValue(fields, "loansGiven"),
                LoansReceived = GetIntValue(fields, "loansReceived"),
                JoinDate = GetDateTimeValue(fields, "joinDate"),
                Status = Enum.Parse<UserStatus>(GetStringValue(fields, "status")),
                IsVerified = GetBoolValue(fields, "isVerified"),
                TotalLent = (decimal)GetDoubleValue(fields, "totalLent"),
                TotalBorrowed = (decimal)GetDoubleValue(fields, "totalBorrowed")
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error parsing user JSON: {ex.Message} - FirestoreService.cs:298");
            return null;
        }
    }

    private string LoanToJson(LoanRequest loan)
    {
        var fields = new Dictionary<string, object>
        {
            { "userId", new { stringValue = loan.UserId } },
            { "userName", new { stringValue = loan.UserName } },
            { "description", new { stringValue = loan.Description } },
            { "amount", new { doubleValue = (double)loan.Amount } },
            { "interestRate", new { doubleValue = (double)loan.InterestRate } },
            { "durationMonths", new { integerValue = loan.DurationMonths } },
            { "status", new { stringValue = loan.Status.ToString() } },
            { "category", new { stringValue = loan.Category } },
            { "fundedPercentage", new { integerValue = loan.FundedPercentage } },
            { "amountFunded", new { doubleValue = (double)loan.AmountFunded } },
            { "minCreditScore", new { doubleValue = (double)loan.MinCreditScore } },
            { "riskRating", new { stringValue = loan.RiskRating } }
        };

        return JsonSerializer.Serialize(new { fields });
    }

    private LoanRequest? JsonToLoan(string loanId, string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var fields = root.GetProperty("fields");

            return new LoanRequest
            {
                Id = loanId,
                UserId = GetStringValue(fields, "userId"),
                UserName = GetStringValue(fields, "userName"),
                Description = GetStringValue(fields, "description"),
                Amount = (decimal)GetDoubleValue(fields, "amount"),
                InterestRate = (decimal)GetDoubleValue(fields, "interestRate"),
                DurationMonths = GetIntValue(fields, "durationMonths"),
                Status = Enum.Parse<LoanStatus>(GetStringValue(fields, "status")),
                Category = GetStringValue(fields, "category"),
                FundedPercentage = GetIntValue(fields, "fundedPercentage"),
                AmountFunded = (decimal)GetDoubleValue(fields, "amountFunded"),
                MinCreditScore = (decimal)GetDoubleValue(fields, "minCreditScore"),
                RiskRating = GetStringValue(fields, "riskRating")
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error parsing loan JSON: {ex.Message} - FirestoreService.cs:351");
            return null;
        }
    }

    private string TransactionToJson(Transaction tx)
    {
        var fields = new Dictionary<string, object>
        {
            { "loanRequestId", new { stringValue = tx.LoanRequestId } },
            { "fromUserId", new { stringValue = tx.FromUserId } },
            { "toUserId", new { stringValue = tx.ToUserId } },
            { "amount", new { doubleValue = (double)tx.Amount } },
            { "type", new { stringValue = tx.Type.ToString() } },
            { "status", new { stringValue = tx.Status.ToString() } },
            { "description", new { stringValue = tx.Description } }
        };

        return JsonSerializer.Serialize(new { fields });
    }

    private string InvestmentToJson(LoanInvestment inv)
    {
        var fields = new Dictionary<string, object>
        {
            { "loanRequestId", new { stringValue = inv.LoanRequestId } },
            { "investorUserId", new { stringValue = inv.InvestorUserId } },
            { "investmentAmount", new { doubleValue = (double)inv.InvestmentAmount } },
            { "expectedReturn", new { doubleValue = (double)inv.ExpectedReturn } },
            { "status", new { stringValue = inv.Status.ToString() } },
            { "interestRate", new { doubleValue = (double)inv.InterestRate } }
        };

        return JsonSerializer.Serialize(new { fields });
    }

    private string BudgetToJson(Budget budget)
    {
        var fields = new Dictionary<string, object>
        {
            { "userId", new { stringValue = budget.UserId } },
            { "category", new { stringValue = budget.Category } },
            { "budgetLimit", new { doubleValue = (double)budget.BudgetLimit } },
            { "amountSpent", new { doubleValue = (double)budget.AmountSpent } },
            { "isActive", new { booleanValue = budget.IsActive } }
        };

        return JsonSerializer.Serialize(new { fields });
    }

    private string ExtractDocumentId(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var name = doc.RootElement.GetProperty("name").GetString();
            return name?.Split('/').Last() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string GetStringValue(JsonElement fields, string key)
    {
        try
        {
            return fields.GetProperty(key).GetProperty("stringValue").GetString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private double GetDoubleValue(JsonElement fields, string key)
    {
        try
        {
            return fields.GetProperty(key).GetProperty("doubleValue").GetDouble();
        }
        catch
        {
            return 0;
        }
    }

    private int GetIntValue(JsonElement fields, string key)
    {
        try
        {
            return (int)fields.GetProperty(key).GetProperty("integerValue").GetInt64();
        }
        catch
        {
            return 0;
        }
    }

    private bool GetBoolValue(JsonElement fields, string key)
    {
        try
        {
            return fields.GetProperty(key).GetProperty("booleanValue").GetBoolean();
        }
        catch
        {
            return false;
        }
    }

    private DateTime GetDateTimeValue(JsonElement fields, string key)
    {
        try
        {
            var timestamp = fields.GetProperty(key).GetProperty("timestampValue").GetString();
            return DateTime.Parse(timestamp ?? DateTime.UtcNow.ToString("O"));
        }
        catch
        {
            return DateTime.UtcNow;
        }
    }

    // ==================== UPDATE OPERATIONS ====================

    /// <summary>
    /// Update an existing user document
    /// </summary>
    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            Debug.WriteLine($"[FirestoreService] Updating user: {user.Email}");
            return await SaveUserAsync(user); // PATCH operation handles create or update
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] Error updating user: {ex.Message}");
            return false;
        }
    }

    // ==================== DELETE OPERATIONS ====================

    /// <summary>
    /// Delete a user document
    /// </summary>
    public async Task<bool> DeleteUserAsync(string email)
    {
        try
        {
            Debug.WriteLine($"[FirestoreService] Deleting user: {email}");
            
            var documentId = Uri.EscapeDataString(email);
            var url = $"{_baseUrl}/{UsersCollection}/{documentId}?key={_apiKey}";
            var response = await _httpClient.DeleteAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[FirestoreService] ✅ User {email} deleted successfully");
                return true;
            }
            
            Debug.WriteLine($"[FirestoreService] ❌ Error deleting user - Status: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] ❌ Exception deleting user: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete a loan document
    /// </summary>
    public async Task<bool> DeleteLoanAsync(string loanId)
    {
        try
        {
            Debug.WriteLine($"[FirestoreService] Deleting loan: {loanId}");
            
            var url = $"{_baseUrl}/{LoansCollection}/{loanId}?key={_apiKey}";
            var response = await _httpClient.DeleteAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[FirestoreService] ✅ Loan {loanId} deleted successfully");
                return true;
            }
            
            Debug.WriteLine($"[FirestoreService] ❌ Error deleting loan - Status: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] ❌ Exception deleting loan: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete a transaction document
    /// </summary>
    public async Task<bool> DeleteTransactionAsync(string transactionId)
    {
        try
        {
            Debug.WriteLine($"[FirestoreService] Deleting transaction: {transactionId}");
            
            var url = $"{_baseUrl}/{TransactionsCollection}/{transactionId}?key={_apiKey}";
            var response = await _httpClient.DeleteAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[FirestoreService] ✅ Transaction {transactionId} deleted successfully");
                return true;
            }
            
            Debug.WriteLine($"[FirestoreService] ❌ Error deleting transaction - Status: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] ❌ Exception deleting transaction: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get all loans for a user (as lender or borrower)
    /// </summary>
    public async Task<List<LoanRequest>> GetUserLoansAsync(string email)
    {
        try
        {
            Debug.WriteLine($"[FirestoreService] Getting loans for user: {email}");
            var loans = new List<LoanRequest>();
            
            // For now, return empty list - will be implemented when loan collection is created
            // TODO: Query loans where LenderEmail == email OR BorrowerEmail == email
            
            Debug.WriteLine($"[FirestoreService] Found {loans.Count} loans for user");
            return loans;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] Error getting user loans: {ex.Message}");
            return new List<LoanRequest>();
        }
    }

    /// <summary>
    /// Get all transactions for a user
    /// </summary>
    public async Task<List<Transaction>> GetUserTransactionsAsync(string email)
    {
        try
        {
            Debug.WriteLine($"[FirestoreService] Getting transactions for user: {email}");
            
            // Get all transactions and filter by user email
            var url = $"{_baseUrl}/{TransactionsCollection}?key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return ParseTransactionsFromJson(json, email);
            }
            
            return new List<Transaction>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] Error getting user transactions: {ex.Message}");
            return new List<Transaction>();
        }
    }

    private List<Transaction> ParseTransactionsFromJson(string json, string userEmail)
    {
        var transactions = new List<Transaction>();
        
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("documents", out var documents))
            {
                foreach (var document in documents.EnumerateArray())
                {
                    if (document.TryGetProperty("fields", out var fields))
                    {
                        var fromUserId = GetStringValue(fields, "FromUserId");
                        var toUserId = GetStringValue(fields, "ToUserId");
                        
                        // Only include transactions involving this user
                        if (fromUserId == userEmail || toUserId == userEmail)
                        {
                            transactions.Add(new Transaction
                            {
                                Id = ExtractDocumentId(document.GetRawText()),
                                LoanRequestId = GetStringValue(fields, "LoanRequestId"),
                                FromUserId = fromUserId,
                                ToUserId = toUserId,
                                Amount = (decimal)GetDoubleValue(fields, "Amount"),
                                Type = (TransactionType)GetIntValue(fields, "Type"),
                                Status = (TransactionStatus)GetIntValue(fields, "Status"),
                                CreatedDate = GetDateTimeValue(fields, "CreatedDate"),
                                Description = GetStringValue(fields, "Description")
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] Error parsing transactions: {ex.Message}");
        }
        
        return transactions;
    }
}
