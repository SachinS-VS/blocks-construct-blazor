claude# Action: get-schemas-aggregation

## Purpose

Retrieve a paginated list of schema definitions along with an aggregation summary of access levels (Public, User, Custom) for Read, Write, Edit, and Delete operations. Use this when you need both the schema list and a security overview in a single call.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uds/v1/schemas/aggregation
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/schemas/aggregation?ProjectKey=$VITE_PROJECT_SLUG&PageNo=1&PageSize=20" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| PageNo | integer | no | Default: 1 |
| PageSize | integer | no | Default: 20 |
| Keyword | string | no | General search keyword |
| SchemaName | string | no | Filter by schema name |
| CollectionName | string | no | Filter by collection name |
| SchemaType | enum | no | `Entity` or `Dto` |
| SortBy | string | no | Field name to sort by |
| SortDescending | boolean | no | Sort direction — `true` for descending |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Success",
  "httpStatusCode": 200,
  "data": {
    "items": [
      {
        "id": "schema-id-1",
        "schemaName": "Product",
        "collectionName": "products",
        "schemaType": "Entity",
        "description": "Product catalog",
        "accessSummary": {
          "read": "Public",
          "write": "Custom",
          "edit": "Custom",
          "delete": "User"
        }
      }
    ],
    "totalCount": 1
  },
  "errors": []
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
