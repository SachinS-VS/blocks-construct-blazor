# Action: send-email-with-template

## Purpose

Send an email to a registered user using a pre-configured email template. The template is looked up by `purpose` and `language`. Dynamic values are injected into the template body using `bodyDataContext`.

---

## Endpoint

```
POST $VITE_API_BASE_URL/communication/v1/Mail/Send
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Mail/Send" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "userId": "user-id-here",
    "purpose": "welcome",
    "language": "en",
    "bodyDataContext": {
      "firstName": "Jane",
      "activationLink": "https://app.example.com/activate?code=abc123"
    },
    "attachments": [],
    "projectKey": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| userId | string | yes | ID of the registered user to send to |
| purpose | string | yes | Must exactly match the `purpose` field on the saved template |
| language | string | no | BCP 47 code — used to pick the right language variant of the template; defaults to `"en"` |
| bodyDataContext | object | no | Key/value pairs injected into `{{variableName}}` placeholders in the template |
| attachments | array | no | Leave as empty array if unused |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

---

## Template Variable Injection

The template body supports two placeholder syntaxes:

| Syntax | Example | Resolves from |
|--------|---------|---------------|
| `{{variableName}}` | `{{firstName}}` | Top-level key in `bodyDataContext` |
| `{{.FieldName}}` | `{{.ActivationLink}}` | Object field access in `bodyDataContext` |

Example template body:
```html
<p>Hello {{firstName}},</p>
<p>Click <a href="{{activationLink}}">here</a> to activate your account.</p>
```

With `bodyDataContext`:
```json
{
  "firstName": "Jane",
  "activationLink": "https://app.example.com/activate?code=abc123"
}
```

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "userId": "User not found",
    "purpose": "No template found for this purpose and language"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | User not found, or no template matches the purpose/language | Inspect `errors`; verify the template exists via `get-templates` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Prerequisites

A template with the matching `purpose` and `language` must exist before calling this action. Use `save-template` to create one if needed.
