# Action: add-data-source

## Purpose

Register a new MongoDB database connection for a project. This must be done before defining any schemas — UDS uses this connection to store and query data.

---

## Endpoint

```
POST $VITE_API_BASE_URL/uds/v1/data-sources/add
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/uds/v1/data-sources/add" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "ItemId": "my-project-db",
    "ConnectionString": "mongodb+srv://user:password@cluster.mongodb.net",
    "DatabaseName": "my_database",
    "ProjectKey": "$VITE_PROJECT_SLUG"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| ItemId | string | yes | Unique identifier for this connection — use a slug (e.g. `my-project-db`) |
| ConnectionString | string | yes | Full MongoDB connection string including credentials |
| DatabaseName | string | yes | Name of the MongoDB database to use |
| ProjectKey | string | yes | `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "isSuccess": true,
  "message": "Data source added successfully",
  "httpStatusCode": 200,
  "data": null,
  "errors": {}
}
```

---

## On Failure

| Status | Cause | Action |
|--------|-------|--------|
| 400 | Missing required fields or duplicate ItemId | Check request body and ensure ItemId is unique |
| 401 | Invalid or expired token | Run get-token to refresh |
| 403 | Missing `cloudadmin` role | Check user role in Cloud Portal → People |

---

## Security Note

The `ConnectionString` contains credentials. Never log or expose it. It is stored encrypted by UDS and is never returned in GET responses.

---

## Next Steps

After adding a data source, call `reload-configuration` to confirm connectivity, then proceed with `define-schema`.
