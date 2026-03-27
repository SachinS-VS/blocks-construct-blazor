# Flow: notification-flow

## Trigger

User wants to send, display, or manage in-app notifications for users.

> "send a notification to users"
> "notify all admins"
> "push a bell notification"
> "show unread notification count"
> "mark notifications as read"
> "build a notification bell"
> "build a notification panel"

---

## Pre-flight Questions

Before starting, confirm:

1. Are you sending a notification, displaying notifications, or building the full notification bell UI?
2. Who should receive the notification — specific user IDs, users with a certain role, or users subscribed to a filter key?
3. What is the notification payload format — plain text, or a JSON string with structured data?
4. What is the `subscriptionFilter` key that scopes notifications for this feature area?
5. Should the notification bell poll for updates, or use WebSocket?

---

## Flow Steps

### Step 1 — Send a Notification

```
Action: send-notification
Input:
  userIds             = ["user-id-abc"]        (pick one or more targeting methods)
  roles               = ["admin"]
  subscriptionFilters = ["order-updates"]
  denormalizedPayload = '{"message": "Your order has been shipped.", "orderId": "ORD-001"}'
  configuratoinName   = ""                      (API typo — leave as-is)
  projectKey          = $VITE_PROJECT_SLUG
```

Use at least one of `userIds`, `roles`, or `subscriptionFilters` per request.

On `isSuccess: true` → notification is delivered to target users' inboxes.

---

### Step 2 — Display Unread Count (Notification Bell)

The `NotificationBell` component polls for the unread count every 30 seconds.

```
Action: get-unread-notifications
Input:
  subscriptionFilter = "order-updates"
  projectKey         = $VITE_PROJECT_SLUG
Output:
  unReadNotificationsCount → drive badge on NotificationBell
```

Show a `<BellDot>` icon with a badge when `unReadNotificationsCount > 0`.
Show a plain `<Bell>` icon when the count is 0.

---

### Step 3 — Open Notification Panel

When user clicks the bell icon, open the notification list panel.

```
Action: get-notifications
Input:
  page       = 1
  pageSize   = 20
  projectKey = $VITE_PROJECT_SLUG
Output:
  notifications[]          → render as list items
  totalNotificationsCount  → use for load-more / pagination
  unReadNotificationsCount → sync badge count
```

Show each notification as a row with:
- Parsed `denormalizedPayload` as content
- `createdTime` formatted as relative time (e.g. "2 hours ago")
- Visual distinction between read (`isRead: true`) and unread (`isRead: false`) items

---

### Step 4a — Mark Single Notification as Read

When user clicks a notification item:

```
Action: mark-notification-read
Input:
  notificationId = notification.id
  projectKey     = $VITE_PROJECT_SLUG
```

After success, invalidate `['notifications']` React Query cache.
The bell badge count updates automatically on next poll or cache refresh.

---

### Step 4b — Mark All Notifications as Read

When user clicks "Mark all as read":

```
Action: mark-all-notifications-read
Input:
  subscriptionFilter = "order-updates"
  projectKey         = $VITE_PROJECT_SLUG
```

After success, invalidate `['notifications']` React Query cache.
Badge count drops to 0.

---

### Step 5 — Confirm

After marking as read:

> Notifications updated. Unread count and notification list will refresh automatically.

---

## Targeting Decision Guide

| Scenario | Use |
|----------|-----|
| Notify a specific user | `userIds: ["user-id"]` |
| Notify all users with a role | `roles: ["admin"]` |
| Notify all subscribers of a channel | `subscriptionFilters: ["order-updates"]` |
| Broadcast to multiple groups | Combine two or more fields |

---

## Real-time Strategy

| Strategy | When to use |
|----------|-------------|
| Poll every 30 seconds | Default — sufficient for most apps |
| WebSocket | High-frequency notifications — implement as a layer over the existing hook API |

React Query's `refetchInterval: 30_000` on `useGetUnreadNotifications` handles polling. No additional setup required.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| `isSuccess: false` with no valid recipients | User IDs / roles / filters have no matching users | Verify recipient targeting values |
| `isSuccess: false` on mark-as-read | Notification ID not found or already read | Ignore silently or log — non-critical |
| `401` | `ACCESS_TOKEN` expired | Re-run `get-token` |
| `403` | Missing `cloudadmin` role | Verify role in Cloud Portal → People |
| `404` | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |
| Poll returning stale data | Network error during polling | React Query retries automatically; show last known count |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/communication/components/notification-bell/notification-bell.tsx` | Bell icon with unread badge; polls every 30 seconds |
| `modules/communication/components/notification-list/notification-list.tsx` | Dropdown/panel listing paginated notifications |
| `modules/communication/components/notification-list/notification-item.tsx` | Single notification row with read/unread state |
| `modules/communication/hooks/use-communication.tsx` | `useGetUnreadNotifications`, `useGetNotifications`, `useMarkNotificationRead`, `useMarkAllNotificationsRead`, `useSendNotification` hooks |
| `modules/communication/services/communication.service.ts` | `sendNotification()`, `getUnreadNotifications()`, `getNotifications()`, `markNotificationRead()`, `markAllNotificationsRead()` |
| `modules/communication/types/communication.type.ts` | `Notification`, `NotifyPayload` interfaces |
