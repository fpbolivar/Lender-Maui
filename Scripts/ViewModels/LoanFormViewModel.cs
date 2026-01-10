using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Lender.Helpers;
using Lender.Services;
using Lender.Documents;
using Microsoft.Maui.Media;
#if IOS
using Foundation;
using UIKit;
#endif
using Microsoft.Maui.Media;

namespace Lender.ViewModels;

public class LoanFormViewModel : NavigableViewModel
{
    public ICommand BackCommand { get; }
    public ICommand UploadCollateralImageCommand { get; }
    public ICommand PickCollateralImageCommand { get; }
    public ICommand CaptureCollateralImageCommand { get; }
    public ICommand SubmitCommand { get; }
    public ICommand SetRequestModeCommand { get; }
    public ICommand SetSendModeCommand { get; }
    public ICommand SetInterestTypeCommand { get; }
    public ICommand SelectPaymentFrequencyCommand { get; }
    public ICommand SetByDateCommand { get; }
    public ICommand SetByTimeCommand { get; }
    public ICommand SetDaysCommand { get; }
    public ICommand SetMonthsCommand { get; }
    public ICommand SetYesCollateralCommand { get; }
    public ICommand SetNoCollateralCommand { get; }
    public ICommand SetInterestMethodTotalCommand { get; }
    public ICommand SetInterestMethodAPRCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand ContinueToRequesterInfoCommand { get; }
    public ICommand SetNotificationTypeCommand { get; }
    public ICommand ContinueToFinalSummaryCommand { get; }
    public ICommand SubmitLoanRequestCommand { get; }
    public ICommand GeneratePdfCommand { get; }
    public ICommand AutoFillRequesterDemoCommand { get; }

    public bool IsDebugMode
    {
        get
        {
            bool isDebug = false;
#if DEBUG
            isDebug = true;
#endif
            return isDebug;
        }
    }

    private async Task<Transaction> BuildTransactionDocumentAsync()
    {
        // Gather petitioner (logged-in user) info
        string petitionerName = "Not available";
        string petitionerEmail = "Not available";
        string petitionerPhone = "Not available";

        var authService = ServiceHelper.GetService<IAuthenticationService>();
        if (!string.IsNullOrWhiteSpace(authService?.CurrentUserEmail))
        {
            petitionerEmail = authService.CurrentUserEmail;
            var userProfile = await FirestoreService.Instance.GetUserAsync(authService.CurrentUserEmail);
            if (userProfile != null)
            {
                petitionerName = string.IsNullOrWhiteSpace(userProfile.FullName) ? petitionerName : userProfile.FullName;
                petitionerPhone = string.IsNullOrWhiteSpace(userProfile.PhoneNumber) ? petitionerPhone : userProfile.PhoneNumber;
            }
        }

        return Transaction.FromViewModel(this, petitionerName, petitionerEmail, petitionerPhone);
    }

    private string _amountText = "";
    public string AmountText 
    { 
        get => _amountText; 
        set 
        {
            if (value == _amountText) return;
            
            // Only allow numbers and one decimal point
            var filtered = string.Empty;
            var hasDecimal = false;
            
            foreach (var c in value ?? string.Empty)
            {
                if (char.IsDigit(c))
                {
                    filtered += c;
                }
                else if (c == '.' && !hasDecimal)
                {
                    filtered += c;
                    hasDecimal = true;
                }
            }
            
            SetProperty(ref _amountText, filtered);
            // Update validation when amount changes
            OnPropertyChanged(nameof(IsLoanAmountValid));
        }
    }

    public bool IsLoanAmountValid
    {
        get
        {
            return decimal.TryParse(AmountText, out decimal amount) && amount > 0;
        }
    }

    private string _interestRateText = "0";
    public string InterestRateText { get => _interestRateText; set => SetProperty(ref _interestRateText, value ?? "0"); }

    // Mode: Request or Send
    private string _mode = "Request";
    public string Mode { get => _mode; private set => SetProperty(ref _mode, value); }

    private bool _isRequestSelected = true;
    public bool IsRequestSelected { get => _isRequestSelected; set => SetProperty(ref _isRequestSelected, value); }

    private bool _isSendSelected = false;
    public bool IsSendSelected { get => _isSendSelected; set => SetProperty(ref _isSendSelected, value); }

    // Simple form fields
    private DateTime _paybackDate = DateTime.Today.AddDays(1);
    public DateTime PaybackDate { get => _paybackDate; set => SetProperty(ref _paybackDate, value); }

    public DateTime MinimumPaybackDate => DateTime.Today.AddDays(1);
    public DateTime MaximumPaybackDate => DateTime.Today.AddYears(10);

    // Payback type: By Date or By Time
    private bool _isByDateSelected = true;
    public bool IsByDateSelected { get => _isByDateSelected; set => SetProperty(ref _isByDateSelected, value); }

