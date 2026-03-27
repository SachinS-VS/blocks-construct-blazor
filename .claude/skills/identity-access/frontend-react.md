# Identity & Access — Frontend Guide

This file extends `core/frontend-react.md` with auth-specific patterns for the identity-access skill.
Always read `core/frontend-react.md` first, then apply the overrides and additions here.

---

## Module Structure

All auth UI lives inside `src/modules/auth/`:

```
modules/auth/
├── components/
│   ├── signin-email/         ← email + password form
│   ├── signin-sso/           ← social login buttons
│   ├── signin-oidc/          ← OIDC redirect button
│   ├── signup/               ← email-only registration form
│   ├── set-password/         ← password input with strength indicator
│   ├── forgot-password/      ← email + captcha form
│   ├── reset-password/       ← new password form
│   └── otp-input/            ← digit-by-digit OTP component
├── pages/
│   ├── signin/               ← main login page (conditionally renders method)
│   ├── signup/               ← registration page
│   ├── activate/             ← account activation page (reads code from URL)
│   ├── verify-mfa/           ← MFA OTP entry page
│   ├── forgot-password/      ← forgot password page
│   ├── reset-password/       ← reset password page (reads code from URL)
│   ├── sent-email/           ← confirmation after forgot-password submission
│   ├── success/              ← confirmation after account activation
│   └── activate-failed/      ← error page for invalid/expired activation codes
├── hooks/
│   └── use-auth.tsx          ← all auth mutations and queries
├── services/
│   └── auth.service.ts       ← raw API calls (no state logic)
└── types/
    └── auth.type.ts          ← TypeScript types for auth payloads and responses
```

---

## State Management

Use **Zustand** with the `persist` middleware for auth state. Storage key: `"auth-storage"`.

```typescript
// src/state/store/auth/index.tsx
interface AuthState {
  isAuthenticated: boolean
  accessToken: string
  refreshToken: string
  user: User | null

  login: (accessToken: string, refreshToken: string) => void
  setAccessToken: (token: string) => void
  logout: () => void
}
```

Rules:
- `accessToken` and `refreshToken` are stored in Zustand persist — survives page refresh
- Never store tokens in component state or context
- `isAuthenticated` is derived from `accessToken !== ""`
- On logout: call `logout()` which clears all auth state and removes from storage

---

## HTTP Client

All API calls use the shared `https` client from `src/lib/https.ts`. It automatically attaches auth headers and refreshes the access token on 401 before retrying.

See `core/app-scaffold-react.md` for the full implementation.

```typescript
// Usage in service layer — no manual token handling needed
import https from '@/lib/https'

export const createUser = (payload: CreateUserPayload) =>
  https.post('/idp/v1/User/Create', payload)
```

---

## OTP Input Component

The OTP input is a specialized component in `components/core/otp-input/`.

Behaviour rules:
- Render individual single-character inputs (one per digit)
- Auto-focus next input on each character entry
- Auto-submit the form when all digits are filled
- Support backspace to move to previous input
- Email OTP: **5 digits**
- TOTP (authenticator app): **6 digits**

```tsx
<OtpInput
  length={mfaType === 'email' ? 5 : 6}
  onComplete={(otp) => handleVerify(otp)}
/>
```

---

## Password Strength Indicator

Show a strength indicator below the password input on all set-password and reset-password forms.

Rules:
- Show strength as: Weak / Fair / Strong / Very Strong
- Minimum required: Strong (8+ chars, uppercase, lowercase, number, special char)
- Do not enable the submit button until password meets minimum strength
- Show requirements checklist inline as the user types

---

## CAPTCHA Integration

CAPTCHA is optional per project. Always check `VITE_CAPTCHA_SITE_KEY` before rendering.

```typescript
// Only render CAPTCHA if the env variable is set
const captchaEnabled = !!import.meta.env.VITE_CAPTCHA_SITE_KEY
```

