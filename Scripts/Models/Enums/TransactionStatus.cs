namespace Lender.Models;

/// <summary>
/// Current processing state of a transaction.
/// </summary>
public enum TransactionStatus
{
    Pending,      // Waiting to be processed
    Processing,   // Currently processing
    Completed,    // Successfully completed
    Failed,       // Transaction failed
    Cancelled,    // Cancelled by user
    Refunded      // Refunded back to user
}
