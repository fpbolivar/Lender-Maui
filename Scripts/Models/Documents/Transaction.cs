using Lender.ViewModels;

namespace Lender.Documents;

/// <summary>
/// Canonical transaction document used for submission, retrieval, and PDF rendering.
/// </summary>
public class Transaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    // Loan core
    public string Mode { get; set; } = string.Empty; // Request / Send
    public decimal Amount { get; set; }
    public string PaybackType { get; set; } = string.Empty; // By Date / By Time
    public DateTime? PaybackDate { get; set; }
    public string? PaybackDuration { get; set; }
    public bool IsDaysDuration { get; set; }
    public string PaybackDisplay { get; set; } = string.Empty;

    // Interest
    public string InterestType { get; set; } = string.Empty; // Simple / Compound
    public decimal InterestRate { get; set; }
    public string InterestMethod { get; set; } = string.Empty; // Total % / APR
    public string InterestTypeDisplay { get; set; } = string.Empty;
    public string InterestMethodDisplay { get; set; } = string.Empty;

    // Payments
    public string PaymentFrequencyLabel { get; set; } = string.Empty;
    public int PaymentsPerYear { get; set; }
    public int TotalPayments { get; set; }
    public decimal PeriodicPayment { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalPayment { get; set; }

    // Collateral
    public bool HasCollateral { get; set; }
    public string CollateralDisplay { get; set; } = string.Empty;
    public string? CollateralDescription { get; set; }
    public string? CollateralImageBase64 { get; set; }
    public string? CollateralImageId { get; set; }

    // Requester (the second person info)
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterPhone { get; set; } = string.Empty;
    public string RequesterEmail { get; set; } = string.Empty;
    public string? RequesterAddress { get; set; }
    public string? RequesterIdNumber { get; set; }

    // Petitioner (logged-in user)
    public string PetitionerName { get; set; } = string.Empty;
    public string PetitionerPhone { get; set; } = string.Empty;
    public string PetitionerEmail { get; set; } = string.Empty;

    // Notification
    public string NotificationType { get; set; } = string.Empty;
    public string NotificationTarget { get; set; } = string.Empty; // Phone or Email depending on type

    public static Transaction FromViewModel(LoanFormViewModel vm, string petitionerName, string petitionerEmail, string petitionerPhone)
    {
        decimal.TryParse(vm.AmountText, out var amount);

        return new Transaction
        {
            Mode = vm.Mode,
            Amount = amount,
            PaybackType = vm.IsByDateSelected ? "By Date" : "By Time",
            PaybackDate = vm.IsByDateSelected ? vm.PaybackDate : null,
            PaybackDuration = vm.IsByTimeSelected ? vm.TimeDuration : null,
            IsDaysDuration = vm.IsDaysSelected,
            PaybackDisplay = vm.PaybackDisplay,

            InterestType = vm.InterestType.ToString(),
            InterestRate = vm.InterestRate,
            InterestMethod = vm.InterestMethodDisplay,
            InterestTypeDisplay = vm.InterestTypeDisplay,
            InterestMethodDisplay = vm.InterestMethodDisplay,

            PaymentFrequencyLabel = vm.SelectedPaymentFrequencyLabel,
            PaymentsPerYear = vm.SelectedPaymentFrequency?.PaymentsPerYear ?? 0,
            TotalPayments = vm.TotalPayments,
            PeriodicPayment = vm.PeriodicPayment,
            TotalInterest = vm.TotalInterest,
            TotalPayment = vm.TotalPayment,

            HasCollateral = vm.IsYesCollateralSelected,
            CollateralDisplay = vm.CollateralDisplay,
            CollateralDescription = vm.IsYesCollateralSelected ? vm.CollateralDescription : null,
            CollateralImageBase64 = vm.IsYesCollateralSelected && !string.IsNullOrWhiteSpace(vm.CollateralImageBase64) ? vm.CollateralImageBase64 : null,
            CollateralImageId = vm.IsYesCollateralSelected && !string.IsNullOrWhiteSpace(vm.CollateralImageId) ? vm.CollateralImageId : null,

            RequesterName = vm.RequesterName,
            RequesterPhone = vm.RequesterPhone,
            RequesterEmail = vm.RequesterEmail,
            RequesterAddress = vm.HasAddress ? vm.FullAddress : null,
            RequesterIdNumber = vm.HasIdNumber ? vm.RequesterIdNumber : null,

            PetitionerName = petitionerName,
            PetitionerPhone = petitionerPhone,
            PetitionerEmail = petitionerEmail,

            NotificationType = vm.NotificationTypeDisplay,
            NotificationTarget = vm.IsSMSNotificationSelected
                ? vm.RequesterPhone
                : vm.IsEmailNotificationSelected ? vm.RequesterEmail : string.Empty,

            Status = TransactionStatus.Pending
        };
    }
}