    private bool _isByTimeSelected = false;
    public bool IsByTimeSelected { get => _isByTimeSelected; set => SetProperty(ref _isByTimeSelected, value); }

    private bool _showDatePicker = true;
    public bool ShowDatePicker { get => _showDatePicker; set => SetProperty(ref _showDatePicker, value); }

    private bool _showTimeInput = false;
    public bool ShowTimeInput { get => _showTimeInput; set => SetProperty(ref _showTimeInput, value); }

    // Time duration
    private bool _isDaysSelected = true;
    public bool IsDaysSelected { get => _isDaysSelected; set => SetProperty(ref _isDaysSelected, value); }

    private bool _isMonthsSelected = false;
    public bool IsMonthsSelected { get => _isMonthsSelected; set => SetProperty(ref _isMonthsSelected, value); }

    private string _timeDuration = "";
    public string TimeDuration { get => _timeDuration; set => SetProperty(ref _timeDuration, value); }

    // Interest type selection
    public enum InterestTypeOption
    {
        None,
        Simple,
        Compound
    }

    private InterestTypeOption _interestType = InterestTypeOption.None;
    public InterestTypeOption InterestType { get => _interestType; set => SetProperty(ref _interestType, value); }

private bool _isNoInterestSelected = false;
    public bool IsNoInterestSelected { get => _isNoInterestSelected; set => SetProperty(ref _isNoInterestSelected, value); }

    private bool _isSimpleInterestSelected = false;
    public bool IsSimpleInterestSelected { get => _isSimpleInterestSelected; set => SetProperty(ref _isSimpleInterestSelected, value); }

    private bool _isCompoundInterestSelected = false;
    public bool IsCompoundInterestSelected { get => _isCompoundInterestSelected; set => SetProperty(ref _isCompoundInterestSelected, value); }

    // Interest calculation method: Total or APR
    private bool _isInterestMethodTotalSelected = true;
    public bool IsInterestMethodTotalSelected { get => _isInterestMethodTotalSelected; set => SetProperty(ref _isInterestMethodTotalSelected, value); }

    private bool _isInterestMethodAPRSelected = false;
    public bool IsInterestMethodAPRSelected { get => _isInterestMethodAPRSelected; set => SetProperty(ref _isInterestMethodAPRSelected, value); }

    // Interest details shown only when InterestType != None
    private decimal _interestRate = 0m;
    public decimal InterestRate { get => _interestRate; set => SetProperty(ref _interestRate, value); }

    // Payment frequency
    public IReadOnlyList<PaymentFrequencyOption> PaymentFrequencies => PaymentFrequencyOption.All;
    private PaymentFrequencyOption _selectedPaymentFrequency = PaymentFrequencyOption.Default;
    public PaymentFrequencyOption SelectedPaymentFrequency { get => _selectedPaymentFrequency; set => SetProperty(ref _selectedPaymentFrequency, value); }

    private string _selectedPaymentFrequencyLabel = PaymentFrequencyOption.Default.Label;
    public string SelectedPaymentFrequencyLabel { get => _selectedPaymentFrequencyLabel; set => SetProperty(ref _selectedPaymentFrequencyLabel, value); }

    // Payment frequency selection flags
    private bool _isOneTimeSelected = false;
    public bool IsOneTimeSelected { get => _isOneTimeSelected; set => SetProperty(ref _isOneTimeSelected, value); }
    
    private bool _isTwoPaymentsSelected = false;
    public bool IsTwoPaymentsSelected { get => _isTwoPaymentsSelected; set => SetProperty(ref _isTwoPaymentsSelected, value); }
    
    private bool _isMonthlySelected = false;
    public bool IsMonthlySelected { get => _isMonthlySelected; set => SetProperty(ref _isMonthlySelected, value); }
    
    private bool _isDailySelected = false;
    public bool IsDailySelected { get => _isDailySelected; set => SetProperty(ref _isDailySelected, value); }
    
    private bool _isWeeklySelected = false;
    public bool IsWeeklySelected { get => _isWeeklySelected; set => SetProperty(ref _isWeeklySelected, value); }
    
    private bool _isBiweekSelected = false;
    public bool IsBiweekSelected { get => _isBiweekSelected; set => SetProperty(ref _isBiweekSelected, value); }
    
    private bool _isHalfMonthSelected = false;
    public bool IsHalfMonthSelected { get => _isHalfMonthSelected; set => SetProperty(ref _isHalfMonthSelected, value); }

    // Helper method to check if a payment frequency is selected
    public bool IsPaymentFrequencySelected(string label) => label == SelectedPaymentFrequencyLabel;

    // Section visibility state
    private bool _isSimpleFormVisible = true;
    public bool IsSimpleFormVisible { get => _isSimpleFormVisible; set => SetProperty(ref _isSimpleFormVisible, value); }

    private bool _isInterestTypeVisible = true;
    public bool IsInterestTypeVisible { get => _isInterestTypeVisible; set => SetProperty(ref _isInterestTypeVisible, value); }

    private bool _isInterestDetailsVisible = false;
    public bool IsInterestDetailsVisible { get => _isInterestDetailsVisible; set => SetProperty(ref _isInterestDetailsVisible, value); }

    private bool _isPaymentFrequencyVisible = false;
    public bool IsPaymentFrequencyVisible { get => _isPaymentFrequencyVisible; set => SetProperty(ref _isPaymentFrequencyVisible, value); }

    private bool _isCollateralVisible = false;
    public bool IsCollateralVisible { get => _isCollateralVisible; set => SetProperty(ref _isCollateralVisible, value); }

    private bool _isYesCollateralSelected = false;
    public bool IsYesCollateralSelected { get => _isYesCollateralSelected; set => SetProperty(ref _isYesCollateralSelected, value); }

    private bool _isNoCollateralSelected = false;
    public bool IsNoCollateralSelected { get => _isNoCollateralSelected; set => SetProperty(ref _isNoCollateralSelected, value); }

    private bool _showNextButton = false;
    public bool ShowNextButton { get => _showNextButton; set => SetProperty(ref _showNextButton, value); }

    // Collateral details
    private string _collateralDescription = string.Empty;
    public string CollateralDescription { get => _collateralDescription; set => SetProperty(ref _collateralDescription, value); }

    private byte[]? _collateralFileBytes;
    public byte[]? CollateralFileBytes { get => _collateralFileBytes; set { if (SetProperty(ref _collateralFileBytes, value)) OnPropertyChanged(nameof(HasCollateralFile)); } }

    private string _collateralFileId = string.Empty;
    public string CollateralFileId { get => _collateralFileId; set { if (SetProperty(ref _collateralFileId, value)) OnPropertyChanged(nameof(HasCollateralFile)); } }

    private string _collateralFileType = string.Empty; // image or pdf
    public string CollateralFileType { get => _collateralFileType; set => SetProperty(ref _collateralFileType, value); }

    public bool HasCollateralFile => (CollateralFileBytes != null && CollateralFileBytes.Length > 0) || !string.IsNullOrWhiteSpace(CollateralFileId);

    // Backward compatibility properties (for Transaction/PDF that still use base64)
    public string CollateralFileBase64 => CollateralFileBytes != null ? Convert.ToBase64String(CollateralFileBytes) : string.Empty;
    public string CollateralImageBase64 => CollateralFileBase64;
    public string CollateralImageId { get => CollateralFileId; set => CollateralFileId = value; }
    public bool HasCollateralImage => HasCollateralFile;

    // Requester Information
    private string _requesterName = "";
    public string RequesterName { get => _requesterName; set => SetProperty(ref _requesterName, value); }

    private string _requesterPhone = "";
    public string RequesterPhone { get => _requesterPhone; set => SetProperty(ref _requesterPhone, value); }

    private string _requesterEmail = "";
    public string RequesterEmail { get => _requesterEmail; set => SetProperty(ref _requesterEmail, value); }

    private string _requesterAddress = "";
    public string RequesterAddress { get => _requesterAddress; set => SetProperty(ref _requesterAddress, value); }

    private string _requesterCity = "";
    public string RequesterCity { get => _requesterCity; set => SetProperty(ref _requesterCity, value); }

    private string _requesterState = "";
    public string RequesterState { get => _requesterState; set => SetProperty(ref _requesterState, value); }

    private string _requesterZipCode = "";
    public string RequesterZipCode { get => _requesterZipCode; set => SetProperty(ref _requesterZipCode, value); }

    private string _requesterIdNumber = "";
    public string RequesterIdNumber { get => _requesterIdNumber; set => SetProperty(ref _requesterIdNumber, value); }

    // Notification Type
    private bool _isNoNotificationSelected = true;
    public bool IsNoNotificationSelected { get => _isNoNotificationSelected; set => SetProperty(ref _isNoNotificationSelected, value); }

    private bool _isSMSNotificationSelected = false;
    public bool IsSMSNotificationSelected { get => _isSMSNotificationSelected; set => SetProperty(ref _isSMSNotificationSelected, value); }

    private bool _isEmailNotificationSelected = false;
    public bool IsEmailNotificationSelected { get => _isEmailNotificationSelected; set => SetProperty(ref _isEmailNotificationSelected, value); }

    // Computed outputs
    private decimal _periodicPayment = 0;
    public decimal PeriodicPayment { get => _periodicPayment; private set => SetProperty(ref _periodicPayment, value); }

    private decimal _totalInterest = 0;
    public decimal TotalInterest { get => _totalInterest; private set => SetProperty(ref _totalInterest, value); }

