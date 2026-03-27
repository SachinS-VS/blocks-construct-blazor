# Action: get-templates

## Purpose

List all email templates for the project with optional search and sort. Use this to populate the template management page and to find a template's `itemId` before updating, cloning, or deleting.

---

## Endpoint

```
GET $VITE_API_BASE_URL/communication/v1/Template/Gets
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Template/Gets?projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

### With search and sort

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Template/Gets?search=welcome&sort=name&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| search | string | no | Filter by template name (partial match) |
| sort | string | no | Field name to sort by (e.g. `"name"`, `"createdDate"`) |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "templates": [
    {
      "itemId": "template-id-123",
      "name": "Welcome Email",
      "templateSubject": "Welcome to {{appName}}!",
      "templateBody": "<h1>Hello {{firstName}},</h1>...",
      "purpose": "welcome",
      "language": "en",
      "createdDate": "2024-05-01T08:00:00Z",
      "lastUpdatedDate": "2024-06-01T10:00:00Z"
    },
    {
      "itemId": "template-id-456",
      "name": "Password Reset",
      "templateSubject": "Reset your password",
      "templateBody": "<p>Click <a href=\"{{resetLink}}\">here</a> to reset.</p>",
      "purpose": "password-reset",
      "language": "en",
      "createdDate": "2024-04-15T09:00:00Z",
      "lastUpdatedDate": "2024-04-15T09:00:00Z"
    }
  ],
  "totalCount": 2,
  "isSuccess": true,
  "errors": {}
}
```

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Missing `projectKey` | Inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- This endpoint does not paginate — all templates are returned in a single response. Use `totalCount` for display purposes.
- Use the `itemId` from each template to call `get-template`, `save-template` (update), `clone-template`, or `delete-template`.
- The `search` param filters by name only. To filter by purpose, apply client-side filtering after fetching.
