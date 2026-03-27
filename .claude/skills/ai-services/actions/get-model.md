# Action: get-model

## Purpose

Retrieve full details for a single AI model configuration by its ID.

---

## Endpoint

```
GET $VITE_API_BASE_URL/blocksai-api/v1/models/{model_id}
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/models/mdl_abc123" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `model_id` | string | yes | ID of the model configuration to retrieve (in the URL path) |

---

## On Success (200)

```json
{
  "model_id": "mdl_abc123",
  "name": "GPT-4o Production",
  "provider": "OpenAI",
  "model_name": "gpt-4o",
  "base_url": "",
  "project_key": "my-project",
  "created_at": "2024-01-15T10:30:00Z",
  "is_success": true
}
```

> The `api_key` is never returned in responses. To update the API key, create a new model configuration.

---

## On Failure

- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to view this model configuration
- `404` — Model configuration not found — verify the `model_id` in the URL path
