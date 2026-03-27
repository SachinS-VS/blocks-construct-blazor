# Flow: user-registration

## Trigger

User wants to build a self-registration or account activation flow.

> "build a signup page"
> "add user registration"
> "implement account activation"
> "build onboarding for new users"

---

## Pre-flight Questions

Before starting, confirm:

1. Is this **self-registration** (user signs up themselves) or **admin-created** (admin creates user, user activates)? *(determines which steps are included)*
2. Is CAPTCHA required on the activation form?
3. Is email uniqueness check needed before showing the form?
4. Where should the user be redirected after successful activation?

---

## Flow Steps

### Self-Registration Path

#### Step 1 — Check Email Availability (optional but recommended)

Before or during form input, call `check-email-available` to give early feedback.

```
Action: check-email-available
Input:  email (query param)
Output: true (available) | false (taken)
```

If `false` → show "This email is already registered" inline. Do not proceed.

---

#### Step 2 — Submit Signup (email only)

Collect email address. On submit, call `create-user` with minimal fields.
The backend sends an activation email automatically.

```
Action: create-user
Input:
  email             = user's email
  userCreationType  = "SelfService"
  allowedLogInType  = ["Email"]
  projectKey        = VITE_X_BLOCKS_KEY
```

```
On success → show "Check your email" confirmation page
On 400     → duplicate email or invalid format, show inline error
```

---

#### Step 3 — User Clicks Activation Link

User receives email with activation link containing a `code` parameter.
App extracts `code` from URL on the `/activate` route.

---

#### Step 4 — Validate Activation Code

Before showing the password form, validate the code is still valid.

```
Action: validate-activation-code
Input:
  code       = code from URL
  projectKey = VITE_X_BLOCKS_KEY
```

```
On success → show set-password form (Step 5)
On 400     → code invalid or expired → redirect to /activate-failed
```

---

#### Step 5 — Set Password and Activate

Collect new password. If CAPTCHA is enabled, show CAPTCHA widget first.

```
Action: activate-user
Input:
  code             = code from URL
  password         = user's chosen password
  projectKey       = VITE_X_BLOCKS_KEY
  captchaCode      = captcha token (if enabled)
  preventPostEvent = false
```

Password constraints: min 8 chars, must include uppercase, lowercase, number, special character.

```
On success → redirect to /success confirmation page
On 400     → invalid code or weak password, show inline error
```

---

#### Step 6 — Resend Activation (if needed)

If the activation email expires or was not received, allow resend.

```
Action: resend-activation
Input:
  email      = user's email
  projectKey = VITE_X_BLOCKS_KEY
```

---

### Admin-Created Path

Admin creates user via `create-user` in the backend. The rest of the flow from Step 3 onwards is identical — user receives activation email and sets their password.

```
Action: create-user
Input:
  email            = user's email
  firstName        = optional
  lastName         = optional
  userCreationType = "AdminCreated"
  allowedLogInType = ["Email"]
  mfaEnabled       = true/false based on pre-flight answer
  projectKey       = VITE_X_BLOCKS_KEY
  organizationId   = org ID if applicable
```

---

## CAPTCHA Integration

CAPTCHA is shown on the activation/set-password page.
Load CAPTCHA widget on page mount. Pass the token to `activate-user` as `captchaCode`.

Supported types (from env): `reCaptcha`, `hCaptcha`

If captcha fails or token is missing on submission → show "Please complete the CAPTCHA" error.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `check-email-available` returns false | Email taken | Show inline "already registered" message |
| `create-user` 400 | Duplicate email or invalid field | Show field-level error |
| `validate-activation-code` 400 | Code expired/invalid | Redirect to /activate-failed page |
| `activate-user` 400 | Weak password or bad code | Show inline error with password requirements |
| `resend-activation` 400 | User already activated | Show "Account already active, please login" |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/auth/pages/signup/signup-page.tsx` | Email input page |
| `modules/auth/components/signup/signup-form.tsx` | Email + terms form |
| `modules/auth/pages/activate/account-activation-page.tsx` | Extracts code from URL, validates it |
| `modules/auth/components/set-password/set-password.tsx` | Password input with strength indicator + CAPTCHA |
| `modules/auth/pages/success/activation-success-page.tsx` | Confirmation after activation |
| `modules/auth/pages/activate-failed/verification-failed-page.tsx` | Error page for bad/expired codes |
| `modules/auth/hooks/use-auth.tsx` | `useAccountActivation`, `useResendActivation` hooks |
| `modules/auth/services/auth.service.ts` | `accountActivation()`, `resendActivation()` |
| `modules/auth/types/auth.type.ts` | `AccountActivationPayload` |
| `routes/auth.route.tsx` | `/signup`, `/activate`, `/success`, `/activate-failed` routes |
