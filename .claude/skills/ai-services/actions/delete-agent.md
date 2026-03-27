# Action: delete-agent

## Purpose

Permanently delete an AI agent and all its associated configuration.

---

## Endpoint

```
DELETE $VITE_API_BASE_URL/blocksai-api/v1/agents/delete/{agent_id}
```

---

## curl

```bash
curl --location --request DELETE \
  "$VITE_API_BASE_URL/blocksai-api/v1/agents/delete/agt_abc123" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent to delete (in the URL path) |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Agent deleted successfully",
  "item_id": "agt_abc123",
  "error": {}
}
```

> This operation is irreversible. All conversations, sessions, and configurations tied to this agent are also removed.

---

## On Failure

- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to delete agents in this project
- `404` — Agent not found — verify the `agent_id` in the URL path
- `409` — Agent has active conversations; archive it first or end all sessions
