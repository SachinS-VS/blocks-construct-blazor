# Security Standards

Security rules that apply to **all generated code** — frontend and backend. Following these makes generated output SAST-compliant by default.

For SCA (dependency scanning) and CI/CD pipeline security, see `devsecops/` (planned).

---

## Secrets and Credentials

### Never hardcode secrets

```tsx
// ❌ hardcoded secret
const apiKey = 'sk-abc123'
const token = 'eyJhbGci...'

// ✅ environment variable
const apiKey = import.meta.env.VITE_X_BLOCKS_KEY
```

### Never put private values in VITE_ variables

`VITE_` prefixed variables are bundled into the client and exposed in the browser. Only public, non-sensitive values belong there.

```bash
# ✅ safe in VITE_ — public keys, non-sensitive config
VITE_API_BASE_URL=https://api.seliseblocks.com
VITE_X_BLOCKS_KEY=your_blocks_key
VITE_CAPTCHA_SITE_KEY=your_public_captcha_key

# ❌ never in VITE_ — server secrets, private keys
VITE_DB_PASSWORD=...
VITE_SECRET_KEY=...
```

### Never log tokens

```tsx
// ❌ logs sensitive data
console.log('token:', accessToken)
console.log('user:', JSON.stringify(userData))

// ✅ log only non-sensitive identifiers
console.log('authenticated as:', userId)
```

### Never store tokens in localStorage directly

The Zustand persist store uses localStorage, but access tokens must not be stored in raw localStorage outside of the persisted auth store. The store must use the `storage` adapter from `zustand/middleware`.

```tsx
// ❌ direct localStorage write
localStorage.setItem('access_token', token)

// ✅ use the auth store
useAuthStore.setState({ accessToken: token })
```

---

## Input Validation

### Validate all user inputs with Zod before submitting

Every form must have a Zod schema. Never submit raw form values to an API without schema validation.

```tsx
const schema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
})

// validate before calling API
const result = schema.safeParse(formData)
if (!result.success) {
  // show validation errors — do not proceed
}
```

### Validate API responses before using them

Never assume an API response has the shape you expect. Use Zod or explicit checks before reading nested fields.

```tsx
// ❌ unsafe
const name = response.data.user.name

// ✅ check first
if (response?.data?.user?.name) {
  const name = response.data.user.name
}
```

### Never trust user-provided redirect URLs

Always validate redirect targets against an allowlist before navigating.

```tsx
// ❌ open redirect
window.location.href = searchParams.get('redirect')

// ✅ validate against allowed paths
const redirect = searchParams.get('redirect')
const allowedPaths = ['/dashboard', '/profile', '/settings']
if (redirect && allowedPaths.includes(redirect)) {
  navigate(redirect)
} else {
  navigate('/dashboard')
}
```

---

## XSS Prevention

### Never use dangerouslySetInnerHTML

```tsx
// ❌ XSS risk
<div dangerouslySetInnerHTML={{ __html: userContent }} />

// ✅ let React escape it
<div>{userContent}</div>
```

If HTML rendering is truly required (e.g., rich text from a trusted CMS), sanitize with DOMPurify first:

```tsx
import DOMPurify from 'dompurify'

<div dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(content) }} />
```

### Never use eval() or new Function()

```tsx
// ❌ arbitrary code execution
eval(userInput)
new Function(userInput)()

// ✅ use structured data instead of dynamic code
```

### Never construct URLs from unsanitized input

```tsx
// ❌ XSS via javascript: scheme
const url = userInput // could be "javascript:alert(1)"
<a href={url}>link</a>

// ✅ validate scheme
const isSafeUrl = (url: string) =>
  url.startsWith('https://') || url.startsWith('/')
<a href={isSafeUrl(url) ? url : '#'}>link</a>
```

---

## Authentication and Authorization

### Always check auth state before rendering protected routes

```tsx
// ✅ route guard pattern
const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const { accessToken } = useAuthStore()
  if (!accessToken) return <Navigate to="/login" replace />
  return <>{children}</>
}
```

### Never expose role checks only on the frontend

Frontend role checks are for UX only (hiding buttons, redirecting). They are not a security boundary — the backend enforces authorization. Never skip a backend API call because the frontend determined the user "shouldn't have access".

```tsx
// ❌ treating frontend role check as security
if (user.role === 'admin') {
  await deleteUser(id) // backend must also enforce this
}

// ✅ always call the API — backend will return 403 if unauthorized
await deleteUser(id) // backend enforces, frontend handles 403 gracefully
```

### Handle 401 and 403 responses explicitly

```tsx
// ✅ explicit error handling
if (error.status === 401) {
  // token expired — trigger refresh or redirect to login
  authStore.clearTokens()
  navigate('/login')
}

if (error.status === 403) {
  // authorized but forbidden — show permission error, do not redirect to login
  toast.error('You do not have permission to perform this action.')
}
```

---

## Error Handling

### Never expose internal error details to users

```tsx
// ❌ leaks server internals
toast.error(error.message) // could be "SQL syntax error at line 42"

// ✅ user-friendly message, log internally
console.error('API error:', error)
toast.error('Something went wrong. Please try again.')
```

### Never swallow errors silently

```tsx
// ❌ silent failure
try {
  await submitForm()
} catch {
  // nothing — user sees no feedback
}

// ✅ always handle and surface
try {
  await submitForm()
} catch (error) {
  console.error('Form submission failed:', error)
  toast.error('Submission failed. Please try again.')
}
```

---

## Dependency Safety (SCA Basics)

While full SCA scanning belongs in the `devsecops/` pipeline, follow these rules when adding packages:

- **Run `npm audit` before adding any new package** — if it shows high/critical vulnerabilities, find an alternative
- **Prefer packages with recent activity** — last publish within 12 months, active maintainers
- **Avoid packages with no types** — type safety reduces a class of runtime errors
- **Pin versions for security-critical packages** — use exact versions (`"1.2.3"`) not ranges (`"^1.2.3"`) for auth, crypto, and sanitization libraries
- **Do not use deprecated packages** — check the npm page before adding

```bash
# Check before adding any new dependency
npm audit
npm info <package-name>  # check last publish date and maintainers
```

---

## SAST Checklist — Before Generating Any Code

Run through this before finalising any generated file:

- [ ] No hardcoded tokens, API keys, or passwords
- [ ] No `VITE_` variables containing private/server-side secrets
- [ ] All form inputs validated with Zod before API submission
- [ ] No `dangerouslySetInnerHTML` without DOMPurify
- [ ] No `eval()` or `new Function()`
- [ ] No open redirect from user-supplied URLs
- [ ] Auth state checked on all protected routes
- [ ] 401 and 403 handled explicitly in all API calls
- [ ] Error messages shown to users do not contain server internals
- [ ] No silent `catch {}` blocks

---

## What This Skill Does NOT Cover

The following require the `devsecops/` skill (planned):

- Running SAST tools (Semgrep, SonarQube, CodeQL) against the codebase
- Running SCA tools (Snyk, `npm audit`, OWASP Dependency Check) on CI
- Generating security scan reports
- Configuring pre-commit hooks for security linting
- Container image scanning
- Secret scanning in git history
