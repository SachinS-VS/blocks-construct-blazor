# Action: create-api-tool

## Purpose

Create or update an API tool that an AI agent can call, with defined actions, authentication, and parameter schemas.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/tools/api
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/tools/api" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "name": "Weather API",
    "description": "Get current weather and forecasts for any location",
    "base_url": "https://api.weather.example.com",
    "auth_type": "ApiKey",
    "auth_value": "your-api-key-here",
    "actions": [
      {
        "name": "get_current_weather",
        "method": "GET",
        "path": "/current",
        "description": "Get current weather conditions for a given city",
        "parameters": [
          {
            "name": "city",
            "type": "string",
            "required": true,
            "description": "The name of the city"
          },
          {
            "name": "units",
            "type": "string",
            "required": false,
            "description": "Temperature units: metric or imperial"
          }
        ]
      }
    ],
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | yes | Tool display name |
| `description` | string | yes | What this tool does (used by the AI to decide when to use it) |
| `base_url` | string | yes | Base URL for all API calls from this tool |
| `auth_type` | string | yes | Authentication type: `None`, `ApiKey`, `Bearer`, `Basic` |
| `auth_value` | string | no | Auth credential value (API key, token, or `username:password` for Basic) |
| `actions` | object[] | yes | List of API actions this tool exposes to the agent |
| `actions[].name` | string | yes | Action identifier (used by the AI as a function name) |
| `actions[].method` | string | yes | HTTP method: `GET`, `POST`, `PUT`, `DELETE` |
| `actions[].path` | string | yes | Path relative to `base_url` |
| `actions[].description` | string | yes | What this action does (used by the AI to choose the right action) |
| `actions[].parameters` | object[] | no | Parameter schema for this action |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "API tool created successfully",
  "item_id": "tool_abc123",
  "error": {}
}
```

The `item_id` is the `tool_id`. Use it in `update-agent-ai-config` to attach this tool to an agent.

---

## On Failure

- `400` — Missing required fields, invalid `auth_type`, or malformed `actions` array
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to create tools in this project
- `404` — Project not found — verify `VITE_PROJECT_SLUG`
- `409` — A tool with the same name already exists in this project
