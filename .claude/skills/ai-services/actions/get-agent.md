# Action: get-agent

## Purpose

Retrieve full details for a single AI agent by its ID.

---

## Endpoint

```
GET $VITE_API_BASE_URL/blocksai-api/v1/agents/query/{agent_id}
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/agents/query/agt_abc123" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent to retrieve (in the URL path) |

---

## On Success (200)

```json
{
  "agent_id": "agt_abc123",
  "name": "Customer Support Agent",
  "description": "Handles customer inquiries and support tickets",
  "persona": "You are a helpful, professional support agent.",
  "status": "active",
  "model_id": "mdl_xyz789",
  "temperature": 0.7,
  "max_tokens": 2048,
  "system_prompt": "You are a helpful assistant. Answer questions accurately and concisely.",
  "kb_ids": ["kb_001", "kb_002"],
  "tool_ids": ["tool_001"],
  "project_key": "my-project",
  "created_at": "2024-01-15T10:30:00Z",
  "updated_at": "2024-01-20T14:00:00Z",
  "is_success": true
}
```

---

## On Failure

- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to view this agent
- `404` — Agent not found — verify the `agent_id` in the URL path
