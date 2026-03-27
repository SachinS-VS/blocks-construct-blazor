# Action: save-schema-fields

## Purpose

Add or update a single field on a schema without replacing the entire field list. Use this for incremental schema changes when you want to add one field at a time.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/schemas/fields
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/schemas/fields" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "SchemaId": "$SCHEMA_ID",
    "FieldName": "sku",
    "FieldType": "String",
    "IsArray": false,
    "IsRequired": true,
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| SchemaId | string | yes | Schema ID returned from define-schema or get-schemas |
| FieldName | string | yes | Field name to add or update — use camelCase |
| FieldType | enum | yes | `String`, `Number`, `Boolean`, `Date`, `ObjectId`, `Object`, `Array` |
| IsArray | boolean | yes | Set `true` if this field holds an array of the given type |
| IsRequired | boolean | yes | Whether the field must be present on document create |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Schema field saved successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Invalid SchemaId, missing FieldName, or unsupported FieldType | Check request body values |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Schema not found | Verify SchemaId from get-schemas |

---

## Next Steps

After adding fields, call `reload-configuration` to apply GraphQL changes.
