# AI Services Contracts

> **snake_case Convention:** This entire API uses **snake_case** (Python/FastAPI style). Every request field and response field uses `snake_case` — e.g., `project_key`, `agent_id`, `is_success`, `kb_ids`, `tool_ids`, `base_url`, `auth_type`. Never use camelCase when constructing requests or reading responses from this API.

---

## Common Headers (all authenticated requests)

```
Authorization: Bearer $ACCESS_TOKEN
x-blocks-key: $VITE_X_BLOCKS_KEY
Content-Type: application/json
```

---

## Common Response

All mutating endpoints return this shape:

```json
{
  "is_success": true,
  "detail": "Operation completed successfully",
  "item_id": "abc123",
  "error": {}
}
```

| Field | Type | Description |
|-------|------|-------------|
| `is_success` | boolean | Whether the operation succeeded |
| `detail` | string | Human-readable message |
| `item_id` | string | ID of the created or affected resource (if applicable) |
| `error` | object | Error details if `is_success` is false |

---

## Agents

### CreateAgentRequest

```json
{
  "name": "string",
  "description": "string",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | yes | Display name for the agent |
| `description` | string | yes | What this agent is designed to do |
| `project_key` | string | yes | Project identifier from `$VITE_PROJECT_SLUG` |

---

### UpdateAgentPersonaRequest

```json
{
  "agent_id": "string",
  "name": "string",
  "description": "string",
  "persona": "string",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent to update |
| `name` | string | yes | Updated display name |
| `description` | string | yes | Updated description |
| `persona` | string | no | Personality and behavior instructions for the agent |
| `project_key` | string | yes | Project identifier |

---

### UpdateAiConfigurationsRequest

```json
{
  "agent_id": "string",
  "model_id": "string",
  "temperature": 0.7,
  "max_tokens": 2048,
  "system_prompt": "string",
  "kb_ids": ["string"],
  "tool_ids": ["string"],
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent to configure |
| `model_id` | string | yes | ID of the AI model to use |
| `temperature` | float | no | Response creativity (0.0–1.0). Default: `0.7` |
| `max_tokens` | integer | no | Maximum response length. Default: `2048` |
| `system_prompt` | string | no | System-level instructions for the agent |
| `kb_ids` | string[] | no | List of knowledge base IDs to attach |
| `tool_ids` | string[] | no | List of tool IDs to attach |
| `project_key` | string | yes | Project identifier |

---

### ChangeAgentStatusRequest

```json
{
  "agent_id": "string",
  "status": "active",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent |
| `status` | string | yes | One of: `active`, `inactive`, `archived` |
| `project_key` | string | yes | Project identifier |

---

### GetAgentsRequest

```json
{
  "limit": 20,
  "offset": 0,
  "search": "string",
  "status": "string",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `limit` | integer | no | Max results per page. Default: `20` |
| `offset` | integer | no | Pagination offset. Default: `0` |
| `search` | string | no | Search term to filter agents by name |
| `status` | string | no | Filter by status: `active`, `inactive`, `archived` |
| `project_key` | string | yes | Project identifier |

---

### AgentResponse (single agent)

```json
{
  "agent_id": "string",
  "name": "string",
  "description": "string",
  "persona": "string",
  "status": "active",
  "model_id": "string",
  "temperature": 0.7,
  "max_tokens": 2048,
  "system_prompt": "string",
  "kb_ids": ["string"],
  "tool_ids": ["string"],
  "project_key": "string",
  "created_at": "2024-01-01T00:00:00Z",
  "updated_at": "2024-01-01T00:00:00Z"
}
```

---

## Knowledge Base

### KB File Upload (multipart/form-data)

> Use `Content-Type: multipart/form-data` for this endpoint only.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `file` | binary | yes | File to upload (PDF, DOCX, TXT, MD, CSV) |
| `project_key` | string | yes | Project identifier |
| `kb_folder_id` | string | no | Target KB folder ID |
| `chunk_size` | integer | no | Token chunk size for indexing. Default: `512` |

---

### KBTextIngestRequest

```json
{
  "content": "string",
  "title": "string",
  "kb_folder_id": "string",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `content` | string | yes | Raw text to ingest |
| `title` | string | yes | Title/label for this content block |
| `kb_folder_id` | string | yes | Target KB folder ID |
| `project_key` | string | yes | Project identifier |

---

### KBQAIngestRequest

```json
{
  "pairs": [
    {
      "question": "string",
      "answer": "string"
    }
  ],
  "kb_folder_id": "string",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `pairs` | object[] | yes | Array of Q&A pairs |
| `pairs[].question` | string | yes | The question |
| `pairs[].answer` | string | yes | The answer |
| `kb_folder_id` | string | yes | Target KB folder ID |
| `project_key` | string | yes | Project identifier |

---

### KBLinkIngestRequest

```json
{
  "url": "https://example.com",
  "kb_folder_id": "string",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `url` | string | yes | URL to crawl and index |
| `kb_folder_id` | string | yes | Target KB folder ID |
| `project_key` | string | yes | Project identifier |

---

### CreateKBFolderRequest

```json
{
  "name": "string",
  "embedding_model": "string",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | yes | Folder display name |
| `embedding_model` | string | yes | Embedding model to use for this folder's vectors |
| `project_key` | string | yes | Project identifier |

---

### RetrievalTestRequest

```json
{
  "query": "string",
  "top_k": 5,
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `query` | string | yes | Test query to run against the KB |
| `top_k` | integer | no | Number of top results to return. Default: `5` |
| `project_key` | string | yes | Project identifier |

---

### RetrievalTestResponse

```json
{
  "results": [
    {
      "content": "string",
      "score": 0.92,
      "source": "string",
      "kb_id": "string"
    }
  ],
  "is_success": true
}
```

---

## Tools

### CreateApiToolRequest

```json
{
  "name": "string",
  "description": "string",
  "base_url": "https://api.example.com",
  "auth_type": "None",
  "auth_value": "string",
  "actions": [
    {
      "name": "string",
      "method": "GET",
      "path": "/endpoint",
      "description": "string",
      "parameters": []
    }
  ],
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | yes | Tool display name |
| `description` | string | yes | What this tool does |
| `base_url` | string | yes | Base URL for all API calls |
| `auth_type` | string | yes | One of: `None`, `ApiKey`, `Bearer`, `Basic` |
| `auth_value` | string | no | Auth credential value (token, key, etc.) |
| `actions` | object[] | yes | List of API actions this tool exposes |
| `actions[].name` | string | yes | Action name |
| `actions[].method` | string | yes | HTTP method: `GET`, `POST`, `PUT`, `DELETE` |
| `actions[].path` | string | yes | Path relative to `base_url` |
| `actions[].description` | string | yes | What this action does |
| `actions[].parameters` | object[] | no | Parameter definitions for the action |
| `project_key` | string | yes | Project identifier |

---

### CreateMcpToolRequest

```json
{
  "name": "string",
  "description": "string",
  "server_url": "string",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | yes | Tool display name |
| `description` | string | yes | What this MCP server tool does |
| `server_url` | string | yes | MCP server endpoint URL |
| `project_key` | string | yes | Project identifier |

---

## Models

### CreateModelRequest

```json
{
  "name": "string",
  "provider": "OpenAI",
  "model_name": "gpt-4o",
  "api_key": "string",
  "base_url": "string",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | yes | Display name for this model config |
| `provider` | string | yes | One of: `OpenAI`, `Anthropic`, `AzureOpenAI`, `Cohere`, `Ollama` |
| `model_name` | string | yes | The actual model ID (e.g., `gpt-4o`, `claude-3-5-sonnet-20241022`) |
| `api_key` | string | yes | API key for the provider |
| `base_url` | string | no | Custom base URL (required for `AzureOpenAI` and `Ollama`) |
| `project_key` | string | yes | Project identifier |

---

### ModelResponse

```json
{
  "model_id": "string",
  "name": "string",
  "provider": "OpenAI",
  "model_name": "gpt-4o",
  "base_url": "string",
  "project_key": "string",
  "created_at": "2024-01-01T00:00:00Z"
}
```

---

## Conversations

### GetConversationsRequest

```json
{
  "agent_id": "string",
  "limit": 20,
  "offset": 0,
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `agent_id` | string | yes | ID of the agent whose sessions to list |
| `limit` | integer | no | Max results per page. Default: `20` |
| `offset` | integer | no | Pagination offset. Default: `0` |
| `project_key` | string | yes | Project identifier |

---

### InitiateConversationResponse

```json
{
  "session_id": "string",
  "agent_id": "string",
  "created_at": "2024-01-01T00:00:00Z",
  "is_success": true
}
```

---

### ConversationSession

```json
{
  "session_id": "string",
  "agent_id": "string",
  "title": "string",
  "message_count": 12,
  "created_at": "2024-01-01T00:00:00Z",
  "updated_at": "2024-01-01T00:00:00Z"
}
```

---

## Chat

### ChatRequest (non-streaming)

```json
{
  "message": "string",
  "session_id": "string",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | yes | The user's message |
| `session_id` | string | yes | Active session ID from `initiate-conversation` |
| `project_key` | string | yes | Project identifier |

---

### ChatResponse (non-streaming)

```json
{
  "response": "string",
  "session_id": "string",
  "is_success": true
}
```

---

### SSE Chat (streaming)

The `POST /chat/{session_id}` endpoint returns a Server-Sent Events stream.

Request body:
```json
{
  "message": "string",
  "project_key": "string"
}
```

SSE event format:
```
data: {"token": "Hello"}
data: {"token": " world"}
data: [DONE]
```

Each `data:` line is a JSON object with a `token` field. Accumulate tokens to build the full response. The stream ends with `data: [DONE]`.

---

### LMTQueryRequest (direct LLM query)

```json
{
  "message": "string",
  "model_id": "string",
  "project_key": "string"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | yes | The prompt or message to send |
| `model_id` | string | yes | Model configuration ID to use |
| `project_key` | string | yes | Project identifier |

---

## Enumerations

| Enum | Values |
|------|--------|
| Agent Status | `active`, `inactive`, `archived` |
| Auth Type | `None`, `ApiKey`, `Bearer`, `Basic` |
| HTTP Method | `GET`, `POST`, `PUT`, `DELETE` |
| Provider | `OpenAI`, `Anthropic`, `AzureOpenAI`, `Cohere`, `Ollama` |
