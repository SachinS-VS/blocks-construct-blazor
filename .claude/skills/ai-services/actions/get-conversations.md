# Action: get-conversations

## Purpose

List all chat sessions for an agent with optional filter and pagination.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/conversation/llm-sessions
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/conversation/llm-sessions" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "agent_id": "agt_abc123",
    "limit": 20,
    "offset": 0,
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent whose sessions to list |
| `limit` | integer | no | Max results per page. Default: `20` |
| `offset` | integer | no | Pagination offset. Default: `0` |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "items": [
    {
      "session_id": "sess_xyz789",
      "agent_id": "agt_abc123",
      "title": "How do I reset my password?",
      "message_count": 6,
      "created_at": "2024-01-20T15:00:00Z",
      "updated_at": "2024-01-20T15:05:00Z"
    }
  ],
  "total": 1,
  "is_success": true
}
```

---

## On Failure

- `400` — Missing required fields or malformed request
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to view conversations for this agent
- `404` — Agent not found — verify `agent_id` and `project_key`
