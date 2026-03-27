# Action: get-template

## Purpose

Retrieve a single email template by its ID. Use this to load an existing template into the editor for review or update.

---

## Endpoint

```
GET $VITE_API_BASE_URL/communication/v1/Template/Get
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Template/Get?itemId=template-id-123&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| itemId | string | yes | ID of the template to retrieve — from `get-templates` |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "template": {
    "itemId": "template-id-123",
    "name": "Welcome Email",
    "templateSubject": "Welcome to {{appName}}!",
    "templateBody": "<h1>Hello {{firstName}},</h1><p>Welcome to the platform.</p>",
    "purpose": "welcome",
    "language": "en",
    "createdDate": "2024-05-01T08:00:00Z",
    "lastUpdatedDate": "2024-06-01T10:00:00Z"
  },
  "isSuccess": true,
  "errors": {}
}
```

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Template not found for the given `itemId` | Verify the ID from `get-templates` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- The `templateBody` field contains raw HTML including `{{variableName}}` placeholders — render these safely in a sandboxed `<iframe>` for preview.
- Use `itemId` from this response as the `itemId` field when calling `save-template` to update.
