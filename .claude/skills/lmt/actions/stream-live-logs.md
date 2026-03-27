# Action: stream-live-logs

## Purpose

Stream live log output for a named service in real time using Server-Sent Events (SSE).

---

## Endpoint

```
GET $VITE_API_BASE_URL/lmt/v1/Log/Live?serviceName={serviceName}&projectKey={projectKey}
```

---

## curl

```bash
curl --location --no-buffer \
  "$VITE_API_BASE_URL/lmt/v1/Log/Live?serviceName=identity-service&projectKey=$VITE_X_BLOCKS_KEY" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Accept: text/event-stream"
```

---

## Query Parameters

| Param | Type | Required | Notes |
|-------|------|----------|-------|
| serviceName | string | yes | SELISE Blocks service name |
| projectKey | string | yes | Use $VITE_X_BLOCKS_KEY |

---

## Response

Returns a continuous SSE stream. Each event is a log entry:

```
data: {"timestamp":"2024-01-15T10:30:01Z","level":"Information","message":"User login attempt","traceId":"abc"}

data: {"timestamp":"2024-01-15T10:30:02Z","level":"Error","message":"Token validation failed","traceId":"xyz"}
```

The connection stays open until the client disconnects.

---

## Frontend Usage

```typescript
const evtSource = new EventSource(
  `${import.meta.env.VITE_API_BASE_URL}/lmt/v1/Log/Live?serviceName=${serviceName}&projectKey=${projectKey}`,
  { withCredentials: false }
)

evtSource.onmessage = (event) => {
  const log = JSON.parse(event.data)
  setLogs(prev => [log, ...prev].slice(0, 500)) // keep last 500
}

evtSource.onerror = () => evtSource.close()
```

Note: EventSource does not support custom headers for Authorization. Pass the token via a query param or use a short-lived token issued specifically for this SSE connection.

---

## On Failure

* 400 — missing `serviceName`
* 401 — authentication failed
* 503 — service unavailable or log stream not available
