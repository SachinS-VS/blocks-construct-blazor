# Action: save-template

## Purpose

Create a new email template or update an existing one. Omit `itemId` to create; include it to update. Templates are referenced by `purpose` when sending emails via `send-email-with-template`.

---

## Endpoint

```
POST $VITE_API_BASE_URL/communication/v1/Template/Save
```

---

## curl — Create

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Template/Save" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "name": "Welcome Email",
    "templateSubject": "Welcome to {{appName}}!",
    "templateBody": "<h1>Hello {{firstName}},</h1><p>Welcome to {{appName}}. Click <a href=\"{{activationLink}}\">here</a> to activate your account.</p>",
    "language": "en",
    "purpose": "welcome",
    "projectKey": "'"$VITE_PROJECT_SLUG"'"
  }'
```

## curl — Update

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Template/Save" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "itemId": "template-id-123",
    "name": "Welcome Email v2",
    "templateSubject": "Welcome to {{appName}}!",
    "templateBody": "<h1>Hi {{firstName}},</h1><p>Updated welcome message.</p>",
    "language": "en",
    "purpose": "welcome",
    "projectKey": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| itemId | string | no (create) / yes (update) | Omit to create a new template |
| name | string | yes | Human-readable template name |
| templateSubject | string | yes | Subject line — supports `{{variableName}}` placeholders |
| templateBody | string | yes | Full HTML body — supports `{{variableName}}` and `{{.FieldName}}` placeholders |
| mailConfigurationId | string | no | Reference to a mail server configuration |
| language | string | no | BCP 47 code — defaults to `"en"` |
| purpose | string | yes | Slug-style identifier — used when sending via `send-email-with-template` |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

---

## Template Variable Syntax

| Syntax | Use case |
|--------|----------|
| `{{variableName}}` | Simple key — resolved from top-level `bodyDataContext` keys |
| `{{.FieldName}}` | Object field access — resolved from nested `bodyDataContext` fields |

Both are injected at send time from the `bodyDataContext` object in `SendMailRequest`.

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
    "purpose": "A template with this purpose already exists for this language",
    "templateBody": "Template body cannot be empty"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Validation error, duplicate purpose/language combo, or ID not found | Inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- `purpose` should be a slug-style lowercase string (e.g. `"welcome"`, `"password-reset"`, `"invoice"`).
- Multiple language variants of the same template can be created by saving with the same `purpose` but different `language` values.
- To verify the saved template, use `get-template` with the returned (or known) `itemId`.
