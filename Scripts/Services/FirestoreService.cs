using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Lender.Models;
using Lender.Services.Constants;
using DocTransaction = Lender.Documents.Transaction;

namespace Lender.Services;

/// <summary>
/// Service for managing Firestore database operations via REST API
/// Handles CRUD operations for all collections
/// </summary>
public class FirestoreService
{
    private static FirestoreService? _instance;
    private readonly string _baseUrl;
    private HttpClient _httpClient;

    // Collections and constants centralized in FirestoreCollections and FirestoreConstants


    public static FirestoreService Instance => _instance ??= new FirestoreService();

    public FirestoreService()
    {
        _baseUrl = $"https://firestore.googleapis.com/v1/projects/{FirestoreConstants.ProjectId}/databases/(default)/documents";
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
            Debug.WriteLine($"[FirestoreService] ===== START SAVE USER ===== - FirestoreService.cs:40");
            Debug.WriteLine($"[FirestoreService] Starting SaveUserAsync for user: {user.Email} (ID: {user.Id}) - FirestoreService.cs:41");
            
            // Use email as document ID for easier access
            var documentId = Uri.EscapeDataString(user.Email);
            var url = $"{_baseUrl}/{FirestoreCollections.Users}/{documentId}?key={FirestoreConstants.ApiKey}";
            Debug.WriteLine($"[FirestoreService] URL: {url} - FirestoreService.cs:46");
            
            var payload = UserToJson(user);
            Debug.WriteLine($"[FirestoreService] Payload length: {payload.Length} chars - FirestoreService.cs:49");
            Debug.WriteLine($"[FirestoreService] Payload: {payload} - FirestoreService.cs:50");
            
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            Debug.WriteLine($"[FirestoreService] Sending PATCH request... - FirestoreService.cs:54");
            var response = await _httpClient.PatchAsync(url, content);
            Debug.WriteLine($"[FirestoreService] Response Status: {response.StatusCode} ({(int)response.StatusCode}) - FirestoreService.cs:56");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[FirestoreService] Success! Response length: {responseContent.Length} - FirestoreService.cs:61");
                Debug.WriteLine($"[FirestoreService] Response: {responseContent} - FirestoreService.cs:62");
                Debug.WriteLine($"[FirestoreService] ✅ User {user.Email} saved to Firestore successfully - FirestoreService.cs:63");
                Debug.WriteLine($"[FirestoreService] ===== END SAVE USER SUCCESS ===== - FirestoreService.cs:64");
                return true;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[FirestoreService] ❌ Error saving user  Status: {response.StatusCode} - FirestoreService.cs:69");
            Debug.WriteLine($"[FirestoreService] Error response length: {errorContent.Length} - FirestoreService.cs:70");
            Debug.WriteLine($"[FirestoreService] Error response: {errorContent} - FirestoreService.cs:71");
            Debug.WriteLine($"[FirestoreService] ===== END SAVE USER FAIL ===== - FirestoreService.cs:72");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] ❌ EXCEPTION saving user: {ex.GetType().Name} - FirestoreService.cs:77");
            Debug.WriteLine($"[FirestoreService] Exception message: {ex.Message} - FirestoreService.cs:78");
            Debug.WriteLine($"[FirestoreService] Stack trace: {ex.StackTrace} - FirestoreService.cs:79");
            Debug.WriteLine($"[FirestoreService] ===== END SAVE USER EXCEPTION ===== - FirestoreService.cs:80");
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
            Debug.WriteLine($"[FirestoreService] Getting user: {email} - FirestoreService.cs:92");
            
            var documentId = Uri.EscapeDataString(email);
            var url = $"{_baseUrl}/{FirestoreCollections.Users}/{documentId}?key={FirestoreConstants.ApiKey}";
            var response = await _httpClient.GetAsync(url);
            
