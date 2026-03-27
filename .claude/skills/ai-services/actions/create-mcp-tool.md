# Action: create-mcp-tool

## Purpose

Create an MCP (Model Context Protocol) server tool that an AI agent can call to access external capabilities.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/tools/mcp-server
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/tools/mcp-server" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "name": "File System Tool",
    "description": "Allows the agent to read and write files on the server",
    "server_url": "http://localhost:3001/mcp",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | yes | Tool display name |
| `description` | string | yes | What this MCP server tool does (used by the AI to decide when to use it) |
| `server_url` | string | yes | URL of the MCP server endpoint |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "MCP server tool created successfully",
  "item_id": "tool_mcp_abc123",
  "error": {}
}
```

The `item_id` is the `tool_id`. Use it in `update-agent-ai-config` to attach this tool to an agent.

---

## On Failure

- `400` — Missing required fields or invalid `server_url` format
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to create tools in this project
- `404` — Project not found — verify `VITE_PROJECT_SLUG`
- `422` — MCP server at `server_url` is not reachable or does not implement the MCP protocol
