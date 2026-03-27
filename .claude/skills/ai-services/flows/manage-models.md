# Flow: manage-models

## Trigger

User wants to configure an AI model for use with agents.

> "add an OpenAI model"
> "configure Claude for my agent"
> "set up a model API key"
> "what models are available?"
> "add a custom Ollama model"
> "connect my Azure OpenAI deployment"

---

## Pre-flight Questions

Before starting, confirm:

1. Which provider will you use? (OpenAI, Anthropic, AzureOpenAI, Cohere, Ollama)
2. Do you have an API key for that provider?
3. For AzureOpenAI — what is your Azure endpoint URL and deployment name?
4. For Ollama — what is your local server URL and which model is installed?
5. Do you want to see the list of available models from the provider first?

---

## Flow Steps

### Step 1 — List existing model configurations

Check what models are already configured to avoid duplicates.

```
Action: get-models
Input:  project_key = $VITE_PROJECT_SLUG
Output: list of existing model configurations
```

**Branch:**
- Required model already configured → return `model_id` to the caller, skip remaining steps
- Not configured → continue to Step 2

---

### Step 2 — Browse available models from the provider

Get the list of models available from the chosen provider to help select the right `model_name`.

```
Action: get-provider-models
Input:  provider = user-chosen provider (OpenAI | Anthropic | AzureOpenAI | Cohere | Ollama)
Output: list of model names with display names and descriptions
```

Present the list to the user. Examples by provider:

| Provider | Common model names |
|----------|--------------------|
| `OpenAI` | `gpt-4o`, `gpt-4o-mini`, `gpt-4-turbo` |
| `Anthropic` | `claude-3-5-sonnet-20241022`, `claude-3-opus-20240229`, `claude-3-haiku-20240307` |
| `AzureOpenAI` | Deployment name set in Azure Portal |
| `Cohere` | `command-r-plus`, `command-r` |
| `Ollama` | `llama3.2`, `mistral`, `gemma2`, `phi3` |

---

### Step 3 — Create the model configuration

Register the model with its API credentials.

```
Action: create-model
Input:
  name       = display name (e.g., "GPT-4o Production")
  provider   = chosen provider
  model_name = chosen model from Step 2
  api_key    = user's API key for the provider
  base_url   = (required for AzureOpenAI and Ollama, empty for others)
  project_key = $VITE_PROJECT_SLUG

Output:
  item_id → model_id (store this)
```

**Provider-specific `base_url` requirements:**

| Provider | `base_url` |
|----------|-----------|
| `OpenAI` | Leave empty — uses default `https://api.openai.com` |
| `Anthropic` | Leave empty — uses default `https://api.anthropic.com` |
| `AzureOpenAI` | Required — e.g., `https://my-deployment.openai.azure.com` |
| `Cohere` | Leave empty — uses default Cohere API URL |
| `Ollama` | Required — local server URL e.g., `http://localhost:11434` |

---

### Step 4 — Validate the model

Test the API key and connectivity before using the model in production.

```
Action: validate-model
Input:  model_id = model_id from Step 3
Output: is_success = true/false
```

**Branch:**
- `is_success: true` → model is ready; return `model_id` to the caller
- `is_success: false` (422) → invalid API key — prompt user to re-enter and recreate the model
- `is_success: false` (502) → provider unreachable — check `base_url` and provider status

---

### Step 5 — Assign to agent (if called from create-agent-flow)

If this flow was triggered as part of `create-agent-flow`, return the validated `model_id` to that flow's Step 5.

Otherwise, the user can assign the model manually via `update-agent-ai-config`:

```
Action: update-agent-ai-config
Input:
  agent_id    = target agent ID
  model_id    = model_id from Step 3
  project_key = $VITE_PROJECT_SLUG
  (include all other current config fields — fetch with get-agent first)
```

---

## Error Handling

| Step | Error | Cause | Action |
|------|-------|-------|--------|
| Step 2 | `400` | Invalid provider name | Must be exactly: `OpenAI`, `Anthropic`, `AzureOpenAI`, `Cohere`, or `Ollama` |
| Step 3 | `409` | Model name already exists | Use a different display `name` or update the existing configuration |
| Step 3 | `400` | Missing `base_url` | AzureOpenAI and Ollama require `base_url` — it cannot be empty |
| Step 4 | `422` | Invalid API key | Re-enter the API key and recreate the model configuration |
| Step 4 | `502` | Provider unreachable | For Ollama — verify local server is running; for Azure — check endpoint URL |
| Step 5 | `404` | Agent not found | Verify `agent_id` is correct and agent has not been deleted |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `src/modules/ai/pages/agent-detail/agent-ai-config-tab.tsx` | Model picker dropdown — shows configured models |
| `src/modules/ai/components/model-selector/model-selector.tsx` | Dropdown to select a model from the list |
| `src/modules/ai/components/model-form/model-form.tsx` | Form to create a new model configuration |
| `src/modules/ai/hooks/use-ai.tsx` | `useGetModels`, `useGetModel`, `useCreateModel`, `useValidateModel`, `useGetProviderModels` |
| `src/modules/ai/services/ai.service.ts` | `getModels()`, `createModel()`, `validateModel()`, `getProviderModels()` |
| `src/modules/ai/types/ai.type.ts` | `AIModel`, `CreateModelPayload` |
