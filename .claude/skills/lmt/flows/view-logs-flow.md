# Flow: view-logs-flow

## Trigger

Developer wants to inspect service logs to debug an issue or audit activity.

> "show me logs for the identity service"
> "find errors from last week"
> "debug why login is failing"
> "watch live logs"

---

## Pre-flight Questions

1. Which service are you debugging? (e.g. `identity-service`, `communication-service`)
2. What time range? (last hour / today / custom date range)
3. Are you looking for a specific log level? (Error, Warning, all)
4. Do you have a trace ID to correlate with? (optional)
5. Do you want live streaming logs or historical?

---

## Flow Steps

### Step 1 — Query Historical Logs

For date-bounded queries use `get-logs-by-date`. For open-ended filtering use `get-logs`.

```
Action: get-logs-by-date (if date range given) or get-logs
Input:
  serviceName  = chosen service
  filter.startDate / endDate
  filter.logLevel = "Error" (or omit for all)
  sort.isDescending = true
  pageSize = 50
  projectKey = VITE_X_BLOCKS_KEY
```

```
On success → display log list
On 400     → check serviceName is correct
```

---

### Step 2 — Drill Into a Trace (optional)

If a log entry has a `traceId`, use it to get full distributed trace context:

```
Action: get-trace
Input:
  traceId = traceId from log entry
  projectKey = VITE_X_BLOCKS_KEY
```

This shows the complete call tree — useful for latency debugging.

---

### Step 3 — Live Logs (alternative to Step 1)

If the user wants to watch logs as they happen:

```
Action: stream-live-logs
Input:
  serviceName = chosen service
  projectKey = VITE_X_BLOCKS_KEY
```

Connect via EventSource. Display logs as they arrive in a terminal-style panel.

> Note: EventSource does not support Authorization headers. Generate a short-lived token or pass auth via query param if required.

---

## Error Handling

| Error | Cause | Action |
|-------|-------|--------|
| 400 on get-logs | Missing or invalid serviceName | Check the correct service identifier |
| 404 on get-trace | TraceId expired or not found | Traces are retained for a limited window |
| Empty results | No logs match the filter | Widen date range or lower log level filter |
| SSE connection drops | Network issue | Reconnect automatically with exponential backoff |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `modules/lmt/pages/logs/logs-page.tsx` | Main log viewer with filter panel |
| `modules/lmt/components/log-filter/log-filter.tsx` | Date range, level, service selectors |
| `modules/lmt/components/log-table/log-table.tsx` | Paginated log list with level badges |
| `modules/lmt/components/live-log-panel/live-log-panel.tsx` | SSE streaming terminal view |
| `modules/lmt/hooks/use-lmt.tsx` | `useGetLogs`, `useGetLogsByDate` queries |
| `modules/lmt/services/lmt.service.ts` | API calls |
| `modules/lmt/types/lmt.type.ts` | TypeScript types for log/trace shapes |
