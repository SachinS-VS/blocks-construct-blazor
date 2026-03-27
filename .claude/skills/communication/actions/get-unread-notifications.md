# Action: get-unread-notifications

## Purpose

Retrieve all unread notifications for the current user scoped to a specific subscription filter. This is the primary endpoint used to power the notification bell badge count.

---

## Endpoint

```
GET $VITE_API_BASE_URL/communication/v1/Notifier/GetUnreadNotificationsBySubscriptionFilter
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Notifier/GetUnreadNotificationsBySubscriptionFilter?subscriptionFilter=order-updates&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| subscriptionFilter | string | yes | Filter key to scope results — must match the filter used when sending notifications |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "notifications": [
    {
      "id": "notif-id-123",
      "payload": "string",
      "denormalizedPayload": "{\"message\": \"Your order has been shipped.\", \"orderId\": \"ORD-001\"}",
      "createdTime": "2024-06-01T10:00:00Z",
      "isRead": false,
      "subscriptionFilter": "order-updates"
    }
  ],
  "unReadNotificationsCount": 3,
  "isSuccess": true,
  "errors": {}
}
```

---

## On Failure

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Missing or invalid `subscriptionFilter` | Inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Poll this endpoint every 30 seconds to keep the notification badge count up-to-date.
- Use `unReadNotificationsCount` to drive the badge number on `NotificationBell`.
- To get all notifications (read and unread) with pagination, use `get-notifications` instead.
