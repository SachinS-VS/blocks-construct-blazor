# Action: get-operational-analytics

## Purpose

Get API-level performance metrics for a service: call count, average/p95/p99 latency, and error rate per operation.

---

## Endpoint

```
POST $VITE_API_BASE_URL/lmt/v1/Trace/GetOperationalAnalytics
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/lmt/v1/Trace/GetOperationalAnalytics" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "startTime": "2024-01-01T00:00:00Z",
    "endTime": "2024-01-31T23:59:59Z",
    "serviceName": "identity-service",
    "operationName": "",
    "projectKey": "'"$VITE_X_BLOCKS_KEY"'"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| startTime | ISO datetime | yes | Analytics window start |
| endTime | ISO datetime | yes | Analytics window end |
| serviceName | string | yes | Service to analyze |
| operationName | string | no | Narrow to one endpoint; omit for all |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

```json
{
  "operations": [
    {
      "operationName": "POST /Authentication/Token",
      "callCount": 5420,
      "avgDurationMs": 87,
      "p95DurationMs": 210,
      "p99DurationMs": 480,
      "errorRate": 0.02
    },
    {
      "operationName": "GET /User/GetUsers",
      "callCount": 1230,
      "avgDurationMs": 45,
      "p95DurationMs": 120,
      "p99DurationMs": 280,
      "errorRate": 0.0
    }
  ]
}
```

---

## On Failure

* 400 — missing `startTime`, `endTime`, or `serviceName`
* 401 — invalid or expired token
