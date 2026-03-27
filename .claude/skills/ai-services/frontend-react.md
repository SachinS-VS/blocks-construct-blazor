# AI Services Frontend

## Module Location

All AI services UI lives in `src/modules/ai/`.

> **snake_case in TypeScript types:** Because the API uses snake_case, TypeScript types in this module mirror the API exactly. Do NOT convert to camelCase — this avoids transformation bugs and keeps API contracts readable in code.

---

## Module Structure

```
src/modules/ai/
├── components/
│   ├── agent-card/
│   │   ├── agent-card.tsx           ← Single agent display card
│   │   └── agent-card-skeleton.tsx  ← Loading skeleton
│   ├── chat-window/
│   │   ├── chat-window.tsx          ← Streaming chat UI container
│   │   ├── chat-message.tsx         ← Individual message bubble
│   │   ├── chat-input.tsx           ← Message input bar
│   │   └── typing-indicator.tsx     ← Animated dots while awaiting first token
│   └── kb-upload/
│       ├── kb-upload.tsx            ← File upload with drag-drop
│       ├── kb-upload-progress.tsx   ← Per-file upload progress bar
│       └── kb-processing-status.tsx ← Indexing status (10–30s feedback)
├── pages/
│   ├── agents/
│   │   ├── agents-page.tsx          ← Agent list with search and create button
│   │   └── create-agent-dialog.tsx  ← Modal to create new agent
│   ├── agent-detail/
│   │   ├── agent-detail-page.tsx    ← Single agent full config view
│   │   ├── agent-persona-tab.tsx    ← Name, description, persona fields
│   │   ├── agent-ai-config-tab.tsx  ← Model, temperature, system prompt
│   │   ├── agent-kb-tab.tsx         ← Attached knowledge bases
│   │   └── agent-tools-tab.tsx      ← Attached tools
│   └── chat/
│       ├── chat-page.tsx            ← Full chat interface
│       └── session-sidebar.tsx      ← Conversation session list
├── hooks/
│   └── use-ai.tsx                   ← All AI mutations and queries
├── services/
│   └── ai.service.ts                ← All API call functions
├── types/
│   └── ai.type.ts                   ← TypeScript types (snake_case)
└── index.ts                         ← Public exports
```

---

## TypeScript Types (`types/ai.type.ts`)

Types use snake_case to match the API directly.

```ts
// Agent
export interface Agent {
  agent_id: string
  name: string
  description: string
  persona: string
  status: 'active' | 'inactive' | 'archived'
  model_id: string
  temperature: number
  max_tokens: number
  system_prompt: string
  kb_ids: string[]
  tool_ids: string[]
  project_key: string
  created_at: string
  updated_at: string
}

export interface CreateAgentPayload {
  name: string
  description: string
  project_key: string
}

export interface UpdateAgentPersonaPayload {
  agent_id: string
  name: string
  description: string
  persona: string
  project_key: string
}

export interface UpdateAiConfigPayload {
  agent_id: string
  model_id: string
  temperature: number
  max_tokens: number
  system_prompt: string
  kb_ids: string[]
  tool_ids: string[]
  project_key: string
}

export interface ChangeAgentStatusPayload {
  agent_id: string
  status: 'active' | 'inactive' | 'archived'
  project_key: string
}

export interface GetAgentsPayload {
  limit?: number
  offset?: number
  search?: string
  status?: string
  project_key: string
}

// Knowledge Base
export interface KBFolder {
  kb_folder_id: string
  name: string
  embedding_model: string
  project_key: string
}

export interface KBTextIngestPayload {
  content: string
  title: string
  kb_folder_id: string
  project_key: string
}

export interface KBQAPair {
  question: string
  answer: string
}

export interface KBQAIngestPayload {
  pairs: KBQAPair[]
  kb_folder_id: string
  project_key: string
}

export interface KBLinkIngestPayload {
  url: string
  kb_folder_id: string
  project_key: string
}

export interface RetrievalTestPayload {
  query: string
  top_k?: number
  project_key: string
}

export interface RetrievalResult {
  content: string
  score: number
  source: string
  kb_id: string
}

// Models
export interface AIModel {
  model_id: string
  name: string
  provider: string
  model_name: string
  base_url?: string
  project_key: string
  created_at: string
}

export interface CreateModelPayload {
  name: string
  provider: string
  model_name: string
  api_key: string
  base_url?: string
  project_key: string
}

// Tools
export interface ToolAction {
  name: string
  method: 'GET' | 'POST' | 'PUT' | 'DELETE'
  path: string
  description: string
  parameters: object[]
}

export interface Tool {
  tool_id: string
  name: string
  description: string
  base_url: string
  auth_type: 'None' | 'ApiKey' | 'Bearer' | 'Basic'
  actions: ToolAction[]
  project_key: string
}

export interface CreateApiToolPayload {
  name: string
  description: string
  base_url: string
  auth_type: 'None' | 'ApiKey' | 'Bearer' | 'Basic'
  auth_value?: string
  actions: ToolAction[]
  project_key: string
}

// Conversations
export interface ConversationSession {
  session_id: string
  agent_id: string
  title: string
  message_count: number
  created_at: string
  updated_at: string
}

// Chat
export interface ChatMessage {
  role: 'user' | 'assistant'
  content: string
  timestamp: string
}

export interface ChatPayload {
  message: string
  session_id: string
  project_key: string
}

// Common
export interface AiCommonResponse {
  is_success: boolean
  detail: string
  item_id?: string
  error?: object
}
```

