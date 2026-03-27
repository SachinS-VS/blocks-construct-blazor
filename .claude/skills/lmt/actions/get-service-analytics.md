# Action: get-service-analytics

## Purpose

Get HTTP status code distribution for a service — useful for monitoring error rates and health over time.

---

## Endpoint

```
POST $VITE_API_BASE_URL/lmt/v1/Trace/GetServiceAnalytics
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/lmt/v1/Trace/GetServiceAnalytics" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "startTime": "2024-01-01T00:00:00Z",
    "endTime": "2024-01-31T23:59:59Z",
    "serviceName": "identity-service",
    "projectKey": "'"$VITE_X_BLOCKS_KEY"'"
  }'
```

---

## Request Body

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| startTime | ISO datetime | yes | Analytics window start |
| endTime | ISO datetime | yes | Analytics window end |
| serviceName | string | no | Omit for all services |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

```json
{
  "distribution": [
    { "statusCode": 200, "count": 50000, "percentage": 94.5 },
    { "statusCode": 400, "count": 2100, "percentage": 3.97 },
    { "statusCode": 401, "count": 600, "percentage": 1.13 },
    { "statusCode": 500, "count": 280, "percentage": 0.53 }
  ],
  "totalRequests": 52980
}
```

Use `percentage` to compute overall health score. A healthy service should have 200 > 95% and 500 < 0.5%.

---

## On Failure

* 400 — missing `startTime` or `endTime`
* 401 — invalid or expired token
