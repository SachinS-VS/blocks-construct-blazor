# Flow: send-email-flow

## Trigger

User wants to send an email — either to a registered user via a template, or ad-hoc to any email address.

> "send an email to a user"
> "email a user when they register"
> "notify someone via email"
> "send a transactional email"
> "send a welcome email"

---

## Pre-flight Questions

Before starting, confirm:

1. Is there an existing email template for this purpose? If yes, what is its `purpose` value?
2. Is the recipient a registered user in the system (has a `userId`)? Or are you sending to an arbitrary email address?
3. Does the email body need dynamic values (e.g. name, activation link)? If yes, what are the variable names and their values?
4. What language should the email be sent in?
5. Are there any CC or BCC recipients?

---

## Flow Steps

### Step 1 — Determine Sending Mode

Determine which action to use based on recipient type and template availability:

| Condition | Use |
|-----------|-----|
| Recipient has a `userId` AND a template exists for the `purpose` | `send-email-with-template` |
| Recipient is an arbitrary email address OR no matching template exists | `send-email-to-any` |

---

### Step 2a — Template Branch: Verify Template Exists

If using template mode, confirm the template exists before sending.

```
Action: get-templates
Input:  projectKey = $VITE_PROJECT_SLUG
Output: list of templates with name, purpose, language
```

Scan the response for a template whose `purpose` matches the intended email type.

- Template found → proceed to Step 3a
- Template not found → run `manage-templates-flow` to create one, then return to Step 3a

---

### Step 3a — Template Branch: Send Email with Template

```
Action: send-email-with-template
Input:
  userId         = target user's ID
  purpose        = template purpose identifier (e.g. "welcome", "password-reset")
  language       = "en" (or user's locale)
  bodyDataContext = {
    "firstName": "Jane",
    "activationLink": "https://app.example.com/activate?code=abc123"
  }
  projectKey     = $VITE_PROJECT_SLUG
```

On `isSuccess: true` → confirm to user that the email was queued.
On `isSuccess: false` → inspect `errors` and surface field messages.

---

### Step 2b — Ad-hoc Branch: Send Email to Any Address

```
Action: send-email-to-any
Input:
  to         = ["recipient@example.com"]
  cc         = []
  bcc        = []
  subject    = "Your subject here"
  body       = "<p>Full HTML or plain text body.</p>"
  purpose    = "transactional" (optional label)
  language   = "en"
  projectKey = $VITE_PROJECT_SLUG
```

On `isSuccess: true` → confirm email was queued.
On `isSuccess: false` → inspect `errors` and correct the request.

---

### Step 4 — Confirm

After either branch succeeds, confirm to the developer:

> Email queued for delivery. No delivery receipt is returned — check the mailbox using `get-mailbox-mails` to verify the email was recorded.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `isSuccess: false` with `purpose` error | No template found for the given purpose/language | Run `manage-templates-flow` to create the template first |
| `isSuccess: false` with `userId` error | User ID does not exist | Verify the user via `identity-access` get-user action |
| `isSuccess: false` with `to` error | Missing or invalid recipient email | Check `to` field formatting (must be valid email array) |
| `401` | `ACCESS_TOKEN` expired | Re-run `get-token` |
| `403` | Missing `cloudadmin` role | Verify role in Cloud Portal → People |
| `404` | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/communication/pages/mail-compose/mail-compose-page.tsx` | Compose page with mode toggle (ad-hoc vs template), form, and submit |
| `modules/communication/hooks/use-communication.tsx` | `useSendMailToAny`, `useSendMailWithTemplate` mutations |
| `modules/communication/services/communication.service.ts` | `sendMailToAny()`, `sendMailWithTemplate()` functions |
| `modules/communication/types/communication.type.ts` | `SendMailToAnyPayload`, `SendMailPayload` interfaces |
