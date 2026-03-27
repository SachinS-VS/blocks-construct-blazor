# Action: update-access-policy

## Purpose

Update an existing data access policy — modify allowed roles or permitted operations.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/data-access/policy/update
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/data-access/policy/update" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "SchemaName": "Product",
    "PolicyName": "admin-full-access",
    "AllowedRoles": ["admin", "superadmin", "manager"],
    "Operations": ["Read", "Create", "Update", "Delete"],
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

Same shape as `create-access-policy`. The `PolicyName` is used to identify which policy to update.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| SchemaName | string | yes | The schema the policy belongs to |
| PolicyName | string | yes | Name of the existing policy to update |
| AllowedRoles | array | yes | Updated list of role slugs |
| Operations | array | yes | Updated list of permitted operations |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Access policy updated successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | PolicyName not found for schema, or invalid operations | Verify PolicyName from get-access-policies |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
