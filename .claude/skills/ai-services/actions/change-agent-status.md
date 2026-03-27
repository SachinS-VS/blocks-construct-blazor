# Action: change-agent-status

## Purpose

Enable, disable, or archive an AI agent by changing its status.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/agents/change-status
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/agents/change-status" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "agent_id": "agt_abc123",
    "status": "active",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent |
| `status` | string | yes | New status. One of: `active`, `inactive`, `archived` |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

### Status Values

| Status | Effect |
|--------|--------|
| `active` | Agent is live and can receive conversations |
| `inactive` | Agent is disabled; existing sessions are unaffected but no new ones can start |
| `archived` | Agent is hidden from normal lists; cannot be activated without explicit restore |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Agent status updated to active",
  "item_id": "agt_abc123",
  "error": {}
}
```

---

## On Failure

- `400` — Invalid `status` value — must be one of `active`, `inactive`, `archived`
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to change agent status
- `404` — Agent not found — verify `agent_id` and `project_key`
