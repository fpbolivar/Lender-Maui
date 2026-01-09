# Lender App — Overview and Scope

## Purpose
Track peer-to-peer loans you lend or borrow, focusing on terms, payments, confirmations, and totals. Budgets are out-of-scope for the UX (even if collections exist for now).

## Platform & Stack
- .NET 10 MAUI (iOS first)
- Firebase Auth (email/password) via REST
- Firestore via REST, email used as user document ID
- Shell navigation with login and dashboard flows

## Current Experience
- Authentication: email/password sign-up, sign-in, session restore, sign-out.
- Profile capture: first name, last name, phone (optional), date of birth with native iOS picker, age validation (13-120).
- Data collections scaffolded: users, loans, transactions, investments (budgets collection present but not a UX priority).
- Dashboard: designed to surface totals and recent activity; loan/payment flows are the focus for next iterations.

## Loan Model (required fields)
- Parties: lender name/email, borrower name/email.
- Financials: principal, rate, interest type (simple/compound), term length, payment frequency/amount, remaining balance, expected return.
- Status: pending, active, delinquent, completed, cancelled.
- Collateral: description, estimated value, custody/escrow flag.
- Schedule/Audit: start/end dates, next payment date, last payment date, created/updated timestamps, confirmations.
- Notes: freeform notes per loan.

## Transactions & Payments
- Each payment stores amount, date, method, status (pending/confirmed/failed), who confirmed, note.
- Confirmation updates remaining balance and next payment.
- Allow manual confirm/deny of payments.

## Dashboard & Profile
- Dashboard: totals (lent, borrowed, expected return, next payment), active loans list, recent transactions.
- Profile: full name, email, phone, DOB, credit score, balances, account settings (edit profile, change password, notifications), destructive actions (sign out, delete account with confirmation).

## Output & Demo
- PDF export goal: parties, terms, schedule, payments-to-date, remaining balance, collateral, notes.
- Demo mode allowed; live data when authenticated.

## Priorities
- Keep UI centered on loans, payments, confirmations, and totals; avoid budget features in UX.
- Maintain clear collections separation (users, loans, transactions, investments).
- Preserve email-as-document-ID for Firestore users.
