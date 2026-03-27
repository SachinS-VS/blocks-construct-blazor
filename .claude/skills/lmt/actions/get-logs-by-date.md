# Action: get-logs-by-date

## Purpose

Retrieve service logs filtered by a specific date range. Optimized for date-bounded queries compared to get-logs.

---

## Endpoint

```
POST $VITE_API_BASE_URL/lmt/v1/Log/GetLogsByDate
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/lmt/v1/Log/GetLogsByDate" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "serviceName": "identity-service",
    "filter": {
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": "2024-01-31T23:59:59Z",
      "logLevel": "Error"
    },
    "page": 1,
    "pageSize": 50,
    "sort": { "property": "timestamp", "isDescending": true },
    "search": "",
    "projectKey": "'"$VITE_X_BLOCKS_KEY"'"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| serviceName | string | yes | SELISE Blocks service name |
| filter.startDate | ISO datetime | yes | Start of date range |
| filter.endDate | ISO datetime | yes | End of date range |
| filter.logLevel | string | no | `Error`, `Warning`, `Information`, `Debug` |
| page | number | no | Default: 1 |
| pageSize | number | no | Default: 50 |
| sort.property | string | no | `timestamp`, `level` |
| sort.isDescending | boolean | no | Default: true |
| search | string | no | Full-text search |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

Same structure as `get-logs` — returns `{ data, errors, totalCount }`.

---

## On Failure

* 400 — missing `startDate` or `endDate`
* 401 — invalid or expired token
