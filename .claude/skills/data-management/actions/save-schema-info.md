# Action: save-schema-info

## Purpose

Save (create) the full set of field definitions for a schema. Call this after `define-schema` to add all fields at once. This replaces any existing fields — to update fields without replacement, use `update-schema-info`.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/schemas/info
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/schemas/info" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "SchemaId": "$SCHEMA_ID",
    "Fields": [
      {
        "Name": "title",
        "Type": "String",
        "IsArray": false,
        "IsRequired": true,
        "Description": "Product title",
        "DefaultValue": ""
      },
      {
        "Name": "price",
        "Type": "Number",
        "IsArray": false,
        "IsRequired": true,
        "Description": "Product price in USD",
        "DefaultValue": "0"
      },
      {
        "Name": "tags",
        "Type": "String",
        "IsArray": true,
        "IsRequired": false,
        "Description": "Product tags",
        "DefaultValue": ""
      }
    ],
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| SchemaId | string | yes | ID returned from `define-schema` |
| Fields | array | yes | Full list of fields — replaces all existing fields |
| Fields[].Name | string | yes | Field name — use camelCase (e.g. `firstName`) |
| Fields[].Type | enum | yes | `String`, `Number`, `Boolean`, `Date`, `ObjectId`, `Object`, `Array` |
| Fields[].IsArray | boolean | yes | Set `true` if the field holds an array of the given type |
| Fields[].IsRequired | boolean | yes | Whether the field must be present on create |
| Fields[].Description | string | no | Optional field description |
| Fields[].DefaultValue | string | no | Default value as a string |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Schema info saved successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Invalid SchemaId, missing required fields, or invalid field type | Check request body |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Schema not found | Verify SchemaId from define-schema or get-schemas |

---

## Next Steps

After saving fields, call `reload-configuration` to apply GraphQL changes. Optionally call `create-validation` to add field-level validation rules.
