# Action: get-notifications

## Purpose

Retrieve all notifications (read and unread) for the current user with pagination. Use this to populate the full notification list panel.

---

## Endpoint

```
GET $VITE_API_BASE_URL/communication/v1/Notifier/GetNotifications
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Notifier/GetNotifications?page=1&pageSize=20&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| page | integer | yes | 1-based page number |
| pageSize | integer | yes | Records per page (recommended: 20) |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "notifications": [
    {
      "id": "notif-id-123",
      "payload": "string",
      "denormalizedPayload": "{\"message\": \"Your order has been shipped.\"}",
      "createdTime": "2024-06-01T10:00:00Z",
      "isRead": false,
      "subscriptionFilter": "order-updates"
    },
    {
      "id": "notif-id-456",
      "payload": "string",
      "denormalizedPayload": "{\"message\": \"New user registered.\"}",
      "createdTime": "2024-05-30T08:00:00Z",
      "isRead": true,
      "subscriptionFilter": "user-events"
    }
  ],
  "unReadNotificationsCount": 1,
  "totalNotificationsCount": 47,
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

- `totalNotificationsCount` should be used with `pageSize` to calculate the number of pages.
- `unReadNotificationsCount` in this response is the total unread count across all pages — use this to sync the bell badge.
- To get only unread notifications for a specific filter, use `get-unread-notifications` instead.
- Parse `denormalizedPayload` as JSON in the frontend to extract structured notification data.
