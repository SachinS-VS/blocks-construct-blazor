# LMT — Frontend Guide

This file defines frontend patterns for the `lmt` (Logging, Monitoring & Tracing) skill.
Always read `core/frontend-react.md` first, then apply the additions here.

---

## Module Structure

All LMT UI lives inside `src/modules/lmt/`:

```
modules/lmt/
├── components/
│   ├── log-filter/           ← date range, level, service name selectors
│   ├── log-table/            ← paginated log rows with level badges
│   ├── live-log-panel/       ← SSE streaming terminal view
│   ├── trace-list/           ← paginated trace rows with duration bars
│   ├── trace-detail/         ← span waterfall / call tree
│   └── analytics-charts/     ← status code pie chart, latency bar chart
├── pages/
│   ├── logs/                 ← log viewer with filter panel
│   ├── traces/               ← trace list with filter
│   └── analytics/            ← charts for status codes and latency
├── hooks/
│   └── use-lmt.tsx           ← all LMT queries
├── services/
│   └── lmt.service.ts        ← raw API calls
└── types/
    └── lmt.type.ts           ← TypeScript types
```

---

## TypeScript Types

```typescript
// lmt.type.ts

export interface LogEntry {
  timestamp: string
  level: 'Trace' | 'Debug' | 'Information' | 'Warning' | 'Error' | 'Critical'
  message: string
  serviceName: string
  traceId?: string
  spanId?: string
  properties: Record<string, unknown>
}

export interface GetLogsRequest {
  serviceName: string
  page?: number
  pageSize?: number
  sort?: { property: string; isDescending: boolean }
  filter?: {
    startDate?: string
    endDate?: string
    logLevel?: string
    traceId?: string
    spanId?: string
  }
  search?: string
  projectKey: string
}

export interface TraceSpan {
  spanId: string
  parentSpanId: string | null
  operationName: string
  serviceName: string
  startTime: string
  endTime: string
  duration: number
  statusCode: number
  tags: Record<string, string>
  logs: unknown[]
}

export interface Trace {
  traceId: string
  rootSpan: Omit<TraceSpan, 'parentSpanId' | 'endTime' | 'tags' | 'logs'>
  spanCount: number
  totalDuration: number
}

export interface OperationMetric {
  operationName: string
  callCount: number
  avgDurationMs: number
  p95DurationMs: number
  p99DurationMs: number
  errorRate: number
}
```

---

## Hooks Pattern

```typescript
// use-lmt.tsx
export const useGetLogs = (request: GetLogsRequest) =>
  useQuery({
    queryKey: ['logs', request],
    queryFn: () => lmtService.getLogs(request),
    enabled: !!request.serviceName,
  })

export const useGetLogsByDate = (request: GetLogsByDateRequest) =>
  useQuery({
    queryKey: ['logs-by-date', request],
    queryFn: () => lmtService.getLogsByDate(request),
    enabled: !!request.serviceName && !!request.filter?.startDate,
  })

export const useGetTraces = (request: GetTracesRequest) =>
  useQuery({
    queryKey: ['traces', request],
    queryFn: () => lmtService.getTraces(request),
  })

export const useGetTrace = (traceId: string, projectKey: string) =>
  useQuery({
    queryKey: ['trace', traceId],
    queryFn: () => lmtService.getTrace(traceId, projectKey),
    enabled: !!traceId,
  })

export const useGetOperationalAnalytics = (request: GetApiAnalyticsRequest) =>
  useQuery({
    queryKey: ['operational-analytics', request],
    queryFn: () => lmtService.getOperationalAnalytics(request),
    enabled: !!request.serviceName,
  })
```

---

## Live Log Streaming

```typescript
// live-log-panel.tsx
// EventSource does not support custom Authorization headers.
// Use a short-lived token query param, or proxy through your backend.

const useLiveLogs = (serviceName: string, projectKey: string) => {
  const [logs, setLogs] = useState<LogEntry[]>([])

  useEffect(() => {
    if (!serviceName) return

    const url = new URL(`${import.meta.env.VITE_API_BASE_URL}/lmt/v1/Log/Live`)
    url.searchParams.set('serviceName', serviceName)
    url.searchParams.set('projectKey', projectKey)

    const evtSource = new EventSource(url.toString())

    evtSource.onmessage = (event) => {
      try {
        const log: LogEntry = JSON.parse(event.data)
        setLogs(prev => [log, ...prev].slice(0, 500)) // keep last 500
      } catch {
        // malformed event — skip
      }
    }

    evtSource.onerror = () => {
      evtSource.close()
      // reconnect after 3 seconds
      setTimeout(() => evtSource, 3000)
    }

    return () => evtSource.close()
  }, [serviceName, projectKey])

  return logs
}
```

---

## Log Level Colours

Use consistent colour coding for log levels:

```typescript
const LOG_LEVEL_COLOURS = {
  Trace:       'text-gray-400',
  Debug:       'text-blue-400',
  Information: 'text-green-500',
  Warning:     'text-yellow-500',
  Error:       'text-red-500',
  Critical:    'text-red-700 font-bold',
}
```

---

## Trace Waterfall Component

Display spans as a horizontal waterfall chart:

- Each span is a horizontal bar
- Width = proportional to duration relative to root span
- Indent = nesting level (child spans indented under parent)
- Colour = green for 2xx, yellow for 4xx, red for 5xx

```typescript
// trace-detail.tsx
// Build the span tree from flat spans array:
const buildTree = (spans: TraceSpan[]) => {
  const map = new Map(spans.map(s => [s.spanId, { ...s, children: [] }]))
  const roots: typeof map extends Map<string, infer T> ? T[] : never[] = []
  map.forEach(span => {
    if (span.parentSpanId && map.has(span.parentSpanId)) {
      map.get(span.parentSpanId)!.children.push(span)
    } else {
      roots.push(span)
    }
  })
  return roots
}
```

---

## Route Definitions

```typescript
// routes/lmt.route.tsx
const lmtRoutes = [
  { path: '/lmt/logs',      element: <LogsPage /> },
  { path: '/lmt/traces',    element: <TracesPage /> },
  { path: '/lmt/analytics', element: <AnalyticsPage /> },
]
```

All LMT routes require authentication (`<ProtectedRoute>`). Typically restricted to admin/devops roles.

---

## Error Handling

| Error | Message to show |
|-------|----------------|
| Empty log results | "No logs found for the selected filters. Try widening the date range or changing the log level." |
| 400 on get-logs | "Invalid service name. Check the service identifier and try again." |
| 404 on get-trace | "Trace not found. Traces are retained for a limited window — this trace may have expired." |
| SSE connection dropped | "Live log connection lost. Reconnecting..." |
