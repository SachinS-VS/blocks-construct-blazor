# Action: get-provider-models

## Purpose

Get the list of available AI models from a specific provider (seed data) to help users choose a `model_name` when creating a model configuration.

---

## Endpoint

```
GET $VITE_API_BASE_URL/blocksai-api/v1/models/seed/providers/{provider}
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/models/seed/providers/OpenAI" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `provider` | string | yes | Provider name: `OpenAI`, `Anthropic`, `AzureOpenAI`, `Cohere`, `Ollama` |

---

## On Success (200)

```json
{
  "provider": "OpenAI",
  "models": [
    {
      "model_name": "gpt-4o",
      "display_name": "GPT-4o",
      "description": "Most capable model, best for complex tasks"
    },
    {
      "model_name": "gpt-4o-mini",
      "display_name": "GPT-4o Mini",
      "description": "Lightweight and fast, great for simple tasks"
    },
    {
      "model_name": "gpt-4-turbo",
      "display_name": "GPT-4 Turbo",
      "description": "Powerful model with large context window"
    }
  ],
  "is_success": true
}
```

Use the `model_name` values from this response when calling `create-model`.

---

## On Failure

- `400` — Invalid or unsupported `provider` value — must be one of `OpenAI`, `Anthropic`, `AzureOpenAI`, `Cohere`, `Ollama`
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to view provider data
