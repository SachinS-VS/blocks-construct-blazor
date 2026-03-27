# Flow: view-traces-flow

## Trigger

Developer wants to inspect distributed traces to understand request flows and identify performance bottlenecks.

> "show me slow requests"
> "trace a failed API call"
> "check API performance"
> "why is this endpoint slow"
> "view service analytics"

---

## Pre-flight Questions

1. Which service are you investigating?
2. What time range? (last hour / today / custom)
3. Are you looking for specific status codes? (e.g. 500 errors only)
4. Do you want per-operation latency stats or the HTTP status breakdown?

---

## Flow Steps

### Step 1 — Check Service Health (optional)

Get a quick health overview before drilling in:

```
Action: get-service-analytics
Input:
  startTime  = start of time window
  endTime    = end of time window
  serviceName = chosen service
  projectKey = VITE_X_BLOCKS_KEY
```

Review status code distribution. If 500 percentage is high, filter traces for status 500 in next step.

---

### Step 2 — List Traces

```
Action: get-traces
Input:
  filter.startDate / endDate
  filter.services  = [serviceName]
  filter.statusCodes = [500] (optional — filter for errors)
  sort.property    = "duration"
  sort.isDescending = true
  pageSize = 50
  projectKey = VITE_X_BLOCKS_KEY
```

Sort by `duration` descending to surface slowest requests first.

---

### Step 3 — Drill Into a Single Trace

Click a trace to view the full span tree:

```
Action: get-trace
Input:
  traceId = selected trace's traceId
  projectKey = VITE_X_BLOCKS_KEY
```

Review parent→child span relationships to identify:
- Which service or DB query is the bottleneck
- Which span caused the error

---

### Step 4 — Operational Analytics (optional)

For aggregate latency stats across all endpoints:

```
Action: get-operational-analytics
Input:
  startTime  = start of window
  endTime    = end of window
  serviceName = chosen service
  projectKey = VITE_X_BLOCKS_KEY
```

Review p95 and p99 latencies. Any endpoint with p99 > 1000ms warrants investigation.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| Empty trace list | No traces in time window | Widen date range or check service name |
| 404 on get-trace | Trace expired | Traces have a retention window — check recently |
| High p99 latency | DB query or downstream call is slow | Drill into trace spans for that operation |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/lmt/pages/traces/traces-page.tsx` | Trace list with filter panel |
| `modules/lmt/components/trace-list/trace-list.tsx` | Paginated trace rows |
| `modules/lmt/components/trace-detail/trace-detail.tsx` | Span waterfall / call tree view |
| `modules/lmt/pages/analytics/analytics-page.tsx` | Charts for status codes and latency |
| `modules/lmt/hooks/use-lmt.tsx` | `useGetTraces`, `useGetTrace`, `useGetOperationalAnalytics` |
