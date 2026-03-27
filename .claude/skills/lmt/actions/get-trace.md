# Action: get-trace

## Purpose

Retrieve a single distributed trace by its trace ID, including all spans in the call tree.

---

## Endpoint

```
GET $VITE_API_BASE_URL/lmt/v1/Trace/GetTrace?traceId={traceId}&projectKey={projectKey}
```

---

## curl

```bash
curl --location \
  "$VITE_API_BASE_URL/lmt/v1/Trace/GetTrace?traceId=abc123xyz&projectKey=$VITE_X_BLOCKS_KEY" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| traceId | string | yes | The distributed trace ID |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## On Success (200)

```json
{
  "traceId": "abc123xyz",
  "spans": [
    {
      "spanId": "span001",
      "parentSpanId": null,
      "operationName": "POST /Authentication/Token",
      "serviceName": "identity-service",
      "startTime": "2024-01-15T10:30:00Z",
      "endTime": "2024-01-15T10:30:00.145Z",
      "duration": 145,
      "statusCode": 200,
      "tags": { "http.method": "POST", "http.url": "/Authentication/Token" },
      "logs": []
    },
    {
      "spanId": "span002",
      "parentSpanId": "span001",
      "operationName": "DB:users.find",
      "serviceName": "identity-service",
      "startTime": "2024-01-15T10:30:00.020Z",
      "endTime": "2024-01-15T10:30:00.085Z",
      "duration": 65,
      "statusCode": 200,
      "tags": {},
      "logs": []
    }
  ]
}
```

---

## On Failure

* 404 — trace ID not found or expired
* 401 — invalid or expired token
