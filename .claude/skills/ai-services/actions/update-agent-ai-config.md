# Action: update-agent-ai-config

## Purpose

Update an agent's AI configuration — model, temperature, max tokens, system prompt, and attached knowledge bases and tools.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/agents/update-ai-configurations
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/agents/update-ai-configurations" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "agent_id": "agt_abc123",
    "model_id": "mdl_xyz789",
    "temperature": 0.7,
    "max_tokens": 2048,
    "system_prompt": "You are a helpful assistant. Answer questions accurately and concisely.",
    "kb_ids": ["kb_001", "kb_002"],
    "tool_ids": ["tool_001"],
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent to configure |
| `model_id` | string | yes | ID of the AI model configuration to use |
| `temperature` | float | no | Response creativity from `0.0` (precise) to `1.0` (creative). Default: `0.7` |
| `max_tokens` | integer | no | Maximum response length in tokens. Default: `2048` |
| `system_prompt` | string | no | System-level instructions that guide all responses |
| `kb_ids` | string[] | no | List of knowledge base IDs to attach. Pass empty array `[]` to detach all |
| `tool_ids` | string[] | no | List of tool IDs to attach. Pass empty array `[]` to detach all |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "AI configurations updated successfully",
  "item_id": "agt_abc123",
  "error": {}
}
```

---

## On Failure

- `400` — Invalid `model_id`, invalid `temperature` value, or malformed `kb_ids`/`tool_ids`
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to configure this agent
- `404` — Agent not found, or one of the referenced `kb_ids` or `tool_ids` does not exist
