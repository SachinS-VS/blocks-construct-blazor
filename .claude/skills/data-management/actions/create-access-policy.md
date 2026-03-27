# Action: create-access-policy

## Purpose

Create a data access policy for a schema. Policies define which roles can perform which operations (Read, Create, Update, Delete) on a schema's data. Only applies when the schema's SecurityType is `RoleBased`.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/data-access/policy/create
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/data-access/policy/create" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "SchemaName": "Product",
    "PolicyName": "admin-full-access",
    "AllowedRoles": ["admin", "superadmin"],
    "Operations": ["Read", "Create", "Update", "Delete"],
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| SchemaName | string | yes | The schema this policy applies to |
| PolicyName | string | yes | Unique name for this policy — use kebab-case |
| AllowedRoles | array | yes | Role slugs that this policy grants access to |
| Operations | array | yes | One or more of: `Read`, `Create`, `Update`, `Delete` |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Access policy created successfully",
  "httpStatusCode": 200,
  "data": {
    "itemId": "policy-id-abc123",
    "schemaName": "Product",
    "policyName": "admin-full-access",
    "allowedRoles": ["admin", "superadmin"],
    "operations": ["Read", "Create", "Update", "Delete"]
  },
  "errors": {}
}
```

Store `data.itemId` if you need to update or delete this policy later.

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Duplicate PolicyName, missing fields, or invalid operations | Ensure PolicyName is unique per schema |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |

---

## Common Policy Patterns

**Read-only for viewers:**
```json
{
  "PolicyName": "viewer-read-only",
  "AllowedRoles": ["viewer"],
  "Operations": ["Read"]
}
```

**Write access for editors:**
```json
{
  "PolicyName": "editor-write",
  "AllowedRoles": ["editor"],
  "Operations": ["Read", "Create", "Update"]
}
```

**Full access for admins:**
```json
{
  "PolicyName": "admin-full-access",
  "AllowedRoles": ["admin"],
  "Operations": ["Read", "Create", "Update", "Delete"]
}
```
