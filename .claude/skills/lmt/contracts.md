# LMT Contracts

## Common Headers (all authenticated requests)

```
Authorization: Bearer $ACCESS_TOKEN
x-blocks-key: $VITE_X_BLOCKS_KEY
Content-Type: application/json
```

---

## Common Response

```json
{
  "data": [],
  "errors": {},
  "totalCount": 100
}
```

---

## Logs

### GetLogsRequest

```json
{
  "serviceName": "identity-service",
  "page": 1,
  "pageSize": 50,
  "sort": {
    "property": "timestamp",
    "isDescending": true
  },
  "filter": {
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "logLevel": "Error | Warning | Information | Debug",
    "traceId": "string",
    "spanId": "string"
  },
  "search": "string",
  "projectKey": "string"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| serviceName | string | yes | Name of the SELISE Blocks service to query |
| page | number | no | Default: 1 |
| pageSize | number | no | Default: 50, max: 200 |
| filter.logLevel | string | no | `Error`, `Warning`, `Information`, `Debug`, `Trace` |
| filter.startDate | ISO date | no | Filters by timestamp |
| filter.endDate | ISO date | no | Filters by timestamp |
| filter.traceId | string | no | Correlates logs to a distributed trace |
| search | string | no | Full-text search across log message |

### GetLogsResponse

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

### GetLogsByDateRequest

```json
{
  "serviceName": "identity-service",
  "filter": {
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "logLevel": "Error"
  },
  "page": 1,
  "pageSize": 50,
  "sort": {
    "property": "timestamp",
    "isDescending": true
  },
  "search": "string",
  "projectKey": "string"
}
```

---

## Traces

### GetTracesRequest

```json
{
  "filter": {
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "services": ["identity-service", "communication-service"],
    "statusCodes": [200, 400, 500]
  },
  "page": 1,
  "pageSize": 50,
  "sort": {
    "property": "startTime",
    "isDescending": true
  },
  "search": "string",
  "projectKey": "string"
}
```

### GetTracesResponse

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

### GetTraceResponse (single trace)

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
      "tags": {},
      "logs": []
    }
  ]
}
```

### GetApiAnalyticsRequest

```json
{
  "startTime": "2024-01-01T00:00:00Z",
  "endTime": "2024-01-31T23:59:59Z",
  "serviceName": "identity-service",
  "operationName": "POST /Authentication/Token",
  "projectKey": "string"
}
```

`operationName` is optional — omit to get metrics for all operations in the service.

### GetApiAnalyticsResponse

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
    }
  ]
}
```

### GetHttpStatusAnalyticsRequest

```json
{
  "startTime": "2024-01-01T00:00:00Z",
  "endTime": "2024-01-31T23:59:59Z",
  "serviceName": "identity-service",
  "projectKey": "string"
}
```

### GetHttpStatusAnalyticsResponse

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

---

## Enumerations

| Enum | Values |
|------|--------|
| LogLevel | `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical` |
| SortProperty (Logs) | `timestamp`, `level`, `serviceName` |
| SortProperty (Traces) | `startTime`, `duration`, `statusCode` |
