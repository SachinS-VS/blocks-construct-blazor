# Action: update-schema

## Purpose

Update an existing schema's definition — rename the schema, change the collection name, or update the description. Does not modify fields (use `save-schema-info` or `save-schema-fields` for that).

---

## Endpoint

```
PUT $VITE_API_BASE_URL/uds/v1/schemas/define
```

---

## curl

```bash
curl --location --request PUT "$VITE_API_BASE_URL/uds/v1/schemas/define" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "CollectionName": "products",
    "SchemaName": "Product",
    "ProjectKey": "$VITE_PROJECT_SLUG",
    "SchemaType": "Collection",
    "Description": "Updated product catalog schema"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| CollectionName | string | yes | MongoDB collection name — must match the existing collection |
| SchemaName | string | yes | Updated display name |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| SchemaType | enum | yes | `Collection` or `SingleObject` — changing type is not recommended |
| Description | string | no | Updated description |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Schema updated successfully",
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

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Schema not found or invalid CollectionName | Verify CollectionName matches an existing schema |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |

---

## Next Steps

After `update-schema` succeeds, call `reload-configuration` to apply GraphQL schema changes.
