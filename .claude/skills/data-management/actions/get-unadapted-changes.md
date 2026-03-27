# Action: get-unadapted-changes

## Purpose

Get the list of pending schema changes that have been defined but not yet applied to the GraphQL layer. Use this to check whether `reload-configuration` needs to be called.

---

## Endpoint

```
GET $VITE_API_BASE_URL/uds/v1/schemas/unadapted-change-logs
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/schemas/unadapted-change-logs?projectKey=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Notes |
|-----------|------|----------|-------|
| projectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Success",
  "httpStatusCode": 200,
  "data": {
    "changes": [
      {
        "schemaName": "Product",
        "changeType": "FieldAdded",
        "fieldName": "sku",
        "timestamp": "2024-01-02T12:00:00Z"
      }
    ],
    "total": 1
  },
  "errors": {}
}
```

If `data.total` is greater than 0, call `reload-configuration` to apply pending changes.

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |
| 404 | Wrong projectKey or API URL | Verify `$VITE_PROJECT_SLUG` and `$VITE_API_BASE_URL` |
