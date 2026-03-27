# Action: get-agents

## Purpose

List AI agents for a project with optional search, status filter, and pagination.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/agents/queries
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/agents/queries" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "limit": 20,
    "offset": 0,
    "search": "",
    "status": "active",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `limit` | integer | no | Max results per page. Default: `20` |
| `offset` | integer | no | Pagination offset. Default: `0` |
| `search` | string | no | Search term to filter agents by name or description |
| `status` | string | no | Filter by status: `active`, `inactive`, `archived`. Omit to return all |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "items": [
    {
      "agent_id": "agt_abc123",
      "name": "Customer Support Agent",
      "description": "Handles customer inquiries and support tickets",
      "status": "active",
      "model_id": "mdl_xyz789",
      "created_at": "2024-01-15T10:30:00Z",
      "updated_at": "2024-01-20T14:00:00Z"
    }
  ],
  "total": 1,
  "is_success": true
}
```

---

## On Failure

- `400` — Invalid filter values or malformed request body
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to list agents in this project
- `404` — Project not found — verify `VITE_PROJECT_SLUG`
