# Action: chat-sse

## Purpose

Send a message to an AI agent and receive the response as a Server-Sent Events (SSE) stream, delivering tokens in real time.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/chat/{session_id}
```

---

## curl

```bash
curl --location --no-buffer "$VITE_API_BASE_URL/blocksai-api/v1/chat/sess_xyz789" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "message": "What is your return policy?",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

> Use `--no-buffer` with curl to see tokens as they stream.

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `session_id` | string | yes | Active session ID from `initiate-conversation` (in the URL path) |

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | yes | The user's message to send to the agent |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200 — SSE stream)

The response is a Server-Sent Events stream. Each event delivers a token:

```
data: {"token": "Our"}
data: {"token": " return"}
data: {"token": " policy"}
data: {"token": " allows"}
data: [DONE]
```

Accumulate all `token` values until `data: [DONE]` to build the full assistant reply.

### Frontend Implementation

```ts
const abortController = new AbortController()

const sendMessage = async (message: string, session_id: string) => {
  setIsStreaming(true)
  setStreamingResponse('')

  const res = await fetch(`${BASE}/chat/${session_id}`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify({ message, project_key }),
    signal: abortController.signal,
  })

  if (!res.ok) {
    setIsStreaming(false)
    throw new Error(`Chat failed: ${res.status}`)
  }

  const reader = res.body?.getReader()
  const decoder = new TextDecoder()

  while (reader) {
    const { done, value } = await reader.read()
    if (done) break
    const chunk = decoder.decode(value, { stream: true })
    chunk.split('\n').forEach(line => {
      if (line.startsWith('data: ') && line.trim() !== 'data: [DONE]') {
        try {
          const { token } = JSON.parse(line.slice(6))
          setStreamingResponse(prev => prev + token)
        } catch {
          // Skip malformed lines
        }
      }
    })
  }

  setIsStreaming(false)
}

// Cancel stream on unmount or navigation
useEffect(() => () => abortController.abort(), [])
```

---

## On Failure

- `400` — Empty `message` or missing `project_key`
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to chat with this agent
- `404` — Session not found — call `initiate-conversation` first
- `422` — Agent has no model configured — attach a model via `update-agent-ai-config`
