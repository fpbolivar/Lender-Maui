# Lender App - Master Notes

Purpose: Track loans you lend or borrow, including interest, payments, and confirmations. No budgets.

Core Concepts
- Two roles per loan: lender (you give money) and borrower (you receive money).
- Track: principal, interest type (simple/compound), rate, term, payment frequency, collateral, due dates, and status.
- Payments: record each installment; confirm/deny receipt; update remaining balance and next payment.
- Totals: show total lent, total borrowed, expected returns, next payment due.
- Transactions: log every payment, disbursement, and fee for audit.
- Documents: generate a PDF summary per loan (terms, schedule, payments, collateral, signatures/notes).

Data Must-Haves per Loan
- Parties: lender name/email, borrower name/email.
- Financials: principal, interest rate, interest type (simple/compound), term length, start date, end date, payment frequency, payment amount, remaining balance, expected total return.
- Status: pending, active, delinquent, completed, cancelled.
- Collateral: description, estimated value, custody/escrow flag.
- Schedule: next payment date, schedule table (due dates and amounts), last payment date.
- Audit: created/updated timestamps, who created, confirmations.
- Notes: freeform notes per loan.

Transactions/Payments
- Each payment: amount, date, method, status (pending, confirmed, failed), who confirmed, note.
- Adjust remaining balance and next payment after confirmation.
- Allow manual confirm/deny on a payment.

Dashboards
- Show totals: lent, borrowed, expected return, next payment.
- Active loans list with status badges, amounts, next payment, quick confirm/deny actions.
- Transactions list for recent payments/disbursements.

Profile Page
- Display user information: full name, email, phone number, date of birth.
- Show credit score and balance prominently.
- Account settings: edit profile info, change password, notification preferences.
- Statistics: total loans created, total amount lent, total amount borrowed, on-time payment rate.
- Account actions: sign out, delete account (with confirmation).
- Version info and support links.
- Professional, clean design matching app's dark theme (#0A1929 bg, #FF9F43 accent).

PDF Export
- For any loan, generate a PDF with: parties, amounts, interest terms, schedule, payments to date, remaining balance, collateral, notes.

Demo Mode
- Demo data is allowed; live data loads when authenticated.

Priorities for Future Requests
- Never reintroduce budgets.
- Keep UI focused on loans, payments, confirmations, and totals.
- Ensure Firestore uses email as doc ID for users; keep loan/transaction collections clean and separated.
- Favor clear, minimal UI with key actions: add loan, record payment, confirm/deny payment, export PDF.
