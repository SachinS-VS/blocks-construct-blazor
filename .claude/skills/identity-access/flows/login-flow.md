# Flow: login-flow

## Trigger

User wants to build a login page or authentication entry point.

> "build a login page"
> "add sign in to my app"
> "implement authentication"

---

## Pre-flight Questions

Before starting, confirm:

1. Which login methods are enabled? (email/password, social login, OIDC — can be multiple)
2. Is MFA required? If yes — email OTP, authenticator app (TOTP), or both?
3. Where should the user be redirected after successful login?
4. Is there a "remember me" requirement?

---

## Flow Steps

### Step 1 — Get Login Options

Call `get-login-options` to fetch which login methods are configured for the project.
Use the response to conditionally render the correct login UI.

```
Action: get-login-options
Input:  x-blocks-key only (public endpoint)
Output: list of enabled login providers
```

Branch based on response:
- Email/password enabled → continue to Step 2
- Social login enabled → render SSO buttons alongside (see SSO Branch)
- OIDC only → skip to OIDC Branch

---

### Step 2 — Email/Password Login

Render email + password form. On submit, call `get-token` with `grant_type=password`.

```
Action: get-token
Grant type: password
Input:
  grant_type = "password"
  username   = user's email
  password   = user's password
  client_id  = VITE_BLOCKS_OIDC_CLIENT_ID
```

**Response branches:**

#### Branch A — No MFA (enable_mfa: false)
```
Response fields: access_token, refresh_token, expires_in
→ Store access_token and refresh_token in app state
→ Redirect to home / protected route
```

#### Branch B — MFA required (enable_mfa: true)
```
Response fields: enable_mfa, mfaType, mfaId, message
→ Do NOT store tokens yet
→ Redirect to /verify-mfa?mfaType=...&mfaId=...
→ Continue to Step 3
```

> `mfaType` in this response is `"email"` or `"authenticator"` — these are OAuth token response strings, NOT the `UserMfaType` enum (`OTP`, `TOTP`) used in `generate-otp`.

---

### Step 3 — MFA Verification (only if Branch B)

Render OTP input. Auto-submit when the correct number of digits is entered.

| MFA type | Digits | Source |
|----------|--------|--------|
| email | 5 | Sent to user's email |
| authenticator | 6 | From TOTP app |

Call `get-token` again with `grant_type=mfa_code`.

```
Action: get-token
Grant type: mfa_code
Input:
  grant_type = "mfa_code"
  mfa_id     = mfaId from Step 2 response
  mfa_type   = mfaType from Step 2 response
  client_id  = VITE_BLOCKS_OIDC_CLIENT_ID
```

```
On success:
  → Store access_token and refresh_token
  → Redirect to home / protected route

On failure:
  → Show error, allow retry
  → Do not redirect
```

---

### SSO Branch (social login)

Fetch available providers from `get-login-options` response.
For each provider, render a button that calls `get-social-login-endpoint` (POST) then redirects to the returned URL.

On return from provider, the app receives an authorization code in the URL.
Exchange it via `get-token` with `grant_type=authorization_code`.

```
Action: get-token
Grant type: authorization_code
Input:
  grant_type   = "authorization_code"
  code         = code from URL param
  redirect_uri = VITE_BLOCKS_OIDC_REDIRECT_URI
  client_id    = VITE_BLOCKS_OIDC_CLIENT_ID
```

Check response for `enable_mfa` — if true, continue to Step 3.

---

### OIDC Branch

Build the authorization URL and redirect:

```
{VITE_API_BASE_URL}/idp/v1/Authentication/Authorize
  ?X-Blocks-Key={VITE_X_BLOCKS_KEY}
  &client_id={VITE_BLOCKS_OIDC_CLIENT_ID}
  &redirect_uri={VITE_BLOCKS_OIDC_REDIRECT_URI}
  &response_type=code
  &scope=openid
```

On callback, detect `code` and `state` params in URL.
Exchange code via `get-token` with `grant_type=authorization_code` (same as SSO Branch).

---

## Token Storage

After successful login (any path), store in app state:
- `access_token` → used as `Authorization: Bearer` header on all requests
- `refresh_token` → used to renew access token on 401
- Never store tokens in frontend environment variables or hardcode them

---

## Token Refresh (automatic)

On any API call returning 401:
1. Call `refresh-token` action with current `refresh_token`
2. Store new `access_token` and `refresh_token`
3. Retry the original failed request
4. If refresh also returns 401 → session expired, redirect to login

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| 400 | Wrong client_id or malformed request | Check VITE_BLOCKS_OIDC_CLIENT_ID |
| 401 | Wrong email/password | Show "Invalid credentials" |
| 403 | Account missing cloudadmin role | Not applicable for end users — check admin config |
| 404 | Wrong VITE_API_BASE_URL | Check environment URL |
| enable_mfa: true | MFA required | Redirect to MFA step |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/auth/pages/signin/signin-page.tsx` | Main login page — conditionally renders based on login options |
| `modules/auth/components/signin-email/signin-email.tsx` | Email/password form |
| `modules/auth/components/signin-sso/signin-sso.tsx` | Social login buttons |
| `modules/auth/components/signin-oidc/signin-oidc.tsx` | OIDC login button |
| `modules/auth/pages/verify-mfa/verify-mfa-page.tsx` | OTP input page |
| `modules/auth/hooks/use-auth.tsx` | `useSigninMutation`, `useGetLoginOptions` hooks |
| `modules/auth/services/auth.service.ts` | `signin()`, `getRefreshToken()` |
| `modules/auth/types/auth.type.ts` | `SigninEmailPayload`, `SigninEmailTokenResponse`, `SigninEmailMfaResponse` |
| `routes/auth.route.tsx` | `/login`, `/verify-mfa` routes |
