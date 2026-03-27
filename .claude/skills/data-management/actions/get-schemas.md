# Action: get-schemas

## Purpose

List all schemas for a project with optional pagination and search filtering.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uds/v1/schemas
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/schemas?page=1&pageSize=20&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

With search:

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/schemas?page=1&pageSize=20&search=product&projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| page | integer | no | Default: 1 |
| pageSize | integer | no | Default: 20 |
| search | string | no | Filter schemas by name |
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

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
        "schemaType": "Collection",
        "description": "Product catalog",
        "projectKey": "my-project",
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 20
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
| 404 | Wrong `VITE_API_BASE_URL` | Verify environment URL |
