# Action: get-tools

## Purpose

List all tools in a project with optional filtering.

---

## Endpoint

```
GET $VITE_API_BASE_URL/blocksai-api/v1/tools/
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/tools/?project_key=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |
| `search` | string | no | Search term to filter tools by name |
| `limit` | integer | no | Max results per page. Default: `20` |
| `offset` | integer | no | Pagination offset. Default: `0` |

---

## On Success (200)

```json
{
  "items": [
    {
      "tool_id": "tool_abc123",
      "name": "Weather API",
      "description": "Get current weather and forecasts for any location",
      "base_url": "https://api.weather.example.com",
      "auth_type": "ApiKey",
      "actions": [
        {
          "name": "get_current_weather",
          "method": "GET",
          "path": "/current",
          "description": "Get current weather conditions for a given city"
        }
      ],
      "project_key": "my-project",
      "created_at": "2024-01-15T10:30:00Z"
    }
  ],
  "total": 1,
  "is_success": true
}
```

---

## On Failure

- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to list tools in this project
- `404` — Project not found — verify `VITE_PROJECT_SLUG`
