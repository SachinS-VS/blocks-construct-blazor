# Flow: chat-flow

## Trigger

User wants to build a chat interface to talk to an AI agent.

> "build a chat page"
> "add a chat window to my app"
> "let users talk to the agent"
> "implement streaming chat"
> "create a conversation interface"

---

## Pre-flight Questions

Before starting, confirm:

1. Which agent should users chat with? (single fixed agent or user-selectable?)
2. Should responses be streamed token-by-token, or delivered all at once?
3. Should conversation history be shown (session list in a sidebar)?
4. Should users be able to start a new conversation or continue existing ones?
5. Is this for end users or internal/admin use?

---

## Flow Steps

### Step 1 — Verify the agent is active

Before building the chat UI, confirm the agent is ready.

```
Action: get-agent
Input:  agent_id = target agent ID
Output: agent details including status and model_id
```

**Branch:**
- `status = "active"` and `model_id` is set → proceed to Step 2
- `status = "inactive"` → call `change-agent-status` to activate
- `model_id` is empty → run `create-agent-flow` to complete configuration

---

### Step 2 — Initiate a conversation session

Create a new session or resume an existing one.

```
Action: initiate-conversation
Input:  agent_id = target agent ID (query param)
Output: session_id (store in component state)
```

Call this when:
- User opens the chat page for the first time
- User clicks "New Conversation"
- User selects an existing session from the sidebar

> The same endpoint starts new sessions and reconnects to existing ones.

---

### Step 3 — Load conversation history (if resuming)

If the user is resuming an existing session, fetch previous sessions for the sidebar.

```
Action: get-conversations
Input:
  agent_id    = target agent ID
  limit       = 20
  offset      = 0
  project_key = $VITE_PROJECT_SLUG

Output: list of sessions with title and message_count
```

Display the session list in the sidebar. Clicking a session triggers Step 2 with that session's `session_id`.

---

### Step 4 — Send a message

When the user submits a message, choose the delivery method:

---

#### Branch A — Streaming (recommended)

```
Action: chat-sse
Endpoint: POST /chat/{session_id}
Input:
  message     = user's message text
  project_key = $VITE_PROJECT_SLUG

Response: SSE stream of tokens until "data: [DONE]"
```

**Streaming implementation:**

1. Append the user's message to the message list immediately (optimistic update)
2. Add a placeholder assistant message with empty content
3. Show `<TypingIndicator />` until the first token arrives
4. Replace the typing indicator with the streaming text as tokens arrive
5. On `data: [DONE]`, finalize the message and hide the streaming state
6. Attach `AbortController` — cancel the stream if user navigates away

```ts
const abortController = new AbortController()

const handleSend = async (message: string) => {
  // Optimistic user message
  setMessages(prev => [...prev, { role: 'user', content: message, timestamp: new Date().toISOString() }])
  setIsStreaming(true)
  setStreamingContent('')

  const res = await fetch(`${BASE}/chat/${session_id}`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify({ message, project_key }),
    signal: abortController.signal,
  })

  const reader = res.body?.getReader()
  const decoder = new TextDecoder()

  while (reader) {
    const { done, value } = await reader.read()
    if (done) break
    decoder.decode(value).split('\n').forEach(line => {
      if (line.startsWith('data: ') && line.trim() !== 'data: [DONE]') {
        const { token } = JSON.parse(line.slice(6))
        setStreamingContent(prev => prev + token)
      }
    })
  }

  // Commit streamed content as final assistant message
  setMessages(prev => [...prev, { role: 'assistant', content: streamingContent, timestamp: new Date().toISOString() }])
  setStreamingContent('')
  setIsStreaming(false)
}
```

---

#### Branch B — Non-streaming

```
Action: chat-agent
Endpoint: POST /ai-agent/chat/{session_id}
Input:
  message     = user's message text
  session_id  = session_id from Step 2
  project_key = $VITE_PROJECT_SLUG

Response: complete response string in "response" field
```

Show a loading spinner while waiting. Add both messages to the list on success.

---

### Step 5 — Handle token refresh on 401

If any chat request returns `401`:

1. Call `refresh-token` from identity-access skill
2. Retry the original request with the new `ACCESS_TOKEN`
3. If refresh also fails → redirect to login

---

### Step 6 — Manage sessions

Users can delete sessions they no longer need.

```
Action: delete-conversation
Input:  session_id = session ID to delete (URL path param)
```

After deletion:
- Remove from session sidebar
- If the deleted session was active, start a new one (Step 2)

---

## Error Handling

| Step | Error | Cause | Action |
|------|-------|-------|--------|
| Step 1 | `404` | Agent not found | Verify `agent_id` — may have been deleted |
| Step 1 | `status = "inactive"` | Agent is disabled | Activate via `change-agent-status` |
| Step 2 | `422` | Agent has no model | Attach a model via `update-agent-ai-config` |
| Step 4 | `404` | Session not found | Session expired — call `initiate-conversation` again |
| Step 4 | `422` | Agent misconfigured | Check that agent has active status and model attached |
| Step 4 | Stream abort | User navigated away | Expected — clean up with `abortController.abort()` |
| Step 4 | Network error mid-stream | Connection dropped | Show "Connection lost" toast; allow retry |
| Step 5 | Refresh returns `401` | Session fully expired | Redirect to login |

---

## Frontend Output

| File | Purpose |
|------|---------|
| `src/modules/ai/pages/chat/chat-page.tsx` | Full chat interface — agent selection, session management, message area |
| `src/modules/ai/pages/chat/session-sidebar.tsx` | Left sidebar — session list, new conversation button |
| `src/modules/ai/components/chat-window/chat-window.tsx` | Message area with streaming support |
| `src/modules/ai/components/chat-window/chat-message.tsx` | Individual message bubble (user/assistant) |
| `src/modules/ai/components/chat-window/chat-input.tsx` | Message input bar with send button and stop button |
| `src/modules/ai/components/chat-window/typing-indicator.tsx` | Animated dots shown while waiting for first token |
| `src/modules/ai/hooks/use-ai.tsx` | `useInitiateConversation`, `useGetConversations`, `useDeleteConversation`, `useChatSSE` |
| `src/modules/ai/services/ai.service.ts` | `initiateConversation()`, `getConversations()`, `chatSSE()`, `chatAgent()` |
| `src/modules/ai/types/ai.type.ts` | `ConversationSession`, `ChatMessage`, `ChatPayload` |
| `src/routes/ai.route.tsx` | `/ai/chat` and `/ai/chat/:agent_id` routes |
