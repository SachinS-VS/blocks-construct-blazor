# Action: reload-configuration

## Purpose

Reload the GraphQL schema configuration for a project. Must be called after any schema change (create, update, delete, field changes) to apply those changes to the live GraphQL API. Without this, the API continues serving the previous schema version.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/configurations/reload
```

---

## curl

```bash
curl --location --request POST "$VITE_API_BASE_URL/uds/v1/configurations/reload" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

No request body or path parameters required. The project is identified from the authentication context.

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Configuration reloaded successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | No data source registered for project | Call `add-data-source` first |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 500 | MongoDB connection failed | Check data source connection string via `get-data-source` |

---

## When to Call

Call `reload-configuration` after any of the following:

| Action | Reload Required |
|--------|----------------|
| `define-schema` | Yes |
| `update-schema` | Yes |
| `delete-schema` | Yes |
| `save-schema-info` | Yes |
| `update-schema-info` | Yes |
| `save-schema-fields` | Yes |
| `add-data-source` | Yes |
| `update-data-source` | Yes |
| `change-security` | No |
| `create-access-policy` | No |
| File uploads | No |

---

## Performance Note

This operation triggers a live schema reload on the server. It may take 1–3 seconds. In the frontend, block schema editing UI while this is in progress.
