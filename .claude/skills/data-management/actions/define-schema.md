# Action: define-schema

## Purpose

Create a new schema (collection or single object) for the project. This registers the schema in UDS but does not add fields yet — use `save-schema-info` or `save-schema-fields` after this step.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/schemas/define
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/schemas/define" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "CollectionName": "products",
    "SchemaName": "Product",
    "ProjectKey": "$VITE_PROJECT_SLUG",
    "SchemaType": "Collection",
    "Description": "Product catalog schema"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| CollectionName | string | yes | MongoDB collection name — use lowercase, no spaces (e.g. `products`) |
| SchemaName | string | yes | Human-readable display name (e.g. `Product`) |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| SchemaType | enum | yes | `Collection` for multi-document, `SingleObject` for single config document |
| Description | string | no | Optional description |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Schema defined successfully",
  "httpStatusCode": 200,
  "data": {
    "id": "64a1b2c3d4e5f60001234567",
    "schemaName": "Product",
    "collectionName": "products",
    "schemaType": "Collection",
    "projectKey": "my-project"
  },
  "errors": {}
}
```

Store `data.id` as `$SCHEMA_ID` — required for subsequent `save-schema-info` and `save-schema-fields` calls.

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Missing required fields or duplicate CollectionName | Check request body and ensure CollectionName is unique |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |

---

## Next Steps

After `define-schema` succeeds:
1. Call `save-schema-info` to add all fields at once, OR
2. Call `save-schema-fields` repeatedly to add individual fields
3. Call `reload-configuration` to apply GraphQL schema changes
