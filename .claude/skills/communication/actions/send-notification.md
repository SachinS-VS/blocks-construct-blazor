# Action: send-notification

## Purpose

Send an in-app notification to one or more users. Recipients can be targeted by user ID, role name, or subscription filter key — these targeting methods can be combined in a single request.

---

## Endpoint

```
POST $VITE_API_BASE_URL/communication/v1/Notifier/Notify
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Notifier/Notify" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "userIds": ["user-id-abc"],
    "roles": [],
    "subscriptionFilters": [],
    "denormalizedPayload": "{\"message\": \"Your order has been shipped.\", \"orderId\": \"ORD-001\"}",
    "configuratoinName": "",
    "projectKey": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| userIds | string[] | no | Target specific users by their user ID |
| roles | string[] | no | Target all users with the specified role names |
| subscriptionFilters | string[] | no | Target users subscribed to these filter keys |
| denormalizedPayload | string | yes | Notification content — typically a JSON string or plain text |
| configuratoinName | string | no | Server-side notification config name — note the API typo (intentional) |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

> At least one of `userIds`, `roles`, or `subscriptionFilters` must be provided to target recipients.

> **API Typo:** `configuratoinName` is misspelled in the API contract. Use this exact spelling — do not correct it.

---

## Targeting Examples

### By user ID
```json
{
  "userIds": ["user-id-abc", "user-id-def"],
  "denormalizedPayload": "Your report is ready.",
  "projectKey": "my-project"
}
```

### By role
```json
{
  "roles": ["admin", "manager"],
  "denormalizedPayload": "New user registration requires approval.",
  "projectKey": "my-project"
}
```

### By subscription filter
```json
{
  "subscriptionFilters": ["order-updates"],
  "denormalizedPayload": "{\"type\": \"order-shipped\", \"orderId\": \"ORD-001\"}",
  "projectKey": "my-project"
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
    "userIds": "No valid recipients found"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | No recipients found, or missing required fields | Inspect `errors`; verify user IDs and roles exist |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |
