using iText.Kernel.Pdf;
using iText.IO.Image;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Globalization;
using Lender.Documents;

namespace Lender.Services;

public class LoanPdfService
{
    public async Task<string> GenerateLoanPdf(Transaction transaction)
    {
        try
        {
            // Create file path in app cache directory
            string documentsPath = FileSystem.CacheDirectory;
            string filePath = Path.Combine(documentsPath, $"Loan_Receipt_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            using (PdfWriter writer = new PdfWriter(filePath))
            using (PdfDocument pdf = new PdfDocument(writer))
            using (Document document = new Document(pdf))
            {
                document.SetMargins(20, 20, 20, 20);

                // Header - Company Info
                Paragraph header = new Paragraph("LOAN AGREEMENT RECEIPT")
                    .SetFontSize(20)
                    .SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(header);

                Paragraph date = new Paragraph($"Date: {DateTime.Now:MMM dd, yyyy}")
                    .SetFontSize(11)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginBottom(20);
                document.Add(date);

                // Section 1: Loan Details
                document.Add(CreateSectionHeader("LOAN DETAILS"));
                Table loanDetailsTable = new Table(2)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(20);

                AddTableRow(loanDetailsTable, "Loan Type:", transaction.Mode);
                AddTableRow(loanDetailsTable, "Amount:", $"${transaction.Amount:F2}");
                {
                    var startDateForDisplay = DateTime.Today;
                    string paybackDateText = transaction.PaybackDisplay;
                    if (transaction.PaybackType == "By Date" && transaction.PaybackDate.HasValue)
                    {
                        paybackDateText = transaction.PaybackDate.Value.ToString("MMM dd, yyyy");
                    }
                    else if (transaction.PaybackType == "By Time" && !string.IsNullOrEmpty(transaction.PaybackDuration) && decimal.TryParse(transaction.PaybackDuration, out decimal dur))
                    {
                        DateTime endDateForDisplay = transaction.IsDaysDuration
                            ? startDateForDisplay.AddDays((int)dur)
                            : startDateForDisplay.AddMonths((int)dur);
                        paybackDateText = endDateForDisplay.ToString("MMM dd, yyyy");
                    }
                    AddTableRow(loanDetailsTable, "Payback Date:", paybackDateText);
                }
                AddTableRow(loanDetailsTable, "Interest Type:", transaction.InterestTypeDisplay);
                if (transaction.InterestRate > 0)
                {
                    AddTableRow(loanDetailsTable, "Interest Rate:", $"{transaction.InterestRate}%");
                    AddTableRow(loanDetailsTable, "Interest Method:", transaction.InterestMethodDisplay);
                }
                AddTableRow(loanDetailsTable, "Payment Frequency:", transaction.PaymentFrequencyLabel);
                AddTableRow(loanDetailsTable, "Collateral:", transaction.CollateralDisplay);

                document.Add(loanDetailsTable);

                // Section 2: Payment Plan Summary
                document.Add(CreateSectionHeader("PAYMENT PLAN SUMMARY"));
                Table paymentSummaryTable = new Table(2)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(20);

                    AddTableRow(paymentSummaryTable, "Total Payments:", transaction.TotalPayments.ToString());
                    AddTableRow(paymentSummaryTable, "Payment Amount:", $"${transaction.PeriodicPayment:F2}");
                    AddTableRow(paymentSummaryTable, "Interest:", $"${transaction.TotalInterest:F2}");
                    AddTableRow(paymentSummaryTable, "Total to Pay:", $"${transaction.TotalPayment:F2}");

                document.Add(paymentSummaryTable);

                // Section 3: Payment Schedule
                document.Add(CreateSectionHeader("PAYMENT SCHEDULE"));
                Table scheduleTable = new Table(3)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(20);

                // Header row
                AddHeaderCell(scheduleTable, "Payment #");
                AddHeaderCell(scheduleTable, "Due Date");
                AddHeaderCell(scheduleTable, "Amount");

                // Payment rows
                DateTime startDate = DateTime.Today;
                DateTime endDate = transaction.PaybackType == "By Date" && transaction.PaybackDate.HasValue
                    ? transaction.PaybackDate.Value
                    : DateTime.Today;

                // If By Time, compute end date from duration
                int durationDays = 0;
                int durationMonths = 0;
                if (transaction.PaybackType == "By Time" && !string.IsNullOrEmpty(transaction.PaybackDuration) && decimal.TryParse(transaction.PaybackDuration, out decimal duration))
                {
                    if (transaction.IsDaysDuration)
                    {
                        durationDays = (int)duration;
                        endDate = startDate.AddDays(durationDays);
                    }
                    else
                    {
                        durationMonths = (int)duration;
                        endDate = startDate.AddMonths(durationMonths);
                    }
                }

                decimal amountPerPayment = transaction.TotalPayment / transaction.TotalPayments;

                for (int i = 1; i <= transaction.TotalPayments; i++)
                {
                    DateTime currentDate;
                    if (transaction.PaybackType == "By Time")
                    {
                        if (transaction.IsDaysDuration)
                        {
                            int daysIncrement = (int)Math.Floor(durationDays * (double)i / transaction.TotalPayments);
                            currentDate = startDate.AddDays(daysIncrement);
                        }
                        else
                        {
                            int monthsIncrement = (int)Math.Floor(durationMonths * (double)i / transaction.TotalPayments);
                            currentDate = startDate.AddMonths(monthsIncrement);
                        }
                    }
                    else // By Date
                    {
                        int totalDaysBetween = Math.Max(0, (endDate - startDate).Days);
                        int daysIncrement = (int)Math.Floor(totalDaysBetween * (double)i / transaction.TotalPayments);
                        currentDate = startDate.AddDays(daysIncrement);
                    }

                    scheduleTable.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(i.ToString())));
                    scheduleTable.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(currentDate.ToString("MMM dd, yyyy"))));
                    scheduleTable.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph($"${amountPerPayment:F2}")));
                }

                document.Add(scheduleTable);

                // Section 4: Petitioner (logged-in user) Information
                document.Add(CreateSectionHeader("PETITIONER INFORMATION"));
                Table petitionerTable = new Table(2)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(20);

                AddTableRow(petitionerTable, "Name:", transaction.PetitionerName);
                AddTableRow(petitionerTable, "Phone:", transaction.PetitionerPhone);
                AddTableRow(petitionerTable, "Email:", transaction.PetitionerEmail);

                document.Add(petitionerTable);

                // Section 5: Requester Information
                document.Add(CreateSectionHeader("REQUESTER INFORMATION"));
                Table requesterTable = new Table(2)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(20);

                AddTableRow(requesterTable, "Name:", transaction.RequesterName);
                AddTableRow(requesterTable, "Phone:", transaction.RequesterPhone);
                AddTableRow(requesterTable, "Email:", transaction.RequesterEmail);
                if (!string.IsNullOrWhiteSpace(transaction.RequesterAddress))
                {
                    AddTableRow(requesterTable, "Address:", transaction.RequesterAddress);
                }
                if (!string.IsNullOrWhiteSpace(transaction.RequesterIdNumber))
                {
                    AddTableRow(requesterTable, "ID Number:", transaction.RequesterIdNumber);
                }

                document.Add(requesterTable);

                // Section 6: Notification Settings
                document.Add(CreateSectionHeader("NOTIFICATION SETTINGS"));
                Table notificationTable = new Table(2)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(20);

                AddTableRow(notificationTable, "Notification Type:", transaction.NotificationType);
                if (!string.IsNullOrWhiteSpace(transaction.NotificationTarget))
                {
                    AddTableRow(notificationTable, "Contact:", transaction.NotificationTarget);
                }

                document.Add(notificationTable);

                // Section 7: Collateral Details (if any)
                if (transaction.HasCollateral)
                {
                    document.Add(CreateSectionHeader("COLLATERAL DETAILS"));
                    Table collateralTable = new Table(2)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginBottom(20);

                    AddTableRow(collateralTable, "Has Collateral:", transaction.CollateralDisplay);
                    if (!string.IsNullOrWhiteSpace(transaction.CollateralDescription))
                    {
                        AddTableRow(collateralTable, "Description:", transaction.CollateralDescription);
                    }

                    document.Add(collateralTable);

                    // Add image/file if provided (for images only, PDFs go to separate note)
                    try
                    {
                        byte[]? fileBytes = null;
                        var fileType = "image"; // default
                        
                        if (!string.IsNullOrWhiteSpace(transaction.CollateralImageBase64))
                        {
                            fileBytes = Convert.FromBase64String(transaction.CollateralImageBase64);
                        }
                        else if (!string.IsNullOrWhiteSpace(transaction.CollateralImageId))
                        {
                            var storage = new FirebaseStorageService();
                            var (bytes, error) = await storage.GetImageBytesAsync(transaction.CollateralImageId);
                            fileBytes = bytes;
                            // Determine file type from the ID or metadata
                            fileType = transaction.CollateralImageId?.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ?? false ? "pdf" : "image";
                        }

                        // Only embed images in PDF; PDFs are noted separately
                        if (fileBytes != null && fileBytes.Length > 0 && fileType.Equals("image", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                var imageData = ImageDataFactory.Create(fileBytes);
                                var image = new iText.Layout.Element.Image(imageData)
                                    .SetAutoScale(true)
                                    .SetMarginBottom(20);
                                document.Add(image);
                            }
                            catch (Exception imgEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"PDF Collateral image embedding error: {imgEx.Message}");
                            }
                        }
                        else if (fileType.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            var note = new Paragraph("[PDF collateral attached: See transaction record for full document]")
                                .SetFontSize(11)
                                .SetItalic()
                                .SetMarginBottom(20);
                            document.Add(note);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"PDF Collateral file error: {ex.Message}");
                    }
                }

                // Footer
                Paragraph footer = new Paragraph("Thank you for using Lender App")
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginTop(30);
                document.Add(footer);
            }

            return filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PDF Generation Error: {ex.Message} - LoanPdfService.cs:210");
            throw;
        }
    }

    private Paragraph CreateSectionHeader(string text)
    {
        return new Paragraph(text)
            .SetFontSize(14)
            .SetBold()
            .SetMarginTop(15)
            .SetMarginBottom(10)
            .SetBorderBottom(new iText.Layout.Borders.SolidBorder(1f));
    }

    private void AddTableRow(Table table, string label, string value)
    {
        table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(label).SetBold()));
        table.AddCell(new iText.Layout.Element.Cell().Add(new Paragraph(value)));
    }

    private void AddHeaderCell(Table table, string text)
    {
        iText.Layout.Element.Cell cell = new iText.Layout.Element.Cell()
            .Add(new Paragraph(text).SetBold())
            .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY);
        table.AddCell(cell);
    }
}
