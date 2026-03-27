# Action: initiate-conversation

## Purpose

Start a new chat session or reconnect to an existing one for an agent, returning a `session_id` to use in subsequent chat calls.

---

## Endpoint

```
GET $VITE_API_BASE_URL/blocksai-api/v1/conversation/initiate?agent_id={agent_id}
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/conversation/initiate?agent_id=agt_abc123" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent to start a conversation with |

---

## On Success (200)

```json
{
  "session_id": "sess_xyz789",
  "agent_id": "agt_abc123",
  "created_at": "2024-01-20T15:00:00Z",
  "is_success": true
}
```

Store the `session_id` in your app state. Pass it to:
- `chat-agent` — for workspace-based chat
- `chat-sse` — for SSE streaming chat
- `delete-conversation` — to end the session

---

## On Failure

- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to start conversations with this agent
- `404` — Agent not found — verify `agent_id`
- `422` — Agent is not in `active` status — activate the agent first via `change-agent-status`
