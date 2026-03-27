# Action: query-lmt

## Purpose

Send a message directly to a language model configuration and receive a complete (non-streaming) response.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/ai-agent/query-lmt
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/ai-agent/query-lmt" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "message": "Summarize the key benefits of using vector databases for AI applications.",
    "model_id": "mdl_abc123",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | yes | The prompt or message to send to the LLM |
| `model_id` | string | yes | ID of the model configuration to use |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "response": "Vector databases provide several key benefits for AI applications: 1) Semantic search — they store embeddings...",
  "model_id": "mdl_abc123",
  "is_success": true
}
```

> This endpoint blocks until the full response is generated. For long responses, use `stream-query-lmt` instead.

---

## On Failure

- `400` — Empty `message` or missing required fields
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to query models
- `404` — Model configuration not found — verify `model_id`
- `422` — Model API key is invalid or the model provider returned an error
- `504` — Request timed out — the model took too long to respond; use `stream-query-lmt` for long responses