---

## Service Layer (`services/ai.service.ts`)

```ts
import { getAuthHeaders } from '@/lib/auth-headers'

const BASE = `${import.meta.env.VITE_API_BASE_URL}/blocksai-api/v1`

// Agents
export const createAgent = (payload: CreateAgentPayload) =>
  fetch(`${BASE}/agents/create`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify(payload) })

export const getAgents = (payload: GetAgentsPayload) =>
  fetch(`${BASE}/agents/queries`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify(payload) })

export const getAgent = (agent_id: string) =>
  fetch(`${BASE}/agents/query/${agent_id}`, { headers: getAuthHeaders() })

// KB
export const uploadKBFile = (formData: FormData) =>
  fetch(`${BASE}/kb/file`, { method: 'POST', headers: getAuthHeadersNoContentType(), body: formData })

// Streaming chat
export const chatSSE = (session_id: string, payload: { message: string; project_key: string }) =>
  fetch(`${BASE}/chat/${session_id}`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify(payload) })
```

> `getAuthHeaders()` returns `{ Authorization, x-blocks-key, Content-Type }` from app state.
> `getAuthHeadersNoContentType()` omits `Content-Type` so the browser sets multipart boundary for file uploads.

---

## Hooks (`hooks/use-ai.tsx`)

```ts
import { useMutation, useQuery } from '@tanstack/react-query'

// Agents
export const useCreateAgent = () =>
  useMutation({ mutationFn: (payload: CreateAgentPayload) => createAgent(payload) })

export const useGetAgents = (payload: GetAgentsPayload) =>
  useQuery({ queryKey: ['agents', payload], queryFn: () => getAgents(payload) })

export const useGetAgent = (agent_id: string) =>
  useQuery({ queryKey: ['agent', agent_id], queryFn: () => getAgent(agent_id) })

export const useUpdateAgentPersona = () =>
  useMutation({ mutationFn: (payload: UpdateAgentPersonaPayload) => updateAgentPersona(payload) })

export const useUpdateAgentAiConfig = () =>
  useMutation({ mutationFn: (payload: UpdateAiConfigPayload) => updateAgentAiConfig(payload) })

export const useChangeAgentStatus = () =>
  useMutation({ mutationFn: (payload: ChangeAgentStatusPayload) => changeAgentStatus(payload) })

export const useDeleteAgent = () =>
  useMutation({ mutationFn: (agent_id: string) => deleteAgent(agent_id) })

// KB
export const useUploadKBFile = () =>
  useMutation({ mutationFn: (formData: FormData) => uploadKBFile(formData) })

export const useIngestKBText = () =>
  useMutation({ mutationFn: (payload: KBTextIngestPayload) => ingestKBText(payload) })

// Models
export const useGetModels = (project_key: string) =>
  useQuery({ queryKey: ['models', project_key], queryFn: () => getModels(project_key) })

export const useCreateModel = () =>
  useMutation({ mutationFn: (payload: CreateModelPayload) => createModel(payload) })

// Conversations
export const useInitiateConversation = () =>
  useMutation({ mutationFn: (agent_id: string) => initiateConversation(agent_id) })

export const useGetConversations = (payload: GetConversationsPayload) =>
  useQuery({ queryKey: ['conversations', payload], queryFn: () => getConversations(payload) })
```

---

## Component Patterns

### AgentCard (`components/agent-card/agent-card.tsx`)

- Renders agent name, description, status badge, and a "Configure" button
- Status badge: `active` = green, `inactive` = gray, `archived` = red
- Use `<Card>`, `<Badge>`, `<Button>` from ui-kit
- Show `<AgentCardSkeleton />` when loading

