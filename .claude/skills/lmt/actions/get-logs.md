# Action: get-logs

## Purpose

Retrieve paginated service logs with optional filtering by log level, date range, trace ID, and full-text search.

---

## Endpoint

```
POST $VITE_API_BASE_URL/lmt/v1/Log/GetLogs
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/lmt/v1/Log/GetLogs" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "serviceName": "identity-service",
    "page": 1,
    "pageSize": 50,
    "sort": { "property": "timestamp", "isDescending": true },
    "filter": {
      "logLevel": "Error",
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": "2024-01-31T23:59:59Z"
    },
    "search": "",
    "projectKey": "'"$VITE_X_BLOCKS_KEY"'"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| serviceName | string | yes | SELISE Blocks service name (e.g. `identity-service`) |
| page | number | no | Default: 1 |
| pageSize | number | no | Default: 50, max: 200 |
| sort.property | string | no | `timestamp`, `level`, `serviceName` |
| sort.isDescending | boolean | no | Default: true (newest first) |
| filter.logLevel | string | no | `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical` |
| filter.startDate | ISO datetime | no | Filter start boundary |
| filter.endDate | ISO datetime | no | Filter end boundary |
| filter.traceId | string | no | Correlate logs to a distributed trace |
| filter.spanId | string | no | Narrow to a specific span |
| search | string | no | Full-text search in log message |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

```json
{
  "data": [
    {
      "timestamp": "2024-01-15T10:30:00Z",
      "level": "Error",
      "message": "Unhandled exception in UserService",
      "serviceName": "identity-service",
      "traceId": "abc123",
      "spanId": "def456",
      "properties": {}
    }
  ],
  "errors": {},
  "totalCount": 500
}
```

---

## On Failure

* 400 — missing `serviceName` or invalid filter values
* 401 — invalid or expired token
* 403 — insufficient permissions
