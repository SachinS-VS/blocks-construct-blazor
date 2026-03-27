# Action: update-agent-persona

## Purpose

Update an agent's display name, description, and persona (personality/behavior instructions).

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/agents/update-persona
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/agents/update-persona" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "agent_id": "agt_abc123",
    "name": "Customer Support Agent",
    "description": "Handles customer inquiries and support tickets",
    "persona": "You are a helpful, professional support agent. Always be polite and empathetic. Keep responses concise and actionable.",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent to update |
| `name` | string | yes | Updated display name |
| `description` | string | yes | Updated description |
| `persona` | string | no | Personality and behavioral instructions for the agent |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Agent persona updated successfully",
  "item_id": "agt_abc123",
  "error": {}
}
```

---

## On Failure

- `400` — Missing required fields or malformed request body
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to update this agent
- `404` — Agent not found — verify `agent_id` and `project_key`
