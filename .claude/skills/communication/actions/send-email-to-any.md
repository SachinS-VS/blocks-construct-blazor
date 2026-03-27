# Action: send-email-to-any

## Purpose

Send an email to one or more addresses without requiring a registered user or saved template. Use this for ad-hoc transactional emails where you supply the full subject and body directly.

---

## Endpoint

```
POST $VITE_API_BASE_URL/communication/v1/Mail/SendToAny
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Mail/SendToAny" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "to": ["recipient@example.com"],
    "cc": [],
    "bcc": [],
    "subject": "Your subject here",
    "body": "<p>Your email body here.</p>",
    "purpose": "transactional",
    "language": "en",
    "attachments": [],
    "projectKey": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| to | string[] | yes | One or more recipient email addresses |
| cc | string[] | no | Carbon copy recipients |
| bcc | string[] | no | Blind carbon copy recipients |
| subject | string | yes | Email subject line |
| body | string | yes | Email body — HTML is supported |
| purpose | string | no | Category identifier (e.g. `"welcome"`, `"invoice"`) |
| language | string | no | BCP 47 language code — defaults to `"en"` |
| attachments | array | no | Leave as empty array if unused |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

Email has been queued for delivery. No mail ID is returned.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "to": "At least one recipient is required",
    "subject": "Subject cannot be empty"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Validation error on one or more fields | Inspect `errors` dictionary and correct the request |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |
