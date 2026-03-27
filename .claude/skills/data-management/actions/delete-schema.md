# Action: delete-schema

## Purpose

Permanently delete a schema and all its field definitions by ID. This does not delete the underlying MongoDB collection or its documents.

---

## Endpoint

```
DELETE $VITE_API_BASE_URL/uds/v1/schemas
```

---

## curl

```bash
curl --location --request DELETE "$VITE_API_BASE_URL/uds/v1/schemas?id=$SCHEMA_ID&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| id | string | yes | Schema ID to delete |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Schema deleted successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

After deletion, call `reload-configuration` to apply GraphQL schema changes.

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Schema not found | Verify schema ID from get-schemas |

---

## Warning

Deleting a schema removes the schema definition but does NOT drop the MongoDB collection. Existing data remains in the database until manually removed. Access policies and validation rules linked to this schema should also be cleaned up.
