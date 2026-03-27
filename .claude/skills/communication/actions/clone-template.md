# Action: clone-template

## Purpose

Create a copy of an existing email template under a new name. The cloned template inherits all fields from the source (subject, body, language, purpose) and can be edited independently.

---

## Endpoint

```
POST $VITE_API_BASE_URL/communication/v1/Template/Clone
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/communication/v1/Template/Clone" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "itemId": "template-id-123",
    "newName": "Welcome Email (Copy)",
    "projectKey": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Fields

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| itemId | string | yes | ID of the source template to clone — from `get-templates` |
| newName | string | yes | Name for the cloned template — must be unique within the project |
| projectKey | string | yes | Use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "errors": {}
}
```

The cloned template is created. Use `get-templates` to find the new template's `itemId`.

---

## On Failure

```json
{
  "isSuccess": false,
  "errors": {
    "itemId": "Source template not found",
    "newName": "A template with this name already exists"
  }
}
```

| HTTP Status | Cause | Action |
|-------------|-------|--------|
| 200 with `isSuccess: false` | Source template not found, or `newName` already taken | Inspect `errors` |
| 401 | Missing or expired `ACCESS_TOKEN` | Re-run `get-token` |
| 403 | Account lacks permission | Verify `cloudadmin` role in Cloud Portal |
| 404 | Wrong `VITE_API_BASE_URL` | Check environment URL in Cloud Portal |

---

## Notes

- The cloned template's `purpose` is copied from the source. If you need a different `purpose`, update it after cloning using `save-template`.
- After cloning, call `get-templates` to retrieve the new `itemId` and navigate the user to the editor.
