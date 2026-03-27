# Flow: create-agent-flow

## Trigger

User wants to create and configure a new AI agent.

> "create an AI agent"
> "set up a new chatbot"
> "build an agent for customer support"
> "configure an agent with a knowledge base"
> "I want an AI assistant for my app"

---

## Pre-flight Questions

Before starting, confirm:

1. What should this agent do? (e.g., answer customer questions, help with onboarding, summarize documents)
2. Which AI model should it use? (provider: OpenAI, Anthropic, etc. — or ask to list configured models first)
3. What data sources will it need? (uploaded files, text content, URLs, or none)
4. Does it need external tool access? (APIs to call, MCP server, or none)
5. Should it be published to the marketplace, or stay private?

---

## Flow Steps

### Step 1 — Check for existing model configurations

Before creating the agent, check if there are usable models already configured.

```
Action: get-models
Input:  project_key = $VITE_PROJECT_SLUG
Output: list of existing model configurations
```

**Branch:**
- Models exist → proceed to Step 2 using an existing `model_id`
- No models exist → proceed to `manage-models` flow first, then return here

---

### Step 2 — Create the agent

Call `create-agent` to register the agent with name and description.

```
Action: create-agent
Input:
  name         = agent name from user
  description  = what the agent does
  project_key  = $VITE_PROJECT_SLUG

Output:
  item_id → agent_id (store this)
```

Confirm success before continuing.

---

### Step 3 — Update agent persona

Set the agent's personality and behavioral instructions.

```
Action: update-agent-persona
Input:
  agent_id    = agent_id from Step 2
  name        = same as creation name
  description = same as creation description
  persona     = behavioral instructions (tone, constraints, format preferences)
  project_key = $VITE_PROJECT_SLUG
```

**Persona guidance:**
- Be specific — e.g., "You are a concise, professional support agent. Always respond in bullet points. Never make up information."
- Include scope limits — e.g., "Only answer questions about our products. Politely decline off-topic requests."
- Include tone — e.g., "Use friendly, informal language. Avoid jargon."

---

### Step 4 — Set up knowledge base (if needed)

If the agent needs data sources, run `setup-knowledge-base` flow first to create a KB folder and ingest content.

```
If data sources needed:
  → Run: flows/setup-knowledge-base.md
  → Output: kb_folder_id and one or more kb_ids

If no data sources:
  → Skip to Step 5
```

---

### Step 5 — Configure AI settings

Attach the model, KB, tools, and system prompt.

```
Action: update-agent-ai-config
Input:
  agent_id      = agent_id from Step 2
  model_id      = model_id from Step 1 or manage-models flow
  temperature   = 0.7 (default) or user preference
  max_tokens    = 2048 (default) or user preference
  system_prompt = high-level instructions (complements persona)
  kb_ids        = [kb_folder_id, ...] from Step 4 (empty array if none)
  tool_ids      = [tool_id, ...] from tool creation (empty array if none)
  project_key   = $VITE_PROJECT_SLUG
```

**Temperature guidance:**
- `0.0–0.3` — Factual, deterministic (Q&A, support, data lookup)
- `0.4–0.7` — Balanced (general-purpose assistants)
- `0.8–1.0` — Creative (brainstorming, writing, ideation)

**System prompt vs persona:**
- `system_prompt` — High-level role definition (e.g., "You are a customer support agent for Acme Corp.")
- `persona` — Behavioral detail (e.g., "Always respond in 3 bullet points. Be concise.")

---

### Step 6 — Activate the agent

Set agent status to `active` so it can receive conversations.

```
Action: change-agent-status
Input:
  agent_id    = agent_id from Step 2
  status      = "active"
  project_key = $VITE_PROJECT_SLUG
```

---

### Step 7 — Test retrieval (if KB was added)

Verify the knowledge base is working correctly.

```
Action: test-kb-retrieval
Input:
  agent_id    = agent_id from Step 2
  query       = a test question relevant to the KB content
  top_k       = 5
  project_key = $VITE_PROJECT_SLUG

Expected: results with scores above 0.7
```

**Branch:**
- Scores above 0.7 → KB is working well, proceed to Step 8
- Scores below 0.5 → Review content quality, check chunk_size, re-ingest with better-structured content
- No results → KB indexing may still be in progress — wait 30 seconds and retry

---

### Step 8 — Publish to marketplace (optional)

If the user wants to publish the agent:

```
Action: publish-agent
Input:
  agent_id = agent_id from Step 2

Prerequisite: agent must be in "active" status with a model configured
```

**Branch:**
- Success → agent is now in the marketplace
- Skip → agent remains private, accessible only within this project

---

## Error Handling

| Step | Error | Cause | Action |
|------|-------|-------|--------|
| Step 1 | No models found | No model configurations exist | Run `manage-models` flow first |
| Step 2 | `409` | Agent name already taken | Use a different name or get-agents to find the existing one |
| Step 2 | `404` | Project not found | Verify `VITE_PROJECT_SLUG` |
| Step 4 | KB ingestion fails | Unsupported file type or network error | Check file format and retry |
| Step 5 | `404` on `model_id` | Model was deleted after listing | Re-run Step 1 to get a fresh model list |
| Step 5 | `404` on `kb_ids` | KB folder or entry not found | Verify KB IDs from setup-knowledge-base flow |
| Step 7 | Low retrieval scores | Poor content quality or wrong chunk size | Re-ingest with better-chunked or more structured content |
| Step 8 | `422` on publish | Agent not active or no model | Complete Steps 5 and 6 first |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `src/modules/ai/pages/agents/agents-page.tsx` | Agent list with "Create Agent" button |
| `src/modules/ai/pages/agents/create-agent-dialog.tsx` | Modal form — name, description fields |
| `src/modules/ai/pages/agent-detail/agent-detail-page.tsx` | Tabbed config view after creation |
| `src/modules/ai/pages/agent-detail/agent-persona-tab.tsx` | Name, description, persona form |
| `src/modules/ai/pages/agent-detail/agent-ai-config-tab.tsx` | Model picker, temperature slider, system prompt |
| `src/modules/ai/pages/agent-detail/agent-kb-tab.tsx` | KB attachment manager |
| `src/modules/ai/pages/agent-detail/agent-tools-tab.tsx` | Tool attachment manager |
| `src/modules/ai/components/agent-card/agent-card.tsx` | Agent card in list view |
| `src/modules/ai/hooks/use-ai.tsx` | `useCreateAgent`, `useUpdateAgentPersona`, `useUpdateAgentAiConfig`, `useChangeAgentStatus`, `usePublishAgent` |
| `src/modules/ai/services/ai.service.ts` | API call implementations |
| `src/modules/ai/types/ai.type.ts` | `Agent`, `CreateAgentPayload`, `UpdateAgentPersonaPayload`, `UpdateAiConfigPayload` |
| `src/routes/ai.route.tsx` | `/ai/agents` and `/ai/agents/:agent_id` routes |
