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
            var url = $"{_baseUrl}/{UsersCollection}/{user.Id}?key={_apiKey}";
            var payload = UserToJson(user);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PatchAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"User {user.Email} saved to Firestore");
                return true;
            }
            
            Debug.WriteLine($"Error saving user: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception saving user: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get a user by ID
    /// </summary>
    public async Task<User?> GetUserAsync(string userId)
    {
        try
        {
            var url = $"{_baseUrl}/{UsersCollection}/{userId}?key={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonToUser(userId, content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting user: {ex.Message}");
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
                Debug.WriteLine($"Loan created with ID: {docId}");
                return docId;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating loan: {ex.Message}");
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
            Debug.WriteLine($"Error getting loan: {ex.Message}");
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
            Debug.WriteLine($"Error updating loan: {ex.Message}");
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
                Debug.WriteLine($"Transaction created with ID: {docId}");
                return docId;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating transaction: {ex.Message}");
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
                Debug.WriteLine($"Investment created with ID: {docId}");
                return docId;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating investment: {ex.Message}");
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
            Debug.WriteLine($"Error saving budget: {ex.Message}");
            return false;
        }
    }

    // ==================== JSON CONVERSION METHODS ====================

    private string UserToJson(User user)
    {
        var fields = new Dictionary<string, object>
        {
            { "fullName", new { stringValue = user.FullName } },
            { "email", new { stringValue = user.Email } },
            { "phoneNumber", new { stringValue = user.PhoneNumber } },
            { "dateOfBirth", new { timestampValue = user.DateOfBirth.ToString("O") } },
            { "balance", new { doubleValue = (double)user.Balance } },
            { "creditScore", new { doubleValue = (double)user.CreditScore } },
            { "loansGiven", new { integerValue = user.LoansGiven } },
            { "loansReceived", new { integerValue = user.LoansReceived } },
            { "joinDate", new { timestampValue = user.JoinDate.ToString("O") } },
            { "status", new { stringValue = user.Status.ToString() } },
            { "isVerified", new { booleanValue = user.IsVerified } },
            { "totalLent", new { doubleValue = (double)user.TotalLent } },
            { "totalBorrowed", new { doubleValue = (double)user.TotalBorrowed } }
        };

        return JsonSerializer.Serialize(new { fields });
    }

    private User? JsonToUser(string userId, string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var fields = root.GetProperty("fields");

            return new User
            {
                Id = userId,
                FullName = GetStringValue(fields, "fullName"),
                Email = GetStringValue(fields, "email"),
                PhoneNumber = GetStringValue(fields, "phoneNumber"),
                DateOfBirth = GetDateTimeValue(fields, "dateOfBirth"),
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
            Debug.WriteLine($"Error parsing user JSON: {ex.Message}");
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
            Debug.WriteLine($"Error parsing loan JSON: {ex.Message}");
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
}
