# Action: create-agent

## Purpose

Create a new AI agent in the project from a name and description.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/agents/create
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/agents/create" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "name": "Customer Support Agent",
    "description": "Handles customer inquiries and support tickets",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | yes | Display name for the agent |
| `description` | string | yes | What this agent is designed to do |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Agent created successfully",
  "item_id": "agt_abc123",
  "error": {}
}
```

The `item_id` is the new `agent_id`. Store it to use in subsequent calls to `update-agent-persona`, `update-agent-ai-config`, and `change-agent-status`.

---

## On Failure

- `400` — Missing required fields or invalid `project_key`
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to create agents in this project
- `404` — Project not found — verify `VITE_PROJECT_SLUG`
- `409` — Agent with the same name already exists in this project
