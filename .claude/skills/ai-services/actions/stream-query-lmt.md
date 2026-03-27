# Action: stream-query-lmt

## Purpose

Send a message directly to a language model and receive the response as a stream of tokens.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/ai-agent/query-lmt/stream
```

---

## curl

```bash
curl --location --no-buffer "$VITE_API_BASE_URL/blocksai-api/v1/ai-agent/query-lmt/stream" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "message": "Write a detailed explanation of transformer architecture.",
    "model_id": "mdl_abc123",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

> Use `--no-buffer` with curl to see tokens as they arrive.

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | yes | The prompt or message to send to the LLM |
| `model_id` | string | yes | ID of the model configuration to use |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200 — SSE stream)

The response is a Server-Sent Events stream. Each event delivers a token:

```
data: {"token": "Transformer"}
data: {"token": " architecture"}
data: {"token": " is"}
data: {"token": " a"}
data: [DONE]
```

Accumulate all `token` values until `data: [DONE]` to build the complete response.

### Frontend Implementation

```ts
const res = await fetch(`${BASE}/ai-agent/query-lmt/stream`, {
  method: 'POST',
  headers: getAuthHeaders(),
  body: JSON.stringify({ message, model_id, project_key }),
  signal: abortController.signal,
})

const reader = res.body?.getReader()
const decoder = new TextDecoder()
let fullResponse = ''

while (reader) {
  const { done, value } = await reader.read()
  if (done) break
  const chunk = decoder.decode(value)
  chunk.split('\n').forEach(line => {
    if (line.startsWith('data: ') && line !== 'data: [DONE]') {
      const { token } = JSON.parse(line.slice(6))
      fullResponse += token
      setDisplayText(fullResponse) // update UI on each token
    }
  })
}
```

---

## On Failure

- `400` — Empty `message` or missing required fields
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to query models
- `404` — Model configuration not found — verify `model_id`
- `422` — Model API key is invalid or the model provider returned an error
