# Action: mark-notification-read

## Purpose

Mark a single notification as read by its ID. Call this when a user clicks on a specific notification in the panel.

---

## Endpoint

```
POST $VITE_API_BASE_URL/communication/v1/Notifier/MarkNotificationAsRead
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Notifier/MarkNotificationAsRead" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "notificationId": "notif-id-123",
    "projectKey": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| notificationId | string | yes | ID of the notification to mark as read — from `get-notifications` or `get-unread-notifications` |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

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
    "notificationId": "Notification not found"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Notification ID not found, or already read | Inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- After a successful call, invalidate `['notifications']` in React Query to update both the list and the badge count.
- For bulk mark-as-read, use `mark-all-notifications-read` instead.
