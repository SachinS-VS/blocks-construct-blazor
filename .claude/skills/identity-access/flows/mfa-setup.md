# Flow: mfa-setup

## Trigger

User wants to set up MFA for a user account, or build an MFA enrollment page.

> "add MFA setup"
> "implement two-factor authentication"
> "build authenticator app setup"
> "enable TOTP for users"

---

## Pre-flight Questions

Before starting, confirm:

1. Which MFA type to set up? `email OTP`, `authenticator app (TOTP)`, or both?
2. Is this triggered by the user voluntarily, or forced on first login?
3. After setup, should the user immediately verify with an OTP to confirm enrollment?

---

## MFA Types

| Type | How it works | Actions used |
|------|-------------|--------------|
| Email OTP | Code sent to user's email | `generate-otp` → `verify-otp` |
| TOTP (Authenticator app) | QR code scanned in Google/Microsoft Authenticator | `setup-totp` → user scans QR → `verify-otp` to confirm |

---

## Flow Steps

### Path A — Email OTP Setup

#### Step 1 — Generate OTP

```
Action: generate-otp
Input:
  userId     = target user's ID
  projectKey = VITE_X_BLOCKS_KEY
  mfaType    = "OTP"
```

```
On success → OTP sent to user's registered email
           → Show OTP input form
On 400     → MFA not enabled for this user — enable it first via update-user
```

---

#### Step 2 — Verify OTP

Collect the 5-digit code from the user.

```
Action: verify-otp
Input:
  userId     = target user's ID
  otp        = code entered by user
  projectKey = VITE_X_BLOCKS_KEY
```

```
On success → MFA confirmed, show success state
On 400     → Invalid or expired OTP, allow retry
           → Offer resend via resend-otp
```

---

#### Step 3 — Resend OTP (if needed)

```
Action: resend-otp
Input:
  userId     = target user's ID
  projectKey = VITE_X_BLOCKS_KEY
```

```
On 429 → Rate limited, show "Please wait before requesting another code"
```

---

### Path B — TOTP (Authenticator App) Setup

#### Step 1 — Get TOTP Setup Data

```
Action: setup-totp
Input:  userId (query param)
Output: QR code URI + secret key
```

```
On success → display QR code for user to scan in their authenticator app
           → display secret key as text fallback
```

---

#### Step 2 — Confirm Enrollment

After scanning, user enters the 6-digit code shown in their authenticator app.

```
Action: verify-otp
Input:
  userId     = target user's ID
  otp        = 6-digit TOTP code
  projectKey = VITE_X_BLOCKS_KEY
```

```
On success → TOTP enrollment confirmed, show success state
On 400     → Wrong code (time drift or wrong scan) — ask user to try again
```

---

## Disabling MFA

To remove MFA from a user account:

```
Action: disable-user-mfa
Input:
  userId     = target user's ID
  projectKey = VITE_X_BLOCKS_KEY
```

---

## MFA During Login (reference)

MFA verification during login uses `get-token` with `grant_type=mfa_code`, not `verify-otp`.
See `flows/login-flow.md` Step 3 for the login-time MFA flow.

`verify-otp` is used for **standalone MFA verification** outside the login token flow.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `generate-otp` 400 | MFA not enabled on user | Call update-user to set mfaEnabled=true first |
| `verify-otp` 400 | Wrong or expired code | Show error, allow retry or resend |
| `resend-otp` 429 | Rate limited | Show wait message |
| `setup-totp` 401 | Token expired | Run refresh-token then retry |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/auth/pages/mfa-setup/mfa-setup-page.tsx` | MFA type selection and setup entry |
| `modules/auth/components/totp-setup/totp-setup.tsx` | QR code display + secret key text fallback |
| `modules/auth/components/otp-input/otp-input.tsx` | Digit-by-digit OTP input with auto-submit |
| `modules/auth/pages/verify-otp/verify-otp-page.tsx` | OTP verification step |
| `modules/auth/hooks/use-mfa.tsx` | `useGenerateOtp`, `useVerifyOtp`, `useSetupTotp`, `useResendOtp` hooks |
| `routes/auth.route.tsx` | `/mfa-setup`, `/verify-otp` routes |
