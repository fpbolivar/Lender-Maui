using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Lender.Helpers;
using Lender.Models;
using Lender.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Lender.ViewModels;

public class TransactionDetailViewModel : NavigableViewModel
{
    public string TransactionId { get; }
    public string TransactionType { get; }
    public string Status { get; }
    public string AmountDisplay { get; }
    public string TotalPaymentDisplay { get; }
    public string TotalInterestDisplay { get; }
    public string CounterpartyName { get; }
    public string CounterpartyEmail { get; }
    public string InterestRateDisplay { get; }
    public string InterestMethodDisplay { get; }
    public string InterestTypeDisplay { get; }
    public string CreatedDateDisplay { get; }
    public string Mode { get; }
    
    // Duration and Payment Info
    public string DurationDisplay { get; }
    public string PaymentFrequencyDisplay { get; }
    public string PeriodicPaymentDisplay { get; }
    public string TotalPaymentsDisplay { get; }
    
    // Requester/Petitioner Info
    public string RequesterName { get; }
    public string RequesterEmail { get; }
    public string RequesterPhone { get; }
    public string RequesterAddress { get; }
    public string RequesterIdNumber { get; }
    
    public string PetitionerName { get; }
    public string PetitionerEmail { get; }
    public string PetitionerPhone { get; }
    
    // Notification Info
    public string NotificationTarget { get; }
    public string NotificationType { get; }

    public bool HasCollateral { get; }
    public string? CollateralDescription { get; }
    public string? CollateralImageId { get; }
    public string? CollateralOwnerEmail { get; }

    private readonly Transaction _transaction;

    public ICommand GoBackCommand { get; }
    public ICommand DownloadCollateralCommand { get; }
    public ICommand PreviewPdfCommand { get; }

    public TransactionDetailViewModel(TransactionDisplayModel displayModel)
    {
        var authService = ServiceHelper.GetService<IAuthenticationService>();
        var currentEmail = authService?.CurrentUserEmail ?? string.Empty;

        var txn = displayModel.UnderlyingTransaction;
        _transaction = txn;
        
        TransactionId = txn.Id;
        TransactionType = displayModel.TransactionTypeDisplay;
        Status = displayModel.Status;
        AmountDisplay = displayModel.Amount.ToString("C2");
        CounterpartyName = displayModel.OtherPartyName;
        CounterpartyEmail = displayModel.OtherPartyEmail;
        InterestRateDisplay = $"{txn.InterestRate:F2}%";
        CreatedDateDisplay = txn.CreatedDate.ToLocalTime().ToString("MMM dd, yyyy hh:mm tt");
        Mode = txn.Mode ?? string.Empty;
        
        // Extended fields from transaction
        TotalPaymentDisplay = txn.TotalPayment > 0 ? txn.TotalPayment.ToString("C2") : "N/A";
        TotalInterestDisplay = txn.TotalInterest > 0 ? txn.TotalInterest.ToString("C2") : "N/A";
        InterestMethodDisplay = txn.InterestMethod ?? "N/A";
        InterestTypeDisplay = txn.InterestType ?? "N/A";
        
        DurationDisplay = !string.IsNullOrWhiteSpace(txn.PaybackDuration) 
            ? $"{txn.PaybackDuration} {(txn.IsDaysDuration ? "days" : "months")}"
            : "N/A";
        PaymentFrequencyDisplay = txn.PaymentFrequencyLabel ?? "N/A";
        PeriodicPaymentDisplay = txn.PeriodicPayment > 0 ? txn.PeriodicPayment.ToString("C2") : "N/A";
        TotalPaymentsDisplay = txn.TotalPayments > 0 ? txn.TotalPayments.ToString() : "N/A";
        
        RequesterName = txn.RequesterName ?? "N/A";
        RequesterEmail = txn.RequesterEmail ?? "N/A";
        RequesterPhone = txn.RequesterPhone ?? "N/A";
        RequesterAddress = txn.RequesterAddress ?? "N/A";
        RequesterIdNumber = txn.RequesterIdNumber ?? "N/A";
        
        PetitionerName = txn.PetitionerName ?? "N/A";
        PetitionerEmail = txn.PetitionerEmail ?? "N/A";
        PetitionerPhone = txn.PetitionerPhone ?? "N/A";
        
        NotificationTarget = txn.NotificationTarget ?? "N/A";
        NotificationType = txn.NotificationType ?? "N/A";

        HasCollateral = displayModel.HasCollateral;
        CollateralDescription = displayModel.CollateralName;
        CollateralImageId = displayModel.CollateralImageId;
        // Determine best owner email for storage path
        // Prefer petitioner email for uploads; fallback to owner from display model
        if (!string.IsNullOrWhiteSpace(txn.PetitionerEmail))
            CollateralOwnerEmail = txn.PetitionerEmail;
        else if (!string.IsNullOrWhiteSpace(displayModel.CollateralOwnerEmail))
            CollateralOwnerEmail = displayModel.CollateralOwnerEmail;
        else
            CollateralOwnerEmail = txn.FromUserId;

        GoBackCommand = new Command(async () => await Shell.Current.Navigation.PopAsync());
        DownloadCollateralCommand = new Command(async () => await DownloadCollateralAsync());
        PreviewPdfCommand = new Command(async () => await PreviewPdfAsync());
    }