    private decimal _totalPayment = 0;
    public decimal TotalPayment { get => _totalPayment; private set => SetProperty(ref _totalPayment, value); }

    private int _totalPayments = 0;
    public int TotalPayments { get => _totalPayments; private set => SetProperty(ref _totalPayments, value); }

    public string InterestTypeDisplay => InterestType switch
    {
        InterestTypeOption.None => "No Interest",
        InterestTypeOption.Simple => "Simple Interest",
        InterestTypeOption.Compound => "Compound Interest",
        _ => "None"
    };

    public string PaybackDisplay
    {
        get
        {
            if (IsByDateSelected)
                return PaybackDate.ToString("MMM dd, yyyy");
            else if (IsByTimeSelected)
                return $"{TimeDuration} {(IsDaysSelected ? "Days" : "Months")}";
            return "Not set";
        }
    }

    public string TimeDurationLabel => IsDaysSelected ? "Days" : "Months";

    public string CollateralDisplay => IsYesCollateralSelected ? "Yes" : "No";

    public string InterestMethodDisplay => IsInterestMethodTotalSelected ? "Total %" : "APR (Annual)";

    public string NotificationTypeDisplay
    {
        get
        {
            if (IsSMSNotificationSelected) return "SMS";
            if (IsEmailNotificationSelected) return "Email";
            return "None";
        }
    }

    public bool HasAddress => !string.IsNullOrWhiteSpace(RequesterAddress) || !string.IsNullOrWhiteSpace(RequesterCity) ||
                              !string.IsNullOrWhiteSpace(RequesterState) || !string.IsNullOrWhiteSpace(RequesterZipCode);

