# Action: get-mailbox-mails

## Purpose

Retrieve a paginated list of sent and received emails in the project mailbox.

---

## Endpoint

```
GET $VITE_API_BASE_URL/communication/v1/Mail/GetMailBoxMails
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Mail/GetMailBoxMails?page=1&pageSize=20&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| page | integer | yes | 1-based page number |
| pageSize | integer | yes | Number of records per page (recommended: 20) |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "mails": [
    {
      "itemId": "mail-id-123",
      "subject": "Welcome to the platform",
      "to": ["user@example.com"],
      "from": "noreply@example.com",
      "body": "<p>Hello...</p>",
      "purpose": "welcome",
      "language": "en",
      "sentTime": "2024-06-01T10:00:00Z",
      "isRead": false
    }
  ],
  "totalCount": 42,
  "isSuccess": true,
  "errors": {}
}
```

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Invalid pagination params or missing projectKey | Inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Use `totalCount` with `pageSize` to calculate the total number of pages for pagination controls.
- To retrieve the full body of a specific email, use `get-mailbox-mail` with the `itemId`.
