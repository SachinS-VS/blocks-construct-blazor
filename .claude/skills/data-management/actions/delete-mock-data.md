# Action: delete-mock-data

## Purpose

Delete mock / test data for a specific schema. Use this to clean up seeded test records before going to production or when resetting a development environment.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/data-manage/mock-data
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/data-manage/mock-data" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "ProjectKey": "$VITE_PROJECT_SLUG",
    "SchemaName": "Product"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |
| SchemaName | string | yes | Schema whose mock data should be deleted |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Mock data deleted successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Missing SchemaName or schema not found | Verify SchemaName from get-schemas |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |

---

## Warning

This deletes all mock records for the specified schema. This is a destructive operation. Only run this in development environments. Always use `get-mock-data` first to confirm what will be deleted.