    public string FullAddress
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(RequesterAddress)) parts.Add(RequesterAddress);
            if (!string.IsNullOrWhiteSpace(RequesterCity)) parts.Add(RequesterCity);
            if (!string.IsNullOrWhiteSpace(RequesterState)) parts.Add(RequesterState);
            if (!string.IsNullOrWhiteSpace(RequesterZipCode)) parts.Add(RequesterZipCode);
            return string.Join(", ", parts);
        }
    }

    public bool HasIdNumber => !string.IsNullOrWhiteSpace(RequesterIdNumber);

    public LoanFormViewModel()
    {
        Debug.WriteLine("LoanFormViewModel constructor started - LoanFormViewModel.cs:373");
        BackCommand = new Command(async () => 
        {
            Debug.WriteLine("BackCommand executing - LoanFormViewModel.cs:376");
            await Shell.Current.GoToAsync("..");
        });
        UploadCollateralImageCommand = new Command(async () =>
        {
            await PickCollateralImageAsync();
        });
        PickCollateralImageCommand = new Command(async () =>
        {
            await PickCollateralImageAsync();
        });
        CaptureCollateralImageCommand = new Command(async () =>
        {
            await CaptureCollateralImageAsync();
        });
        SubmitCommand = new Command(async () =>
        {
            // Navigate to requester info page
            var requesterInfoPage = new LoanRequesterInfoPage();
            requesterInfoPage.BindingContext = this;
            await Shell.Current.Navigation.PushAsync(requesterInfoPage);
        });
        
        ContinueToRequesterInfoCommand = new Command(async () =>
        {
            // Validate: if collateral is selected, description is required
            if (IsYesCollateralSelected && string.IsNullOrWhiteSpace(CollateralDescription))
            {
                var page = Application.Current?.Windows[0]?.Page;
                if (page != null)
                {
                    await page.DisplayAlertAsync(
                        "Missing Collateral Description",
                        "Please enter a description for the collateral.",
                        "OK");
                }
                return;
            }

            // Navigate to requester info page
            var requesterInfoPage = new LoanRequesterInfoPage();
            requesterInfoPage.BindingContext = this;
            await Shell.Current.Navigation.PushAsync(requesterInfoPage);
        });
        
        SetNotificationTypeCommand = new Command<string>((type) =>
        {
            IsNoNotificationSelected = type == "None";
            IsSMSNotificationSelected = type == "SMS";
            IsEmailNotificationSelected = type == "Email";
        });
        
        ContinueToFinalSummaryCommand = new Command(async () =>
        {
            // Navigate to final summary page
            var finalSummaryPage = new LoanFinalSummaryPage();
            finalSummaryPage.BindingContext = this;
            await Shell.Current.Navigation.PushAsync(finalSummaryPage);
        });
        
        SubmitLoanRequestCommand = new Command(async () =>
        {
            try
            {
            await EnsureCollateralImageSizeAsync();
                // Upload collateral file to Firebase Storage if present
                if (IsYesCollateralSelected && CollateralFileBytes != null && CollateralFileBytes.Length > 0)
                {
                    try
                    {
                        var fileExt = CollateralFileType.Equals("pdf", StringComparison.OrdinalIgnoreCase) ? "pdf" : "jpg";
                        var authService = ServiceHelper.GetService<IAuthenticationService>();
                        System.Diagnostics.Debug.WriteLine($"[Upload] Auth state - IsAuthenticated: {authService?.IsAuthenticated}, CurrentUserId: {authService?.CurrentUserId}, CurrentUserEmail: {authService?.CurrentUserEmail} - LoanFormViewModel.cs:451");
                        
                        // Use email for the storage path instead of UID
                        var userEmail = authService?.CurrentUserEmail;
                        if (string.IsNullOrWhiteSpace(userEmail))
                        {
                            var page = Application.Current?.Windows[0]?.Page;
                            if (page != null)
                            {
                                await page.DisplayAlertAsync(
                                    "Error",
                                    "Unable to identify logged-in user. Please log in again.",
                                    "OK");
                            }
                            return;
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"[Upload] User email: {userEmail} - LoanFormViewModel.cs:465");
                        
                        // Generate unique image ID
                        var imageId = $"{Guid.NewGuid():N}";
                        var imageIdWithExt = $"{imageId}.{fileExt}";
                        
                        // Build full storage path: users/{email}/collateral/{imageId.ext}
                        var fullPath = $"users/{userEmail}/collateral/{imageIdWithExt}";
                        System.Diagnostics.Debug.WriteLine($"[Upload] Full file path: {fullPath} - LoanFormViewModel.cs:473");
                        var storage = new FirebaseStorageService();
                        var contentType = CollateralFileType.Equals("pdf", StringComparison.OrdinalIgnoreCase) ? "application/pdf" : "image/jpeg";
                        System.Diagnostics.Debug.WriteLine($"[Upload] File size: {CollateralFileBytes.Length} bytes, Content-Type: {contentType} - LoanFormViewModel.cs:476");
                        var uploadedName = await storage.UploadImageAsync(CollateralFileBytes, fullPath, contentType);
                        if (!string.IsNullOrWhiteSpace(uploadedName))
                        {
                            // Store ONLY the image ID (filename), not the full path
                            CollateralFileId = imageIdWithExt;
                            System.Diagnostics.Debug.WriteLine($"[Upload] Stored image ID in Firestore: {imageIdWithExt} - LoanFormViewModel.cs:481");
                            // Clear bytes to avoid oversized memory usage
                            CollateralFileBytes = null;
                        }
                        else
                        {
                            // Upload failed; prevent submission
                            var page = Application.Current?.Windows[0]?.Page;
                            if (page != null)
                            {
                                await page.DisplayAlertAsync(
                                    "Upload Failed",
                                    "Collateral file upload to storage failed. Please try again.",
                                    "OK");
                            }
                            return;
                        }
                    }
                    catch (Exception upEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Collateral upload failed: {upEx.Message} - LoanFormViewModel.cs:471");
                        var page = Application.Current?.Windows[0]?.Page;
                        if (page != null)
                        {
                            await page.DisplayAlertAsync(
                                "Upload Error",
                                $"Failed to upload collateral file: {upEx.Message}",
                                "OK");
                        }
                        return;
                    }
                }

                var transactionDoc = await BuildTransactionDocumentAsync();
                var docId = await FirestoreService.Instance.CreateTransactionDocumentAsync(transactionDoc);

                if (!string.IsNullOrEmpty(docId))
                {
                    // Also save transaction to user's sub-collection
                    var authService = ServiceHelper.GetService<IAuthenticationService>();
                    if (authService != null && !string.IsNullOrWhiteSpace(authService.CurrentUserEmail))
                    {
                        // Save to user's transactions sub-collection
                        await FirestoreService.Instance.CreateUserTransactionDocumentAsync(authService.CurrentUserEmail, transactionDoc);

                        // Update user profile with new loan statistics
                        var currentUser = await FirestoreService.Instance.GetUserAsync(authService.CurrentUserEmail);
                        if (currentUser != null)
                        {
                            // Parse amount
                            if (decimal.TryParse(AmountText, out var amount) && amount > 0)
                            {
                                // Update statistics based on mode
                                if (Mode == "Send")
                                {
                                    // User is lending (sending money)
                                    currentUser.LoansGiven++;
                                    currentUser.TotalLent += amount;
                                    // Deduct from balance (lent out)
                                    currentUser.Balance -= amount;
                                }
                                else // Request mode
                                {
                                    // User is borrowing (requesting money)
                                    currentUser.LoansReceived++;
                                    currentUser.TotalBorrowed += amount;
                                    // Add to balance (will receive)
                                    currentUser.Balance += amount;
                                }

                                // Save updated user back to Firestore
                                await FirestoreService.Instance.UpdateUserAsync(currentUser);
                            }
                        }
                    }

                    var page = Application.Current?.Windows[0]?.Page;
                    if (page != null)
                    {
                        await page.DisplayAlertAsync("Success", "Loan request submitted successfully!", "OK");
                    }
                }
                else
                {
                    var page = Application.Current?.Windows[0]?.Page;
                    if (page != null)
                    {
                        await page.DisplayAlertAsync("Error", "Failed to submit loan request.", "OK");
                    }
                }

                await Shell.Current.GoToAsync("//mainpage");
            }
            catch (Exception ex)
            {
                var page = Application.Current?.Windows[0]?.Page;
                if (page != null)
                {
                    await page.DisplayAlertAsync("Error", $"Failed to submit loan request: {ex.Message}", "OK");
                }
            }
        });
        
        GeneratePdfCommand = new Command(async () =>
        {
            try
            {
                await EnsureCollateralImageSizeAsync();
                // In debug, attach a sample collateral file if missing
                if (IsDebugMode && IsYesCollateralSelected && (CollateralFileBytes == null || CollateralFileBytes.Length == 0) && string.IsNullOrWhiteSpace(CollateralFileId))
                {
                    CollateralDescription = string.IsNullOrWhiteSpace(CollateralDescription) ? "Demo collateral file" : CollateralDescription;
                    // 1x1 white PNG
                    CollateralFileBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=");
                    CollateralFileType = "image";
                }

                var transactionDoc = await BuildTransactionDocumentAsync();

                var pdfService = new LoanPdfService();
                string filePath = await pdfService.GenerateLoanPdf(transactionDoc);
                
                // Use Share to preview and allow user to save/share the PDF
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Loan Agreement Receipt",
                    File = new ShareFile(filePath)
                });
            }
            catch (Exception ex)
            {
                var page = Application.Current?.Windows[0]?.Page;
                if (page != null)
                {
                    await page.DisplayAlertAsync("Error", $"Failed to generate PDF: {ex.Message}", "OK");
                }
            }
        });
        
        SetRequestModeCommand = new Command(() => {
            Mode = "Request";
            IsRequestSelected = true;
            IsSendSelected = false;
            IsSimpleFormVisible = true;
            IsInterestTypeVisible = true;
            IsInterestDetailsVisible = InterestType != InterestTypeOption.None;
            IsPaymentFrequencyVisible = InterestType != InterestTypeOption.None;
        });
        SetSendModeCommand = new Command(() => {
            Mode = "Send";
            IsRequestSelected = false;
            IsSendSelected = true;
            IsSimpleFormVisible = true;
            IsInterestTypeVisible = true;
            IsInterestDetailsVisible = InterestType != InterestTypeOption.None;
            IsPaymentFrequencyVisible = InterestType != InterestTypeOption.None;
        });

        SetInterestTypeCommand = new Command<object?>(param =>
        {
            var val = (param as string) ?? string.Empty;
            InterestType = val switch
            {
                "None" => InterestTypeOption.None,
                "Simple" => InterestTypeOption.Simple,
                "Compound" => InterestTypeOption.Compound,
                _ => InterestTypeOption.None
            };

            // Update selection flags
            IsNoInterestSelected = InterestType == InterestTypeOption.None;
            IsSimpleInterestSelected = InterestType == InterestTypeOption.Simple;
            IsCompoundInterestSelected = InterestType == InterestTypeOption.Compound;

            // Toggle interest details visibility based on type
            IsInterestDetailsVisible = InterestType != InterestTypeOption.None;
            IsPaymentFrequencyVisible = true;
        });

        SelectPaymentFrequencyCommand = new Command<object?>(param =>
        {
            if (param is PaymentFrequencyOption p)
            {
                SelectedPaymentFrequency = p;
                SelectedPaymentFrequencyLabel = p.Label;
                
                // Update selection flags
                IsOneTimeSelected = p.Label == "One Time";
                IsTwoPaymentsSelected = p.Label == "Two Payments";
                IsDailySelected = p.Label == "Daily";
                IsWeeklySelected = p.Label == "Weekly";
                IsBiweekSelected = p.Label == "Bi-week";
                IsHalfMonthSelected = p.Label == "Half Month";
                IsMonthlySelected = p.Label == "Monthly";
                
                // Show collateral options after frequency selection
                IsCollateralVisible = true;
            }
        });

        SetByDateCommand = new Command(() =>
        {
            IsByDateSelected = true;
            IsByTimeSelected = false;
            ShowDatePicker = true;
            ShowTimeInput = false;
        });

        SetByTimeCommand = new Command(() =>
        {
            IsByDateSelected = false;
            IsByTimeSelected = true;
            ShowDatePicker = false;
            ShowTimeInput = true;
        });

        SetDaysCommand = new Command(() =>
        {
            IsDaysSelected = true;
            IsMonthsSelected = false;
        });

        SetMonthsCommand = new Command(() =>
        {
            IsDaysSelected = false;
            IsMonthsSelected = true;
        });

        SetYesCollateralCommand = new Command(() =>
        {
            IsYesCollateralSelected = true;
            IsNoCollateralSelected = false;
            ShowNextButton = true;
        });

        SetNoCollateralCommand = new Command(() =>
        {
            IsYesCollateralSelected = false;
            IsNoCollateralSelected = true;
            ShowNextButton = true;
            CollateralDescription = string.Empty;
            CollateralFileBytes = null;
            CollateralFileId = string.Empty;
            CollateralFileType = string.Empty;
        });

        AutoFillRequesterDemoCommand = new Command(() =>
        {
            RequesterName = "Test1";
            RequesterPhone = "Test2";
            RequesterEmail = "Test3";
            RequesterAddress = "Test4";
            RequesterCity = "Test5";
            RequesterState = "Test6";
            RequesterZipCode = "Test7";
            RequesterIdNumber = "Test8";

            // Set notification to Email for demo
            IsNoNotificationSelected = false;
            IsSMSNotificationSelected = false;
            IsEmailNotificationSelected = true;

            // Notify derived properties changed
            OnPropertyChanged(nameof(HasAddress));
            OnPropertyChanged(nameof(FullAddress));
            OnPropertyChanged(nameof(HasIdNumber));
            OnPropertyChanged(nameof(NotificationTypeDisplay));
        });

        SetInterestMethodTotalCommand = new Command(() =>
        {
            IsInterestMethodTotalSelected = true;
            IsInterestMethodAPRSelected = false;
        });

        SetInterestMethodAPRCommand = new Command(() =>
        {
            IsInterestMethodTotalSelected = false;
            IsInterestMethodAPRSelected = true;
        });

        NextCommand = new Command(async () =>
        {
            // Calculate loan details before navigating
            CalculateLoanDetails();
            
            // Navigate to summary page - the page will use the same ViewModel instance
            var summaryPage = new LoanSummaryPage();
            summaryPage.BindingContext = this;
            await Shell.Current.Navigation.PushAsync(summaryPage);
        });
        
        // Initialize selection flags
        IsRequestSelected = true;
        IsInterestMethodTotalSelected = true;
        
        Debug.WriteLine("LoanFormViewModel constructor completed - LoanFormViewModel.cs:742");
    }

    private void CalculateLoanDetails()
    {
        // Parse amount
        if (!decimal.TryParse(AmountText, out decimal principal) || principal <= 0)
        {
            principal = 0;
            TotalPayments = 0;
            PeriodicPayment = 0;
            TotalInterest = 0;
            TotalPayment = 0;
            return;
        }

        // Determine number of periods
        int periods = 0;
        
        // Special handling for One Time and Two Payments
        if (SelectedPaymentFrequency.Label == "One Time")
        {
            periods = 1;
        }
        else if (SelectedPaymentFrequency.Label == "Two Payments")
        {
            periods = 2;
        }
        else if (IsByDateSelected)
        {
            var timeSpan = PaybackDate - DateTime.Today;
            periods = Math.Max(1, (int)(timeSpan.TotalDays / (365.0 / SelectedPaymentFrequency.PaymentsPerYear)));
        }
        else if (IsByTimeSelected && decimal.TryParse(TimeDuration, out decimal duration))
        {
            if (IsDaysSelected)
            {
                periods = Math.Max(1, (int)((double)duration / (365.0 / SelectedPaymentFrequency.PaymentsPerYear)));
            }
            else // Months
            {
                periods = Math.Max(1, (int)((double)duration * SelectedPaymentFrequency.PaymentsPerYear / 12.0));
            }
        }

        TotalPayments = Math.Max(1, periods);

        // Calculate based on interest type
        if (InterestType == InterestTypeOption.None)
        {
            TotalInterest = 0;
            TotalPayment = principal;
            PeriodicPayment = principal / TotalPayments;
        }
        else
        {
            decimal rate = InterestRate / 100m;
            
            if (IsInterestMethodTotalSelected)
            {
                // Total interest method: rate applies to entire loan period
                if (InterestType == InterestTypeOption.Simple)
                {
                    // Simple interest: Total interest = Principal * Rate
                    TotalInterest = principal * rate;
                    TotalPayment = principal + TotalInterest;
                    PeriodicPayment = TotalPayment / TotalPayments;
                }
                else // Compound
                {
                    // Compound interest: Final Amount = Principal * (1 + rate)
                    TotalPayment = principal * (decimal)Math.Pow((double)(1 + rate), 1);
                    TotalInterest = TotalPayment - principal;
                    PeriodicPayment = TotalPayment / TotalPayments;
                }
            }
            else // APR method
            {
                // APR method: rate is annual, calculate based on loan duration
                if (InterestType == InterestTypeOption.Simple)
                {
                    // Simple interest: I = P * r * t (where t is in years)
                    decimal years = TotalPayments / (decimal)SelectedPaymentFrequency.PaymentsPerYear;
                    TotalInterest = principal * rate * years;
                    TotalPayment = principal + TotalInterest;
                    PeriodicPayment = TotalPayment / TotalPayments;
                }
                else // Compound
                {
                    // Compound interest with periodic payments (amortization formula)
                    decimal periodicRate = rate / SelectedPaymentFrequency.PaymentsPerYear;
                    if (periodicRate > 0)
                    {
                        PeriodicPayment = principal * (periodicRate * (decimal)Math.Pow((double)(1 + periodicRate), TotalPayments)) /
                                          ((decimal)Math.Pow((double)(1 + periodicRate), TotalPayments) - 1);
                        TotalPayment = PeriodicPayment * TotalPayments;
                        TotalInterest = TotalPayment - principal;
                    }
                    else
                    {
                        PeriodicPayment = principal / TotalPayments;
                        TotalPayment = principal;
                        TotalInterest = 0;
                    }
                }
            }
        }
    }

    private async Task PickCollateralImageAsync()
    {
        try
        {
            // File picker for images and PDFs (documents)
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select collateral (image or PDF)",
                FileTypes = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.image", "com.adobe.pdf" } },
                        { DevicePlatform.Android, new[] { "image/*", "application/pdf" } }
                    })
            });

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var fileType = result.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? "pdf" : "image";
                TrySetCollateralFileBase64FromBytes(ms.ToArray(), fileType);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PickCollateralImageAsync error: {ex.Message} - LoanFormViewModel.cs:878");
        }
    }

    private async Task CaptureCollateralImageAsync()
    {
        try
        {
            // Photo library picker (gallery) for both iOS and Android
#pragma warning disable CS0618 // Type or member is obsolete
            var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select photo from gallery"
            });
