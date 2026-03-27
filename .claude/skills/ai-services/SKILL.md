---
name: ai-services
description: "Use this skill for creating AI agents, setting up knowledge bases (RAG), configuring LLM models, building tools (API/MCP), managing conversations, streaming chat, or querying LLMs directly on SELISE Blocks. Note: this API uses snake_case throughout."
user-invocable: false
blocks-version: "1.0.3"
---

# AI Services Skill

## Purpose

Handles all AI agent management, knowledge base ingestion, tool configuration, model setup, and conversation operations for SELISE Blocks via the blocksai-api v1 API.

> **Important:** This API uses **snake_case** (Python/FastAPI style) throughout. All request and response fields use `snake_case` — e.g., `project_key`, `agent_id`, `is_success`, `kb_ids`, `tool_ids`. Do NOT use camelCase when constructing requests or reading responses.

Must run get-token before any action in a session.

---

## When to Use

Example prompts that should route here:
- "Create an AI customer support agent with a knowledge base"
- "Upload product docs as a knowledge base for RAG"
- "Configure GPT-4 as the model for my agent"
- "Build a chat interface that streams responses from an agent"
- "Set up an MCP tool so the agent can call external APIs"

---

## Execution Context

Before executing any action or flow from this skill, read `../core/execution-context.md` for the required supporting files, load order, and cross-domain orchestration rules.

---

## Base Path

All endpoints are prefixed with: `$VITE_API_BASE_URL/blocksai-api/v1`

---

## Intent Mapping

Use this table to route user requests. Check `flows/` first — if a flow covers the request, use it. For single-action requests, go directly to the action.

| User wants to... | Use |
|------------------|-----|
| Create an AI agent from scratch | `flows/create-agent-flow.md` |
| Send a prompt to an LLM directly without an agent | `flows/query-lmt-flow.md` |
| Stream a response from a language model | `flows/query-lmt-flow.md` |
| Configure an agent's model, KB, tools | `flows/create-agent-flow.md` |
| Publish an agent to marketplace | `actions/publish-agent.md` |
| Set up a knowledge base with files/text/links | `flows/setup-knowledge-base.md` |
| Upload files to a knowledge base | `actions/upload-kb-file.md` |
| Add text content to a knowledge base | `actions/ingest-kb-text.md` |
| Add Q&A pairs to a knowledge base | `actions/ingest-kb-qa.md` |
| Crawl a URL into a knowledge base | `actions/ingest-kb-link.md` |
| Test knowledge base retrieval | `actions/test-kb-retrieval.md` |
| Chat with an AI agent | `flows/chat-flow.md` |
| Start a conversation session | `actions/initiate-conversation.md` |
| Send a message to an agent | `actions/chat-agent.md` |
| Stream a response from an agent | `actions/chat-sse.md` |
| Query the LLM directly (no agent) | `actions/query-lmt.md` |
| Stream a direct LLM query | `actions/stream-query-lmt.md` |
| Add an AI model config | `flows/manage-models.md` |
| List available models | `actions/get-models.md` |
| Validate a model's API key | `actions/validate-model.md` |
| Create an API tool for an agent | `actions/create-api-tool.md` |
| Create an MCP server tool | `actions/create-mcp-tool.md` |
| Test a tool action | `actions/test-tool-action.md` |
| List all tools | `actions/get-tools.md` |
| Delete a tool | `actions/delete-tool.md` |
| List agents | `actions/get-agents.md` |
| Get agent details | `actions/get-agent.md` |
| Update agent name/description/persona | `actions/update-agent-persona.md` |
| Update agent AI configuration | `actions/update-agent-ai-config.md` |
| Enable or disable an agent | `actions/change-agent-status.md` |
| Delete an agent | `actions/delete-agent.md` |
| List conversation sessions | `actions/get-conversations.md` |
| Delete a conversation session | `actions/delete-conversation.md` |

---

## Flows

| Flow | File | Description |
|------|------|-------------|
| create-agent-flow | flows/create-agent-flow.md | Create agent → configure AI settings → add KB → add tools → publish |
| setup-knowledge-base | flows/setup-knowledge-base.md | Create folder → upload files/text/links → test retrieval → attach to agent |
| chat-flow | flows/chat-flow.md | Initiate conversation → send messages → handle streaming → manage session |
| manage-models | flows/manage-models.md | List providers → get available models → create model config → validate API key |

---

## Action Index

### Agents

| Action | File | Description |
|--------|------|-------------|
| create-agent | actions/create-agent.md | Create a new AI agent from a description |
| update-agent-persona | actions/update-agent-persona.md | Update agent name, description, and persona |
| update-agent-ai-config | actions/update-agent-ai-config.md | Update model, temperature, system prompt, KB and tool attachments |
| change-agent-status | actions/change-agent-status.md | Enable, disable, or archive an agent |
| delete-agent | actions/delete-agent.md | Permanently delete an agent |
| get-agents | actions/get-agents.md | List agents with filter and pagination |
| get-agent | actions/get-agent.md | Get a single agent's full details |
| publish-agent | actions/publish-agent.md | Publish an agent to the marketplace |

### Knowledge Base

| Action | File | Description |
|--------|------|-------------|
| upload-kb-file | actions/upload-kb-file.md | Upload and index a file (PDF, DOCX, TXT, etc.) |
| ingest-kb-text | actions/ingest-kb-text.md | Ingest raw text content into a KB folder |
| ingest-kb-qa | actions/ingest-kb-qa.md | Ingest Q&A pairs into a KB folder |
| ingest-kb-link | actions/ingest-kb-link.md | Crawl and index a URL into a KB folder |
| create-kb-folder | actions/create-kb-folder.md | Create a KB folder with embedding configuration |
| delete-kb | actions/delete-kb.md | Delete a knowledge base entry |
| test-kb-retrieval | actions/test-kb-retrieval.md | Test retrieval quality with a query against an agent |

### Tools

| Action | File | Description |
|--------|------|-------------|
| create-api-tool | actions/create-api-tool.md | Create or update an API tool with actions |
| create-mcp-tool | actions/create-mcp-tool.md | Create an MCP server tool |
| test-tool-action | actions/test-tool-action.md | Test a specific action on a tool |
| get-tools | actions/get-tools.md | List all tools with optional filtering |
| delete-tool | actions/delete-tool.md | Delete a tool permanently |

### Models

| Action | File | Description |
|--------|------|-------------|
| create-model | actions/create-model.md | Create a new AI model configuration |
| get-models | actions/get-models.md | List all configured AI models |
| get-model | actions/get-model.md | Get a single model's details |
| validate-model | actions/validate-model.md | Validate a model's API key and configuration |
| get-provider-models | actions/get-provider-models.md | Get available models from a provider (seed data) |

### Conversations

| Action | File | Description |
|--------|------|-------------|
| initiate-conversation | actions/initiate-conversation.md | Start or reconnect to a chat session for an agent |
| get-conversations | actions/get-conversations.md | List all chat sessions with filter and pagination |
| delete-conversation | actions/delete-conversation.md | Delete a chat session permanently |

### Chat

| Action | File | Description |
|--------|------|-------------|
| query-lmt | actions/query-lmt.md | Query the language model directly (non-streaming) |
| stream-query-lmt | actions/stream-query-lmt.md | Stream a language model query response |
| chat-agent | actions/chat-agent.md | Send a message to an agent workspace |
| chat-sse | actions/chat-sse.md | Send a message via SSE chat endpoint (Server-Sent Events) |
