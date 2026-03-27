# Action: get-mailbox-mail

## Purpose

Retrieve a single email from the project mailbox by its ID. Use this to display the full email body and metadata.

---

## Endpoint

```
GET $VITE_API_BASE_URL/communication/v1/Mail/GetMailBoxMail
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Mail/GetMailBoxMail?itemId=mail-id-123&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| itemId | string | yes | ID of the email to retrieve — obtained from `get-mailbox-mails` |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "mail": {
    "itemId": "mail-id-123",
    "subject": "Welcome to the platform",
    "to": ["user@example.com"],
    "from": "noreply@example.com",
    "body": "<p>Hello Jane, click <a href='...'>here</a> to activate.</p>",
    "purpose": "welcome",
    "language": "en",
    "sentTime": "2024-06-01T10:00:00Z",
    "isRead": true
  },
  "isSuccess": true,
  "errors": {}
}
```

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Email not found for the given `itemId` | Verify the ID from `get-mailbox-mails` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- The `body` field contains HTML — render it safely using a sandboxed `<iframe>` in the frontend, never via `dangerouslySetInnerHTML`.
- Use `itemId` values from the `get-mailbox-mails` response to navigate to individual emails.
