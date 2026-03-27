# Action: chat-agent

## Purpose

Send a message to an AI agent workspace and receive a complete (non-streaming) response.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/ai-agent/chat/{w_id}
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/ai-agent/chat/sess_xyz789" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "message": "What is your return policy?",
    "session_id": "sess_xyz789",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `w_id` | string | yes | The workspace/session ID — use the `session_id` from `initiate-conversation` |

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | yes | The user's message to send to the agent |
| `session_id` | string | yes | Active session ID from `initiate-conversation` |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "response": "Our return policy allows returns within 30 days of purchase. Items must be in original condition with receipt. Refunds are processed within 5-7 business days.",
  "session_id": "sess_xyz789",
  "is_success": true
}
```

> This endpoint blocks until the full response is generated. For real-time streaming, use `chat-sse` instead.

---

## On Failure

- `400` — Empty `message`, mismatched `session_id`, or missing required fields
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to chat with this agent
- `404` — Session not found — call `initiate-conversation` first
- `422` — Agent has no model configured — attach a model via `update-agent-ai-config`
- `504` — Response timed out — use `chat-sse` for streaming to avoid timeout issues
