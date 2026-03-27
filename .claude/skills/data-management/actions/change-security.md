# Action: change-security

## Purpose

Set the security type for a schema. This controls who can access the schema's data through the GraphQL API.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/data-access/security/change
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/data-access/security/change" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "SchemaName": "Product",
    "SecurityType": "RoleBased",
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| SchemaName | string | yes | The schema's `SchemaName` value — not the ID |
| SecurityType | enum | yes | `Public`, `Private`, or `RoleBased` |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

### SecurityType Values

| Value | Description |
|-------|-------------|
| `Public` | Anyone can read/write without authentication |
| `Private` | Only authenticated users can access |
| `RoleBased` | Access is controlled by access policies — use with `create-access-policy` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Security changed successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Schema not found or invalid SecurityType | Verify SchemaName and SecurityType |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |

---

## Next Steps

If SecurityType is `RoleBased`, call `create-access-policy` to define which roles can perform which operations. Without policies, no role (including `cloudadmin`) will have access via the data API.