Supported providers (via `VITE_CAPTCHA_TYPE`):
- `reCaptcha` — Google reCAPTCHA v2
- `hCaptcha` — hCaptcha

On pages that use CAPTCHA (forgot-password, activate):
- Mount the CAPTCHA widget when the page loads
- On forgot-password: show the widget after the user starts typing in the email field
- Pass the resolved token to the API call as `captchaCode`
- If CAPTCHA token expires before form submission, reset and require re-completion

---

## Protected Routes

Wrap all authenticated routes with a `<ProtectedRoute>` component:

```tsx
// Redirects to /login if not authenticated
// Prevents flash of unauthenticated content by returning null until auth state is resolved
const ProtectedRoute = ({ children }) => {
  const { isAuthenticated } = useAuthState()
  if (!isAuthenticated) return <Navigate to="/login" replace />
  return children
}
```

Public routes (no auth required): `/login`, `/signup`, `/activate`, `/forgot-password`, `/resetpassword`, `/sent-email`, `/success`, `/activate-failed`

---

## Error Handling

Auth errors follow a consistent pattern across all forms:

```typescript
// Map API error codes to user-friendly messages
const AUTH_ERROR_MAP = {
  INVALID_CREDENTIALS: 'Invalid email or password',
  EMAIL_PASSWORD_NOT_VALID: 'Invalid email or password',
  invalid_request: 'Something went wrong. Please try again.',
}
```

Rules:
- Show field-level errors inline (under the relevant input) for validation errors
- Show form-level errors in an `<ErrorAlert />` component for API errors
- Never show raw error codes or stack traces to the user
- On 401 during login (not token refresh): show "Invalid credentials" — never "Unauthorized"

---

## Route Definitions

```typescript
// src/routes/auth.route.tsx
const authRoutes = [
  { path: '/login',           element: <SigninPage /> },
  { path: '/signup',          element: <SignupPage /> },
  { path: '/activate',        element: <AccountActivationPage /> },
  { path: '/activate-failed', element: <VerificationFailedPage /> },
  { path: '/success',         element: <ActivationSuccessPage /> },
  { path: '/forgot-password', element: <ForgotPasswordPage /> },
  { path: '/sent-email',      element: <EmailSentPage /> },
  { path: '/resetpassword',   element: <ResetPasswordPage /> },
  { path: '/verify-mfa',      element: <VerifyMfaPage /> },
  { path: '/oidc',            element: <OidcCallbackPage /> },
]
```

---

## Hooks Pattern

All auth mutations use React Query's `useMutation`. Centralize in `use-auth.tsx`:

```typescript
export const useSigninMutation = () => useMutation({
  mutationFn: (payload: SigninEmailPayload) => authService.signin(payload),
})

export const useGetLoginOptions = () => useQuery({
  queryKey: ['loginOptions'],
  queryFn: () => authService.getLoginOptions(),
})
```

Rules:
- One hook per API operation
- Service functions are pure (no state) — hooks own the state
- Use `useGlobalMutation` for mutations that need centralized error handling
- Always handle `onError` to show user-facing error messages

---

## TypeScript Types

Define all auth payload and response types in `auth.type.ts`:

```typescript
// Payloads (sent to API)
export interface SigninEmailPayload { username: string; password: string }
export interface AccountActivationPayload { password: string; code: string; captchaCode?: string; projectKey: string }
export interface ForgotPasswordPayload { email: string; captchaCode?: string; projectKey: string }

// Responses (from API)
export interface SigninTokenResponse { access_token: string; token_type: string; expires_in: number; refresh_token: string }
export interface SigninMfaResponse { enable_mfa: true; mfaType: 'email' | 'authenticator'; mfaId: string; message: string }
export type SigninResponse = SigninTokenResponse | SigninMfaResponse

// Type guard
export const isMfaResponse = (res: SigninResponse): res is SigninMfaResponse =>
  (res as SigninMfaResponse).enable_mfa === true
```