#pragma warning restore CS0618

            if (photo != null)
            {
                using var stream = await photo.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                TrySetCollateralFileBase64FromBytes(ms.ToArray(), "image");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CaptureCollateralImageAsync error: {ex.Message} - LoanFormViewModel.cs:902");
        }
    }

    private void TrySetCollateralFileBase64FromBytes(byte[] bytes, string fileType)
    {
        try
        {
            if (bytes.Length == 0)
                return;

            CollateralFileType = fileType;

            // For images, attempt iOS-specific JPEG compression
            if (fileType.Equals("image", StringComparison.OrdinalIgnoreCase))
            {
#if IOS
                try
                {
                    using var data = NSData.FromArray(bytes);
                    using var uiImage = UIImage.LoadFromData(data);
                    if (uiImage != null)
                    {
                        using var jpgData = uiImage.AsJPEG(0.7f); // 70% quality
                        if (jpgData != null)
                        {
                            CollateralFileBytes = jpgData.ToArray();
                            return;
                        }
                    }
                }
                catch (Exception iosEx)
                {
                    Debug.WriteLine($"iOS compression failed, fallback to raw: {iosEx.Message} - LoanFormViewModel.cs:932");
                }
#endif
            }

            CollateralFileBytes = bytes;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TrySetCollateralFileBase64FromBytes error: {ex.Message} - LoanFormViewModel.cs:941");
        }
    }

    private async Task EnsureCollateralImageSizeAsync()
    {
        // No longer needed since we're uploading bytes directly to storage
        // instead of storing in Firestore as base64
        await Task.CompletedTask;
    }

    private string? TryGenerateThumbnailBase64(string base64, int maxWidth)
    {
        try
        {
#if IOS
            // Decode base64 into UIImage
            var bytes = Convert.FromBase64String(base64);
            using var data = NSData.FromArray(bytes);
            using var uiImage = UIImage.LoadFromData(data);
            if (uiImage == null || uiImage.Size.Width <= 0 || uiImage.Size.Height <= 0)
            {
                return null;
            }

            // Compute target size maintaining aspect ratio
            var aspect = uiImage.Size.Height / uiImage.Size.Width;
            nfloat targetWidth = maxWidth;
            nfloat targetHeight = (nfloat)(maxWidth * aspect);

            // Use modern UIGraphicsImageRenderer for iOS 17+ compatibility
            var renderer = new UIGraphicsImageRenderer(new CoreGraphics.CGSize(targetWidth, targetHeight));
            var resized = renderer.CreateImage((context) =>
            {
                uiImage.Draw(new CoreGraphics.CGRect(0, 0, targetWidth, targetHeight));
            });

            if (resized == null)
            {
                return null;
            }

            using var jpgData = resized.AsJPEG(0.6f);
            if (jpgData == null)
            {
                return null;
            }

            return Convert.ToBase64String(jpgData.ToArray());
#else
            // Non-iOS: no thumbnail support implemented
            return null;
#endif
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TryGenerateThumbnailBase64 error: {ex.Message} - LoanFormViewModel.cs:996");
            return null;
        }
    }
}