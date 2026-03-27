# Action: publish-agent

## Purpose

Publish an AI agent to the marketplace, making it available for wider use.

---

## Endpoint

```
GET $VITE_API_BASE_URL/blocksai-api/v1/agents/publish-agents/{agent_id}
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/agents/publish-agents/agt_abc123" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent to publish (in the URL path) |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Agent published to marketplace successfully",
  "item_id": "agt_abc123",
  "error": {}
}
```

> The agent must have status `active` and a valid AI configuration (model attached) before it can be published.

---

## On Failure

- `400` — Agent is not in `active` status or missing required AI configuration
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to publish agents
- `404` — Agent not found — verify the `agent_id` in the URL path
- `409` — Agent is already published to the marketplace