            Debug.WriteLine($"[FirestoreService] Get user response status: {response.StatusCode} - FirestoreService.cs:98");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[FirestoreService] Error getting user: {errorContent} - FirestoreService.cs:103");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[FirestoreService] User data received, parsing... - FirestoreService.cs:108");
            return JsonToUser(email, content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] Exception getting user: {ex.Message} - FirestoreService.cs:113");
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
            var url = $"{_baseUrl}/{FirestoreCollections.Loans}?key={FirestoreConstants.ApiKey}";
            var payload = LoanToJson(loan);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var docId = ExtractDocumentId(responseContent);
                Debug.WriteLine($"Loan created with ID: {docId} - FirestoreService.cs:136");
                return docId;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating loan: {ex.Message} - FirestoreService.cs:144");
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
            var url = $"{_baseUrl}/{FirestoreCollections.Loans}/{loanId}?key={FirestoreConstants.ApiKey}";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonToLoan(loanId, content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting loan: {ex.Message} - FirestoreService.cs:166");
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
            var url = $"{_baseUrl}/{FirestoreCollections.Loans}/{loan.Id}?key={FirestoreConstants.ApiKey}";
            var payload = LoanToJson(loan);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PatchAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating loan: {ex.Message} - FirestoreService.cs:187");
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
            var url = $"{_baseUrl}/{FirestoreCollections.Transactions}?key={FirestoreConstants.ApiKey}";
            var payload = TransactionToJson(transaction);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var docId = ExtractDocumentId(responseContent);
                Debug.WriteLine($"Transaction created with ID: {docId} - FirestoreService.cs:210");
                return docId;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating transaction: {ex.Message} - FirestoreService.cs:218");
            return null;
        }
    }

    /// <summary>
    /// Record a transaction document (loan request flow) with full details
    /// </summary>
    public async Task<string?> CreateTransactionDocumentAsync(DocTransaction transaction)
    {
        try
        {
            var url = $"{_baseUrl}/{FirestoreCollections.Transactions}?key={FirestoreConstants.ApiKey}";
            var payload = TransactionDocumentToJson(transaction);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var docId = ExtractDocumentId(responseContent);
                Debug.WriteLine($"Transaction doc created with ID: {docId} - FirestoreService.cs:239");
                return docId;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Error creating transaction doc: {errorContent} - FirestoreService.cs:244");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating transaction doc: {ex.Message} - FirestoreService.cs:249");
            return null;
        }
    }

    /// <summary>
    /// Create a transaction document in a user's sub-collection for easy access
    /// </summary>
    public async Task<string?> CreateUserTransactionDocumentAsync(string userEmail, DocTransaction transaction)
    {
        try
        {
            var documentId = Uri.EscapeDataString(userEmail);
            var url = $"{_baseUrl}/{FirestoreCollections.Users}/{documentId}/transactions?key={FirestoreConstants.ApiKey}";
            var payload = TransactionDocumentToJson(transaction);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var docId = ExtractDocumentId(responseContent);
                Debug.WriteLine($"User transaction doc created with ID: {docId} - FirestoreService.cs:279");
                return docId;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Error creating user transaction doc: {errorContent} - FirestoreService.cs:284");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating user transaction doc: {ex.Message} - FirestoreService.cs:289");
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
            var url = $"{_baseUrl}/{FirestoreCollections.Investments}?key={FirestoreConstants.ApiKey}";
            var payload = InvestmentToJson(investment);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var docId = ExtractDocumentId(responseContent);
                Debug.WriteLine($"Investment created with ID: {docId} - FirestoreService.cs:272");
                return docId;
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating investment: {ex.Message} - FirestoreService.cs:280");
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
            var url = $"{_baseUrl}/{FirestoreCollections.Budgets}/{budget.Id}?key={FirestoreConstants.ApiKey}";
            var payload = BudgetToJson(budget);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PatchAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving budget: {ex.Message} - FirestoreService.cs:303");
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
        Debug.WriteLine($"[FirestoreService] Generated User JSON: {json} - FirestoreService.cs:341");
        return json;
    }

    private string TransactionDocumentToJson(DocTransaction t)
    {
#pragma warning disable CS8604 // Possible null reference argument - nulls are removed from dictionary in cleanup step below
        var fields = new Dictionary<string, object>
        {
            { "id", new { stringValue = t.Id } },
            { "createdAt", new { timestampValue = t.CreatedAt.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") } },
            { "status", new { stringValue = t.Status.ToString() } },

            // Loan core
            { "mode", new { stringValue = t.Mode } },
            { "amount", new { doubleValue = (double)t.Amount } },
            { "paybackDate", t.PaybackDate.HasValue ? new { timestampValue = t.PaybackDate.Value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") } : (object?)null },
            { "paybackDuration", string.IsNullOrWhiteSpace(t.PaybackDuration) ? (object?)null : new { stringValue = t.PaybackDuration } },
            { "isDaysDuration", new { booleanValue = t.IsDaysDuration } },
            // Omit display-only field to avoid duplication
            // { "paybackDisplay", new { stringValue = t.PaybackDisplay } },

            // Interest
            { "interestType", new { stringValue = t.InterestType } },
            { "interestRate", new { doubleValue = (double)t.InterestRate } },
            { "interestMethod", new { stringValue = t.InterestMethod } },
            // Omit display-only fields to avoid duplication
            // { "interestTypeDisplay", new { stringValue = t.InterestTypeDisplay } },
            // { "interestMethodDisplay", new { stringValue = t.InterestMethodDisplay } },

            // Payments
            { "paymentFrequencyLabel", new { stringValue = t.PaymentFrequencyLabel } },
            { "paymentsPerYear", new { integerValue = t.PaymentsPerYear } },
            { "totalPayments", new { integerValue = t.TotalPayments } },
            { "periodicPayment", new { doubleValue = (double)t.PeriodicPayment } },
            { "totalInterest", new { doubleValue = (double)t.TotalInterest } },
            { "totalPayment", new { doubleValue = (double)t.TotalPayment } },

            // Collateral
            { "hasCollateral", new { booleanValue = t.HasCollateral } },
            // Omit display-only field to avoid duplication
            // { "collateralDisplay", new { stringValue = t.CollateralDisplay } },
            { "collateralDescription", string.IsNullOrWhiteSpace(t.CollateralDescription) ? (object?)null : new { stringValue = t.CollateralDescription } },
            { "collateralImageId", string.IsNullOrWhiteSpace(t.CollateralImageId) ? (object?)null : new { stringValue = t.CollateralImageId } },

            // Requester
            { "requesterName", new { stringValue = t.RequesterName } },
            { "requesterPhone", new { stringValue = t.RequesterPhone } },
            { "requesterEmail", new { stringValue = t.RequesterEmail } },
            { "requesterAddress", string.IsNullOrWhiteSpace(t.RequesterAddress) ? (object?)null : new { stringValue = t.RequesterAddress } },
            { "requesterIdNumber", string.IsNullOrWhiteSpace(t.RequesterIdNumber) ? (object?)null : new { stringValue = t.RequesterIdNumber } },

            // Petitioner
            { "petitionerName", new { stringValue = t.PetitionerName } },
            { "petitionerPhone", new { stringValue = t.PetitionerPhone } },
            { "petitionerEmail", new { stringValue = t.PetitionerEmail } },

            // Notification
            { "notificationType", new { stringValue = t.NotificationType } },
            { "notificationTarget", new { stringValue = t.NotificationTarget } }
        };
#pragma warning restore CS8604

        // Remove null entries to avoid Firestore errors
        var cleaned = fields
            .Where(kvp => kvp.Value != null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var document = new { fields = cleaned };
        return JsonSerializer.Serialize(document);
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
            Debug.WriteLine($"Error parsing user JSON: {ex.Message} - FirestoreService.cs:436");
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
            { "riskRating", new { stringValue = loan.RiskRating } },
            // extended fields
            { "isOffer", new { booleanValue = loan.IsOffer } },
            { "interestType", new { stringValue = loan.InterestType.ToString() } },
            { "paymentsPerYear", new { integerValue = loan.PaymentsPerYear } },
            { "termUnit", new { stringValue = loan.TermUnit.ToString() } },
            { "termLength", new { integerValue = loan.TermLength } },
            { "hasDownPayment", new { booleanValue = loan.HasDownPayment } },
            { "downPaymentAmount", new { doubleValue = (double)loan.DownPaymentAmount } },
            { "hasCollateral", new { booleanValue = loan.HasCollateral } },
            { "collateralName", new { stringValue = loan.CollateralName } },
            { "collateralImageUrl", new { stringValue = loan.CollateralImageUrl } },
            { "notes", new { stringValue = loan.Notes } }
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
                RiskRating = GetStringValue(fields, "riskRating"),
                // extended fields
                IsOffer = GetBoolValue(fields, "isOffer"),
                InterestType = Enum.TryParse<Lender.Models.Enums.InterestType>(GetStringValue(fields, "interestType"), out var it) ? it : Lender.Models.Enums.InterestType.Amortized,
                PaymentsPerYear = GetIntValue(fields, "paymentsPerYear"),
                TermUnit = Enum.TryParse<Lender.Models.Enums.TermUnit>(GetStringValue(fields, "termUnit"), out var tu) ? tu : Lender.Models.Enums.TermUnit.Months,
                TermLength = GetIntValue(fields, "termLength"),
                HasDownPayment = GetBoolValue(fields, "hasDownPayment"),
                DownPaymentAmount = (decimal)GetDoubleValue(fields, "downPaymentAmount"),
                HasCollateral = GetBoolValue(fields, "hasCollateral"),
                CollateralName = GetStringValue(fields, "collateralName"),
                CollateralImageUrl = GetStringValue(fields, "collateralImageUrl"),
                Notes = GetStringValue(fields, "notes")
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error parsing loan JSON: {ex.Message} - FirestoreService.cs:511");
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
            Debug.WriteLine($"[FirestoreService] Updating user: {user.Email} - FirestoreService.cs:645");
            return await SaveUserAsync(user); // PATCH operation handles create or update
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] Error updating user: {ex.Message} - FirestoreService.cs:650");
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
            Debug.WriteLine($"[FirestoreService] Deleting user: {email} - FirestoreService.cs:664");
            
            var documentId = Uri.EscapeDataString(email);
            var url = $"{_baseUrl}/{FirestoreCollections.Users}/{documentId}?key={FirestoreConstants.ApiKey}";
            var response = await _httpClient.DeleteAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[FirestoreService] ✅ User {email} deleted successfully - FirestoreService.cs:672");
                return true;
            }
            
            Debug.WriteLine($"[FirestoreService] ❌ Error deleting user  Status: {response.StatusCode} - FirestoreService.cs:676");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] ❌ Exception deleting user: {ex.Message} - FirestoreService.cs:681");
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
            Debug.WriteLine($"[FirestoreService] Deleting loan: {loanId} - FirestoreService.cs:693");
            
            var url = $"{_baseUrl}/{FirestoreCollections.Loans}/{loanId}?key={FirestoreConstants.ApiKey}";
            var response = await _httpClient.DeleteAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[FirestoreService] ✅ Loan {loanId} deleted successfully - FirestoreService.cs:700");
                return true;
            }
            
            Debug.WriteLine($"[FirestoreService] ❌ Error deleting loan  Status: {response.StatusCode} - FirestoreService.cs:704");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] ❌ Exception deleting loan: {ex.Message} - FirestoreService.cs:709");
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
            Debug.WriteLine($"[FirestoreService] Deleting transaction: {transactionId} - FirestoreService.cs:721");
            
            var url = $"{_baseUrl}/{FirestoreCollections.Transactions}/{transactionId}?key={FirestoreConstants.ApiKey}";
            var response = await _httpClient.DeleteAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[FirestoreService] ✅ Transaction {transactionId} deleted successfully - FirestoreService.cs:728");
                return true;
            }
            
            Debug.WriteLine($"[FirestoreService] ❌ Error deleting transaction  Status: {response.StatusCode} - FirestoreService.cs:732");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] ❌ Exception deleting transaction: {ex.Message} - FirestoreService.cs:737");
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
            Debug.WriteLine($"[FirestoreService] Getting loans for user: {email} - FirestoreService.cs:749");

            if (string.IsNullOrWhiteSpace(email))
            {
                return new List<LoanRequest>();
            }

            var url = $"{_baseUrl}/{FirestoreCollections.Loans}?key={FirestoreConstants.ApiKey}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var loans = ParseLoansFromJson(json, email);
                Debug.WriteLine($"[FirestoreService] Found {loans.Count} loans for user - FirestoreService.cs:755");
                return loans;
            }

            Debug.WriteLine($"[FirestoreService] Failed to fetch loans. Status: {response.StatusCode} - FirestoreService.cs:757");
            return new List<LoanRequest>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] Error getting user loans: {ex.Message} - FirestoreService.cs:760");
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
            Debug.WriteLine($"[FirestoreService] Getting transactions for user: {email} - FirestoreService.cs:772");
            
            // Get all transactions and filter by user email
            var url = $"{_baseUrl}/{FirestoreCollections.Transactions}?key={FirestoreConstants.ApiKey}";
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
            Debug.WriteLine($"[FirestoreService] Error getting user transactions: {ex.Message} - FirestoreService.cs:788");
            return new List<Transaction>();
        }
    }

    private List<LoanRequest> ParseLoansFromJson(string json, string userEmail)
    {
        var loans = new List<LoanRequest>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("documents", out var documents))
            {
                return loans;
            }

            foreach (var document in documents.EnumerateArray())
            {
                if (!document.TryGetProperty("fields", out var fields))
                {
                    continue;
                }

                var documentUserId = GetStringValue(fields, "userId");
                var documentUserEmail = GetStringValue(fields, "userEmail");
                var lenderEmail = GetStringValue(fields, "lenderEmail");
                var borrowerEmail = GetStringValue(fields, "borrowerEmail");
                var requesterEmail = GetStringValue(fields, "requesterEmail");
                var petitionerEmail = GetStringValue(fields, "petitionerEmail");

                var matchesUser =
                    string.Equals(documentUserId, userEmail, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(documentUserEmail, userEmail, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(lenderEmail, userEmail, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(borrowerEmail, userEmail, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(requesterEmail, userEmail, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(petitionerEmail, userEmail, StringComparison.OrdinalIgnoreCase);

                if (!matchesUser)
                {
                    continue;
                }

                var name = document.TryGetProperty("name", out var nameEl) ? nameEl.GetString() ?? string.Empty : string.Empty;
                var loanId = name.Split('/').LastOrDefault() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(loanId))
                {
                    continue;
                }

                var loan = JsonToLoan(loanId, document.GetRawText());
                if (loan != null)
                {
                    loans.Add(loan);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] Error parsing loans: {ex.Message} - FirestoreService.cs:823");
        }

        return loans;
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
                        // Canonical transaction document produced by loan request flow
                        var petitionerEmail = GetStringValue(fields, "petitionerEmail");
                        var requesterEmail = GetStringValue(fields, "requesterEmail");
                        var mode = GetStringValue(fields, "mode");

                        // Only include transactions involving this user
                        if (string.Equals(petitionerEmail, userEmail, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(requesterEmail, userEmail, StringComparison.OrdinalIgnoreCase))
                        {
                            // Map Mode to TransactionType for UI: Send => Funding, Request => Repayment
                            var type = mode.Equals("Send", StringComparison.OrdinalIgnoreCase)
                                ? TransactionType.Funding
                                : TransactionType.Repayment;

                            // Map status string to Models.TransactionStatus
                            var statusString = GetStringValue(fields, "status");
                            var status = MapStatus(statusString);

                            // Extract document id from name
                            var name = document.GetProperty("name").GetString() ?? string.Empty;
                            var id = name.Split('/').LastOrDefault() ?? string.Empty;

                            transactions.Add(new Transaction
                            {
                                Id = id,
                                LoanRequestId = GetStringValue(fields, "id"), // using our doc id field as association
                                FromUserId = petitionerEmail,
                                ToUserId = requesterEmail,
                                Amount = (decimal)GetDoubleValue(fields, "amount"),
                                Type = type,
                                Status = status,
                                CreatedDate = GetDateTimeValue(fields, "createdAt"),
                                Description = GetStringValue(fields, "description"),
                                HasCollateral = GetBoolValue(fields, "hasCollateral"),
                                CollateralDescription = GetStringValue(fields, "collateralDescription"),
                                CollateralImageId = GetStringValue(fields, "collateralImageId"),
                                Mode = mode,
                                PetitionerEmail = petitionerEmail,
                                RequesterEmail = requesterEmail,
                                
                                // Extended fields
                                TotalPayment = (decimal)GetDoubleValue(fields, "totalPayment"),
                                TotalInterest = (decimal)GetDoubleValue(fields, "totalInterest"),
                                InterestRate = (decimal)GetDoubleValue(fields, "interestRate"),
                                InterestMethod = GetStringValue(fields, "interestMethod"),
                                InterestType = GetStringValue(fields, "interestType"),
                                PaybackDuration = GetStringValue(fields, "paybackDuration"),
                                IsDaysDuration = GetBoolValue(fields, "isDaysDuration"),
                                PaymentFrequencyLabel = GetStringValue(fields, "paymentFrequencyLabel"),
                                PaymentsPerYear = (int)GetDoubleValue(fields, "paymentsPerYear"),
                                PeriodicPayment = (decimal)GetDoubleValue(fields, "periodicPayment"),
                                TotalPayments = (int)GetDoubleValue(fields, "totalPayments"),
                                
                                // Requester info
                                RequesterName = GetStringValue(fields, "requesterName"),
                                RequesterPhone = GetStringValue(fields, "requesterPhone"),
                                RequesterAddress = GetStringValue(fields, "requesterAddress"),
                                RequesterIdNumber = GetStringValue(fields, "requesterIdNumber"),
                                
                                // Petitioner info
                                PetitionerName = GetStringValue(fields, "petitionerName"),
                                PetitionerPhone = GetStringValue(fields, "petitionerPhone"),
                                
                                // Notification info
                                NotificationTarget = GetStringValue(fields, "notificationTarget"),
                                NotificationType = GetStringValue(fields, "notificationType")
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FirestoreService] Error parsing transactions: {ex.Message} - FirestoreService.cs:831");
        }
        
        return transactions;
    }

    private Lender.Models.TransactionStatus MapStatus(string status)
    {
        return status switch
        {
            "Pending" => Lender.Models.TransactionStatus.Pending,
            "Completed" => Lender.Models.TransactionStatus.Completed,
            "Cancelled" => Lender.Models.TransactionStatus.Cancelled,
            "Active" => Lender.Models.TransactionStatus.Processing,
            "InProgress" => Lender.Models.TransactionStatus.Processing,
            _ => Lender.Models.TransactionStatus.Pending
        };
    }
}
