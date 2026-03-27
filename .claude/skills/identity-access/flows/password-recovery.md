# Flow: password-recovery

## Trigger

User wants to build a forgot password or password reset feature.

> "build forgot password"
> "add password reset"
> "implement password recovery"

---

## Pre-flight Questions

Before starting, confirm:

1. Is CAPTCHA required on the forgot password form?
2. Where should the user be redirected after successful password reset?
3. Should the user be logged out of all sessions after resetting?

---

## Flow Steps

### Step 1 — Forgot Password Form

Collect user's email address. If CAPTCHA is enabled, show CAPTCHA widget when the email field has input.

```
Action: recover-user
Input:
  email      = user's registered email
  projectKey = VITE_X_BLOCKS_KEY
  captchaCode = captcha token (if enabled)
```

```
On success → show /sent-email confirmation page
On 404     → email not found, show "If this email is registered, you will receive a link"
             (do not confirm or deny email existence for security)
```

> Security note: Always show the same success message regardless of whether the email exists. This prevents email enumeration attacks.

---

### Step 2 — Email Sent Confirmation

Show a static confirmation page:
> "If your email is registered, you will receive a password reset link shortly."

Provide a "Resend" option that calls Step 1 again.

---

### Step 3 — User Clicks Reset Link

User receives email with a link containing a `code` parameter.
App extracts `code` from URL on the `/resetpassword` route.

---

### Step 4 — Reset Password Form

Collect new password. Show password strength indicator.

```
Action: reset-password
Input:
  code        = code from URL
  newPassword = user's new password
  projectKey  = VITE_X_BLOCKS_KEY
```

Password constraints: min 8 chars, must include uppercase, lowercase, number, special character.

```
On success → continue to Step 5
On 400     → code expired/invalid or weak password, show inline error
```

---

### Step 5 — Redirect to Login

After a successful password reset, redirect the user to `/login`.

```
→ Redirect to /login
→ Show toast: "Password reset successful. Please log in."
```

> Session invalidation on password reset is handled by the backend automatically — do NOT call `logout-all` from this flow. The user is unauthenticated during reset so calling `logout-all` would fail with 401. The backend invalidates active sessions when the password changes.

---

## CAPTCHA Integration

CAPTCHA on forgot password is triggered conditionally:
- Show CAPTCHA widget after the user starts typing in the email field
- Required before form submission if enabled
- Pass token as `captchaCode` to `recover-user`

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `recover-user` 404 | Email not registered | Show generic success message (do not reveal) |
| `reset-password` 400 (invalid code) | Link expired or already used | Show "This link has expired. Please request a new one." with link to /forgot-password |
| `reset-password` 400 (weak password) | Doesn't meet strength requirements | Show password requirements inline |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/auth/pages/forgot-password/forgot-password-page.tsx` | Email input + optional CAPTCHA |
| `modules/auth/components/forgot-password/forgot-password-form.tsx` | Form component |
| `modules/auth/pages/sent-email/email-sent-page.tsx` | Confirmation page after submission |
| `modules/auth/pages/reset-password/reset-password-page.tsx` | Extracts code from URL, renders form |
| `modules/auth/components/reset-password/reset-password-form.tsx` | New password input with strength indicator |
| `modules/auth/hooks/use-auth.tsx` | `useForgotPassword`, `useResetPassword` hooks |
| `modules/auth/services/auth.service.ts` | `forgotPassword()`, `resetPassword()` |
| `modules/auth/types/auth.type.ts` | `ForgotPasswordPayload` |
| `routes/auth.route.tsx` | `/forgot-password`, `/sent-email`, `/resetpassword` routes |
