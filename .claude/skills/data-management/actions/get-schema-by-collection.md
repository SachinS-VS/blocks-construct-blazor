# Action: get-schema-by-collection

## Purpose

Retrieve the details of a specific Entity-type schema by its collection name, including all field definitions. Use this when you know the collection name but not the schema ID.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uds/v1/schemas/info-by-name
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/schemas/info-by-name?schemaName=products&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| schemaName | string | yes | The collection name (e.g. `products`, `orders`) |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Success",
  "httpStatusCode": 200,
  "data": {
    "name": "Product",
    "collectionName": "products",
    "description": "Product catalog",
    "type": "Entity",
    "fields": [
      {
        "name": "title",
        "type": "String",
        "isRequired": true,
        "isArray": false
      },
      {
        "name": "price",
        "type": "Number",
        "isRequired": true,
        "isArray": false
      }
    ]
  },
  "errors": []
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Invalid collection name | Verify the collection name from `get-schema-collections` or `get-schemas` |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Collection not found | Verify the collection exists using `get-schema-collections` |
