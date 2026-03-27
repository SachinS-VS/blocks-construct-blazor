# Action: create-model

## Purpose

Create a new AI model configuration with provider credentials, making it available for agents to use.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/models/
```

---

## curl

```bash
curl --location "$VITE_API_BASE_URL/blocksai-api/v1/models/" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY" \
  --header "Content-Type: application/json" \
  --data '{
    "name": "GPT-4o Production",
    "provider": "OpenAI",
    "model_name": "gpt-4o",
    "api_key": "sk-...",
    "base_url": "",
    "project_key": "'"$VITE_PROJECT_SLUG"'"
  }'
```

---

## Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | yes | Display name for this model configuration |
| `provider` | string | yes | Provider name: `OpenAI`, `Anthropic`, `AzureOpenAI`, `Cohere`, `Ollama` |
| `model_name` | string | yes | The model identifier from the provider (e.g., `gpt-4o`, `claude-3-5-sonnet-20241022`) |
| `api_key` | string | yes | API key for authenticating with the provider |
| `base_url` | string | no | Custom base URL. Required for `AzureOpenAI` (Azure endpoint URL) and `Ollama` (local server URL) |
| `project_key` | string | yes | Project identifier — use `$VITE_PROJECT_SLUG` |

### Provider and Model Name Examples

| Provider | Example `model_name` values |
|----------|-----------------------------|
| `OpenAI` | `gpt-4o`, `gpt-4o-mini`, `gpt-4-turbo` |
| `Anthropic` | `claude-3-5-sonnet-20241022`, `claude-3-opus-20240229` |
| `AzureOpenAI` | `gpt-4o` (deployment name in Azure) |
| `Cohere` | `command-r-plus`, `command-r` |
| `Ollama` | `llama3.2`, `mistral`, `gemma2` |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Model configuration created successfully",
  "item_id": "mdl_abc123",
  "error": {}
}
```

The `item_id` is the `model_id`. Use it in `update-agent-ai-config` to assign this model to an agent.

---

## On Failure

- `400` — Missing required fields or invalid `provider` value
- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to create model configurations
- `404` — Project not found — verify `VITE_PROJECT_SLUG`
- `422` — API key validation failed — check the `api_key` value
