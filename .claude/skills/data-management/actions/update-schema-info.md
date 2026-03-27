# Action: update-schema-info

## Purpose

Update the field definitions for an existing schema. Use this to modify the field list of an already-defined schema. Replaces the current field set with the provided list.

---

## Endpoint

```
PUT $VITE_API_BASE_URL/uds/v1/schemas/info
```

---

## curl

```bash
curl --location --request PUT "$VITE_API_BASE_URL/uds/v1/schemas/info" \
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
        "Name": "inStock",
        "Type": "Boolean",
        "IsArray": false,
        "IsRequired": false,
        "Description": "Whether the product is in stock",
        "DefaultValue": "true"
      }
    ],
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

Same shape as `save-schema-info` (POST). The full Fields array replaces the existing field list.

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| SchemaId | string | yes | ID of the schema to update |
| Fields | array | yes | Complete updated field list |
| Fields[].Name | string | yes | Field name — use camelCase |
| Fields[].Type | enum | yes | `String`, `Number`, `Boolean`, `Date`, `ObjectId`, `Object`, `Array` |
| Fields[].IsArray | boolean | yes | Whether the field is an array |
| Fields[].IsRequired | boolean | yes | Whether the field is required |
| Fields[].Description | string | no | Field description |
| Fields[].DefaultValue | string | no | Default value as a string |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Schema info updated successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Invalid SchemaId or invalid field definitions | Check request body |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Schema not found | Verify SchemaId from get-schemas |

---

## Next Steps

After updating fields, call `reload-configuration` to apply GraphQL changes.