    private async Task DownloadCollateralAsync()
    {
        string storagePath = string.Empty;
        try
        {
            if (!HasCollateral || string.IsNullOrWhiteSpace(CollateralImageId) || string.IsNullOrWhiteSpace(CollateralOwnerEmail))
            {
                var page = Shell.Current?.CurrentPage;
                if (page != null)
                {
                    await page.DisplayAlertAsync("Unavailable", "No collateral file available to download.", "OK");
                }
                return;
            }

            storagePath = FirebaseStorageService.BuildStoragePath(CollateralOwnerEmail, CollateralImageId);
            var storage = new FirebaseStorageService();
            var (bytes, error) = await storage.GetImageBytesAsync(storagePath);
            if (bytes == null || bytes.Length == 0)
            {
                Debug.WriteLine($"[TransactionDetailViewModel] Collateral download failed | Path: {storagePath} | Error: {error} - TransactionDetailViewModel.cs:136");
                var page = Shell.Current?.CurrentPage;
                if (page != null)
                {
                    var errorMsg = error ?? "File not found or empty.";
                    if (error?.Contains("403") == true)
                    {
                        errorMsg = $"Access denied. Check Storage permissions.\n{error}";
                    }
                    await page.DisplayAlertAsync("Download failed", errorMsg, "OK");
                }
                return;
            }

            var ext = Path.GetExtension(CollateralImageId).Trim('.').ToLowerInvariant();
            var contentType = ext == "pdf" ? "application/pdf" : "image/jpeg";
            var fileName = $"collateral_{CollateralImageId}";
            var fullPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(fullPath, bytes);

            await Launcher.OpenAsync(new OpenFileRequest
            {
                Title = "Collateral",
                File = new ReadOnlyFile(fullPath, contentType)
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TransactionDetailViewModel] Download collateral failed: {ex.Message} | Path: {storagePath} - TransactionDetailViewModel.cs:164");
            var page = Shell.Current?.CurrentPage;
            if (page != null)
            {
                await page.DisplayAlertAsync("Error", $"Failed to open collateral file.\nError: {ex.Message}", "OK");
            }
        }
    }

    private async Task PreviewPdfAsync()
    {
        try
        {
            // Convert Lender.Models.Transaction to Lender.Documents.Transaction for PDF generation
            var pdfTransaction = new Lender.Documents.Transaction
            {
                Id = _transaction.Id,
                CreatedAt = _transaction.CreatedDate,
                Status = _transaction.Status == Models.TransactionStatus.Pending 
                    ? Lender.Documents.TransactionStatus.Pending 
                    : _transaction.Status == Models.TransactionStatus.Completed
                    ? Lender.Documents.TransactionStatus.Completed
                    : _transaction.Status == Models.TransactionStatus.Processing
                    ? Lender.Documents.TransactionStatus.InProgress
                    : Lender.Documents.TransactionStatus.Cancelled,
                Mode = _transaction.Mode ?? string.Empty,
                Amount = _transaction.Amount,
                PaybackType = "By Time",
                PaybackDuration = _transaction.PaybackDuration,
                IsDaysDuration = _transaction.IsDaysDuration,
                PaybackDisplay = DurationDisplay,
                InterestType = _transaction.InterestType ?? string.Empty,
                InterestRate = _transaction.InterestRate,
                InterestMethod = _transaction.InterestMethod ?? string.Empty,
                InterestTypeDisplay = InterestTypeDisplay,
                InterestMethodDisplay = InterestMethodDisplay,
                PaymentFrequencyLabel = _transaction.PaymentFrequencyLabel ?? string.Empty,
                PaymentsPerYear = 12, // Default, can be adjusted based on frequency
                TotalPayments = _transaction.TotalPayments,
                PeriodicPayment = _transaction.PeriodicPayment,
                TotalInterest = _transaction.TotalInterest,
                TotalPayment = _transaction.TotalPayment,
                HasCollateral = HasCollateral,
                CollateralDisplay = CollateralDescription ?? "None",
                CollateralDescription = _transaction.CollateralDescription,
                CollateralImageId = _transaction.CollateralImageId,
                RequesterName = _transaction.RequesterName ?? string.Empty,
                RequesterPhone = _transaction.RequesterPhone ?? string.Empty,
                RequesterEmail = _transaction.RequesterEmail ?? string.Empty,
                RequesterAddress = _transaction.RequesterAddress,
                RequesterIdNumber = _transaction.RequesterIdNumber,
                PetitionerName = _transaction.PetitionerName ?? string.Empty,
                PetitionerPhone = _transaction.PetitionerPhone ?? string.Empty,
                PetitionerEmail = _transaction.PetitionerEmail ?? string.Empty,
                NotificationType = _transaction.NotificationType ?? string.Empty,
                NotificationTarget = _transaction.NotificationTarget ?? string.Empty
            };

            var pdfService = new LoanPdfService();
            var pdfPath = await pdfService.GenerateLoanPdf(pdfTransaction);

            if (File.Exists(pdfPath))
            {
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    Title = "Transaction Summary PDF",
                    File = new ReadOnlyFile(pdfPath, "application/pdf")
                });
            }
            else
            {
                var page = Shell.Current?.CurrentPage;
                if (page != null)
                {
                    await page.DisplayAlertAsync("Error", "Failed to generate PDF.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TransactionDetailViewModel] PDF preview failed: {ex.Message}");
            var page = Shell.Current?.CurrentPage;
            if (page != null)
            {
                await page.DisplayAlertAsync("Error", $"Failed to generate PDF.\nError: {ex.Message}", "OK");
            }
        }
    }
}
