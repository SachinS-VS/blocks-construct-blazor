# Action: get-traces

## Purpose

Retrieve paginated distributed traces with optional filtering by date range, services, and HTTP status codes.

---

## Endpoint

```
POST $VITE_API_BASE_URL/lmt/v1/Trace/GetTraces
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/lmt/v1/Trace/GetTraces" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "filter": {
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": "2024-01-31T23:59:59Z",
      "services": ["identity-service"],
      "statusCodes": [500]
    },
    "page": 1,
    "pageSize": 50,
    "sort": { "property": "startTime", "isDescending": true },
    "projectKey": "'"$VITE_X_BLOCKS_KEY"'"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| filter.startDate | ISO datetime | no | Start of date range |
| filter.endDate | ISO datetime | no | End of date range |
| filter.services | string[] | no | Filter by service names |
| filter.statusCodes | number[] | no | e.g. `[400, 500]` for errors |
| page | number | no | Default: 1 |
| pageSize | number | no | Default: 50 |
| sort.property | string | no | `startTime`, `duration`, `statusCode` |
| sort.isDescending | boolean | no | Default: true |
| search | string | no | Search in operation name |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

```json
{
  "data": [
    {
      "traceId": "abc123xyz",
      "rootSpan": {
        "spanId": "span001",
        "operationName": "POST /Authentication/Token",
        "serviceName": "identity-service",
        "startTime": "2024-01-15T10:30:00Z",
        "duration": 145,
        "statusCode": 200
      },
      "spanCount": 5,
      "totalDuration": 145
    }
  ],
  "errors": {},
  "totalCount": 1200
}
```

---

## On Failure

* 400 — invalid filter values
* 401 — invalid or expired token
