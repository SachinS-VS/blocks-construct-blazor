# Action: get-models

## Purpose

List all AI model configurations in a project.

---

## Endpoint

```
GET $VITE_API_BASE_URL/blocksai-api/v1/models/
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/models/?project_key=$VITE_PROJECT_SLUG" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

---

## On Success (200)

```json
{
  "items": [
    {
      "model_id": "mdl_abc123",
      "name": "GPT-4o Production",
      "provider": "OpenAI",
      "model_name": "gpt-4o",
      "base_url": "",
      "project_key": "my-project",
      "created_at": "2024-01-15T10:30:00Z"
    },
    {
      "model_id": "mdl_def456",
      "name": "Claude Sonnet",
      "provider": "Anthropic",
      "model_name": "claude-3-5-sonnet-20241022",
      "base_url": "",
      "project_key": "my-project",
      "created_at": "2024-01-16T09:00:00Z"
    }
  ],
  "total": 2,
  "is_success": true
}
```

> API keys are not returned in list responses for security reasons.

---

## On Failure

- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to list models in this project
- `404` — Project not found — verify `VITE_PROJECT_SLUG`
