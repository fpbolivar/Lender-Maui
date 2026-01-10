namespace Lender.Models;

/// <summary>
/// Represents a transaction in the loan system (loan funding, repayment, etc.)
/// </summary>
public class Transaction
{
    /// <summary>
    /// Unique transaction ID - Primary key in Firestore
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Associated loan ID - Foreign key to LoanRequests collection
    /// </summary>
    public string LoanRequestId { get; set; } = string.Empty;

    /// <summary>
    /// User sending the money - Foreign key to Users collection
    /// </summary>
    public string FromUserId { get; set; } = string.Empty;

    /// <summary>
    /// User receiving the money - Foreign key to Users collection
    /// </summary>
    public string ToUserId { get; set; } = string.Empty;

    /// <summary>
    /// Transaction amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Type of transaction
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Current status of the transaction
    /// </summary>
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    /// <summary>
    /// When the transaction was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the transaction completed
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Transaction description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Payment merchant/method (if applicable)
    /// </summary>
    public string Merchant { get; set; } = string.Empty;

    /// <summary>
    /// Transaction category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Monthly payment number (for repayments)
    /// </summary>
    public int PaymentNumber { get; set; } = 0;

    /// <summary>
    /// Principal amount in this payment
    /// </summary>
    public decimal PrincipalAmount { get; set; } = 0;

    /// <summary>
    /// Interest amount in this payment
    /// </summary>
    public decimal InterestAmount { get; set; } = 0;

    /// <summary>
    /// Reference number for tracking
    /// </summary>
    public string ReferenceNumber { get; set; } = string.Empty;

    // Collateral metadata (optional)
    public bool HasCollateral { get; set; } = false;
    public string? CollateralDescription { get; set; }
    public string? CollateralImageId { get; set; }

    // Canonical document context
    public string? Mode { get; set; } // Send / Request
    public string? PetitionerEmail { get; set; }
    public string? RequesterEmail { get; set; }
    
    // Extended transaction fields from database
    public decimal TotalPayment { get; set; } = 0;
    public decimal TotalInterest { get; set; } = 0;
    public decimal InterestRate { get; set; } = 0;
    public string? InterestMethod { get; set; } // "Total %", "Monthly %", etc.
    public string? InterestType { get; set; } // "Simple", "Compound", etc.
    public string? PaybackDuration { get; set; }
    public bool IsDaysDuration { get; set; } = false;
    public string? PaymentFrequencyLabel { get; set; }
    public int PaymentsPerYear { get; set; } = 0;
    public decimal PeriodicPayment { get; set; } = 0;
    public int TotalPayments { get; set; } = 0;
    
    // Requester information
    public string? RequesterName { get; set; }
    public string? RequesterPhone { get; set; }
    public string? RequesterAddress { get; set; }
    public string? RequesterIdNumber { get; set; }
    
    // Petitioner information
    public string? PetitionerName { get; set; }
    public string? PetitionerPhone { get; set; }
    
    // Notification info
    public string? NotificationTarget { get; set; }
    public string? NotificationType { get; set; }}
