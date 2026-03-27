# Flow: session-management

## Trigger

User wants to build session visibility or logout functionality.

> "build logout"
> "show active sessions"
> "let users manage their sessions"
> "add logout from all devices"

---

## Pre-flight Questions

Before starting, confirm:

1. Is this a single-session logout or all-sessions logout (or both)?
2. Should the user see a list of their active sessions?
3. Where should the user be redirected after logout?

---

## Flow Steps

### Logout Current Session

Invalidate the current session by passing the refresh token to the logout endpoint.

```
Action: logout
Input:
  refreshToken = current REFRESH_TOKEN (from app state)
  ACCESS_TOKEN = required in Authorization header
```

```
On success (or 401) → clear ACCESS_TOKEN and REFRESH_TOKEN from app state
                    → clear any persisted auth storage (e.g. localStorage)
                    → redirect to /login
```

> Always clear tokens regardless of the response — even if the token was already expired, local state must be cleared.

---

### Logout All Sessions

Invalidate all active sessions for the user across all devices.

```
Action: logout-all
Input: ACCESS_TOKEN only (no body)
```

```
On success (or 401) → clear ACCESS_TOKEN and REFRESH_TOKEN from app state
                    → redirect to /login
```

Use this after:
- Password reset (security requirement)
- Suspicious activity detected
- "Sign out everywhere" button

---

### View Active Sessions

Fetch and display the user's active sessions.

```
Action: get-sessions
Input: ACCESS_TOKEN (no body or query params needed)
```

Display per session:
- Device / browser info
- IP address
- Last active timestamp
- Location (if available)

Provide a "Logout this device" button per row that calls the `logout` action for that session.

---

## Token Cleanup Checklist

On any logout path, always:
- [ ] Clear `ACCESS_TOKEN` from app state
- [ ] Clear `REFRESH_TOKEN` from app state
- [ ] Clear `auth-storage` from localStorage/sessionStorage
- [ ] Redirect to `/login`
- [ ] Do NOT call any further authenticated API calls after logout

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `logout` 401 | Token already expired | Still clear tokens and redirect — logout is complete |
| `logout-all` 401 | Token already expired | Still clear tokens and redirect |
| `get-sessions` 401 | Token expired | Run refresh-token then retry |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/auth/components/logout-button/logout-button.tsx` | Single logout button (header/navbar) |
| `modules/auth/components/logout-all-button/logout-all-button.tsx` | Logout all devices button (settings page) |
| `modules/auth/pages/sessions/sessions-page.tsx` | Active sessions list |
| `modules/auth/components/session-card/session-card.tsx` | Individual session row with device info |
| `modules/auth/hooks/use-auth.tsx` | `useSignoutMutation`, `useLogoutAllMutation` hooks |
| `modules/auth/services/auth.service.ts` | `signout()`, `logoutAll()` |
