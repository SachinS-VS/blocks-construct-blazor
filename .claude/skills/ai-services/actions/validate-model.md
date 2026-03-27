# Action: validate-model

## Purpose

Validate a model configuration's API key and connectivity by sending a test request to the provider.

---

## Endpoint

```
POST $VITE_API_BASE_URL/blocksai-api/v1/models/{model_id}/validate
```

---

## curl

```bash
curl --location --request POST \
  "$VITE_API_BASE_URL/blocksai-api/v1/models/mdl_abc123/validate" \
  --header "Authorization: Bearer $ACCESS_TOKEN" \
  --header "x-blocks-key: $VITE_X_BLOCKS_KEY"
```

---

## Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `model_id` | string | yes | ID of the model configuration to validate (in the URL path) |

---

## On Success (200)

```json
{
  "is_success": true,
  "detail": "Model API key validated successfully",
  "item_id": "mdl_abc123",
  "error": {}
}
```

A success response means the API key is valid and the model is reachable. The model is ready to be assigned to agents.

---

## On Failure

- `401` — Invalid or expired `ACCESS_TOKEN` — run `get-token` again
- `403` — Account lacks permission to validate models
- `404` — Model configuration not found — verify the `model_id` in the URL path
- `422` — Model API key is invalid or the model is not accessible at the configured `base_url` — update the model configuration with a correct API key
- `502` — Provider API is unreachable — check provider status or `base_url` (for AzureOpenAI/Ollama)