```tsx
<Card>
  <CardHeader>
    <CardTitle>{agent.name}</CardTitle>
    <Badge variant={agent.status === 'active' ? 'default' : 'secondary'}>
      {agent.status}
    </Badge>
  </CardHeader>
  <CardContent>
    <p className="text-sm text-muted-foreground">{agent.description}</p>
  </CardContent>
  <CardFooter>
    <Button onClick={() => navigate(`/ai/agents/${agent.agent_id}`)}>Configure</Button>
  </CardFooter>
</Card>
```

---

### ChatWindow (`components/chat-window/chat-window.tsx`)

Streaming chat UI. Rules:

- Use `EventSource` or `fetch` with `ReadableStream` for the SSE endpoint (`POST /chat/{session_id}`)
- Stream tokens into the display as they arrive — do not buffer the full response before rendering
- Show `<TypingIndicator />` while waiting for the first token
- Abort the ongoing stream when the user navigates away (use `AbortController`)
- Auto-scroll to the bottom on each new token
- Show a "Stop generating" button while streaming is active

```tsx
const abortRef = useRef<AbortController | null>(null)

const sendMessage = async (message: string) => {
  abortRef.current = new AbortController()
  setIsStreaming(true)
  setStreamingContent('')

  const res = await fetch(`${BASE}/chat/${session_id}`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify({ message, project_key }),
    signal: abortRef.current.signal,
  })

  const reader = res.body?.getReader()
  const decoder = new TextDecoder()

  while (reader) {
    const { done, value } = await reader.read()
    if (done) break
    const chunk = decoder.decode(value)
    // Parse SSE lines: "data: {"token": "..."}"
    chunk.split('\n').forEach(line => {
      if (line.startsWith('data: ') && line !== 'data: [DONE]') {
        const { token } = JSON.parse(line.slice(6))
        setStreamingContent(prev => prev + token)
      }
    })
  }
  setIsStreaming(false)
}

// Cleanup on navigation
useEffect(() => () => abortRef.current?.abort(), [])
```

---

### KBUpload (`components/kb-upload/kb-upload.tsx`)

File upload with drag-drop. Rules:

- Accept: `.pdf`, `.docx`, `.txt`, `.md`, `.csv`
- Show per-file upload progress bar (use `XMLHttpRequest` or `fetch` with progress tracking)
- After upload, show processing/indexing status — indexing can take 10–30 seconds
- Poll or show a spinner with message: "Indexing your file — this may take up to 30 seconds"
- Use `useDropzone` from `react-dropzone` or build with native drag events

```tsx
const onDrop = useCallback((acceptedFiles: File[]) => {
  acceptedFiles.forEach(file => {
    const formData = new FormData()
    formData.append('file', file)
    formData.append('project_key', projectKey)
    formData.append('kb_folder_id', kbFolderId)
    uploadKBFile(formData)
  })
}, [kbFolderId, projectKey])
```

---

## Pages

### Agents Page (`pages/agents/agents-page.tsx`)

- Fetch agents with `useGetAgents`
- Show search input — debounce and pass `search` to the query
- Show "Create Agent" button → opens `<CreateAgentDialog />`
- Render `<AgentCard />` grid
- Handle loading state with `<AgentCardSkeleton />` grid
- Handle empty state: "No agents yet. Create your first agent."

### Agent Detail Page (`pages/agent-detail/agent-detail-page.tsx`)

- Tabbed layout: Persona | AI Config | Knowledge Base | Tools
- Each tab is its own component
- Fetch agent with `useGetAgent(agent_id)` on mount
- Save button per tab — calls the relevant mutation
- Show unsaved changes warning if navigating away with dirty form

### Chat Page (`pages/chat/chat-page.tsx`)

- Left sidebar: `<SessionSidebar />` — lists conversation sessions
- Main area: `<ChatWindow />` with `<ChatInput />` at the bottom
- On session select: load history, display messages
- On new session: call `initiate-conversation`, then begin chat
- Show streaming indicator while response is being generated

---

## Routing

```ts
// routes/ai.route.tsx
{
  path: '/ai',
  children: [
    { path: 'agents', element: <AgentsPage /> },
    { path: 'agents/:agent_id', element: <AgentDetailPage /> },
    { path: 'chat', element: <ChatPage /> },
    { path: 'chat/:agent_id', element: <ChatPage /> },
  ]
}
```

---

## Rules Specific to AI Module

- Always use `snake_case` type field names — never transform to camelCase
- For file uploads, never set `Content-Type` manually — let the browser set multipart boundary
- For SSE streaming, always attach an `AbortController` and cancel on component unmount
- Show "Indexing..." feedback after KB file upload — do not assume instant completion
- Temperature slider: range 0.0–1.0, step 0.1, label the extremes ("Precise" / "Creative")
- Max tokens input: numeric, min 256, max 8192, step 256
- Never display raw `agent_id` or `model_id` values to the user — always show the `name` field
