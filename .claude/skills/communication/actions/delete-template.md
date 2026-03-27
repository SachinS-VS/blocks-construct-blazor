# Action: delete-template

## Purpose

Permanently delete an email template by its ID. This action is irreversible — confirm with the user before calling.

---

## Endpoint

```
DELETE $VITE_API_BASE_URL/communication/v1/Template/Delete
```

---

## curl

```bash
curl --location --request DELETE "$VITE_API_BASE_URL/communication/v1/Template/Delete?itemId=template-id-123&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| itemId | string | yes | ID of the template to delete — from `get-templates` |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

The template is permanently deleted.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "itemId": "Template not found"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Template not found for the given `itemId` | Verify the ID from `get-templates` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- Deletion is permanent and cannot be undone. Always show a confirmation dialog before calling this action in the frontend.
- After successful deletion, invalidate the `['templates']` React Query cache to refresh the list.
- If the template is referenced by active `send-email-with-template` calls, those will fail after deletion. Audit usage before deleting.
