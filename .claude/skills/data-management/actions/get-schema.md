# Action: get-schema

## Purpose

Get a single schema by its ID, including all defined fields.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uds/v1/schemas/get-by-id
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/schemas/get-by-id?id=$SCHEMA_ID&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| id | string | yes | Schema ID returned from define-schema or get-schemas |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Success",
  "httpStatusCode": 200,
  "data": {
    "id": "schema-id-1",
    "schemaName": "Product",
    "collectionName": "products",
    "schemaType": "Collection",
    "description": "Product catalog",
    "fields": [
      {
        "name": "title",
        "type": "String",
        "isArray": false,
        "isRequired": true,
        "description": "Product title",
        "defaultValue": null
      },
      {
        "name": "price",
        "type": "Number",
        "isArray": false,
        "isRequired": true,
        "description": "Product price",
        "defaultValue": "0"
      }
    ],
    "projectKey": "my-project",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-02T00:00:00Z"
  },
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Schema not found | Verify schema ID from get-schemas |
