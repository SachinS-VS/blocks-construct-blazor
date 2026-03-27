# AI Services — Frontend Guide (Blazor + Tailwind)

This file extends `core/frontend-blazor-tailwind.md` with AI-specific patterns for the ai-services domain.
Always read `core/frontend-blazor-tailwind.md` first, then apply the overrides and additions here.

---

## Module Location

All AI services UI lives in `Modules/AI/`.

> **snake_case in C# models:** The AI API uses snake_case (Python/FastAPI style). Use `[JsonPropertyName("snake_case")]` on every property. Keep PascalCase C# property names but serialize/deserialize as snake_case. Never rename properties to match the API casing in C#.

---

## Module Structure

```
Modules/AI/
├── Components/
│   ├── AgentCard.razor                ← single agent display card
│   ├── AgentCardSkeleton.razor        ← loading skeleton for agent card
│   ├── ChatWindow.razor               ← streaming chat UI container
│   ├── ChatMessage.razor              ← individual message bubble
│   ├── ChatInput.razor                ← message input bar
│   ├── TypingIndicator.razor          ← animated indicator while awaiting first token
│   ├── KBUpload.razor                 ← file upload with drag-drop
│   ├── KBUploadProgress.razor         ← per-file upload progress bar
│   └── KBProcessingStatus.razor       ← indexing status feedback (10–30s)
├── Pages/
│   ├── AgentsPage.razor               ← agent list with search and create button
│   ├── AgentDetailPage.razor          ← single agent full config view (tabbed)
│   ├── PersonaTab.razor               ← name, description, persona fields
│   ├── AiConfigTab.razor              ← model, temperature, system prompt
│   ├── KBTab.razor                    ← attached knowledge bases
│   ├── ToolsTab.razor                 ← attached tools
│   ├── ChatPage.razor                 ← full chat interface
│   └── SessionSidebar.razor           ← conversation session list (left drawer)
├── Services/
│   └── AiService.cs                   ← all AI API call methods
└── Models/
    ├── AiModels.cs                    ← C# models with [JsonPropertyName]
    └── AiValidators.cs                ← FluentValidation validators
```

---

## C# Models (`Models/AiModels.cs`)

All properties use `[JsonPropertyName]` to map PascalCase C# names to snake_case API fields.

```csharp
using System.Text.Json.Serialization;

// ───────────────────────── Agent ─────────────────────────

public class Agent
{
    [JsonPropertyName("agent_id")]
    public string AgentId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("persona")]
    public string Persona { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // "active", "inactive", "archived"

    [JsonPropertyName("model_id")]
    public string ModelId { get; set; } = string.Empty;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("system_prompt")]
    public string SystemPrompt { get; set; } = string.Empty;

    [JsonPropertyName("kb_ids")]
    public List<string> KbIds { get; set; } = new();

    [JsonPropertyName("tool_ids")]
    public List<string> ToolIds { get; set; } = new();

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;
}

public class CreateAgentPayload
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

public class UpdateAgentPersonaPayload
{
    [JsonPropertyName("agent_id")]
    public string AgentId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("persona")]
    public string Persona { get; set; } = string.Empty;

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

public class UpdateAiConfigPayload
{
    [JsonPropertyName("agent_id")]
    public string AgentId { get; set; } = string.Empty;

    [JsonPropertyName("model_id")]
    public string ModelId { get; set; } = string.Empty;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.7;

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 2048;

    [JsonPropertyName("system_prompt")]
    public string SystemPrompt { get; set; } = string.Empty;

    [JsonPropertyName("kb_ids")]
    public List<string> KbIds { get; set; } = new();

    [JsonPropertyName("tool_ids")]
    public List<string> ToolIds { get; set; } = new();

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

public class ChangeAgentStatusPayload
{
    [JsonPropertyName("agent_id")]
    public string AgentId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // "active", "inactive", "archived"

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

public class GetAgentsPayload
{
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("offset")]
    public int? Offset { get; set; }

    [JsonPropertyName("search")]
    public string? Search { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

// ───────────────────────── Knowledge Base ─────────────────────────

public class KBFolder
{
    [JsonPropertyName("kb_folder_id")]
    public string KbFolderId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("embedding_model")]
    public string EmbeddingModel { get; set; } = string.Empty;

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

public class KBTextIngestPayload
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("kb_folder_id")]
    public string KbFolderId { get; set; } = string.Empty;

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

public class KBQAPair
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;
}

public class KBQAIngestPayload
{
    [JsonPropertyName("pairs")]
    public List<KBQAPair> Pairs { get; set; } = new();

    [JsonPropertyName("kb_folder_id")]
    public string KbFolderId { get; set; } = string.Empty;

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

public class KBLinkIngestPayload
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("kb_folder_id")]
    public string KbFolderId { get; set; } = string.Empty;

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

public class RetrievalTestPayload
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

public class RetrievalResult
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("kb_id")]
    public string KbId { get; set; } = string.Empty;
}

// ───────────────────────── Models ─────────────────────────

public class AIModel
{
    [JsonPropertyName("model_id")]
    public string ModelId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("model_name")]
    public string ModelName { get; set; } = string.Empty;

    [JsonPropertyName("base_url")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;
}

public class CreateModelPayload
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("model_name")]
    public string ModelName { get; set; } = string.Empty;

    [JsonPropertyName("api_key")]
    public string ApiKey { get; set; } = string.Empty;

    [JsonPropertyName("base_url")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

// ───────────────────────── Tools ─────────────────────────

public class ToolAction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty; // "GET", "POST", "PUT", "DELETE"

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public List<object> Parameters { get; set; } = new();
}

public class Tool
{
    [JsonPropertyName("tool_id")]
    public string ToolId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("base_url")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("auth_type")]
    public string AuthType { get; set; } = string.Empty; // "None", "ApiKey", "Bearer", "Basic"

    [JsonPropertyName("actions")]
    public List<ToolAction> Actions { get; set; } = new();

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

public class CreateApiToolPayload
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("base_url")]
    public string BaseUrl { get; set; } = string.Empty;

    [JsonPropertyName("auth_type")]
    public string AuthType { get; set; } = string.Empty; // "None", "ApiKey", "Bearer", "Basic"

    [JsonPropertyName("auth_value")]
    public string? AuthValue { get; set; }

    [JsonPropertyName("actions")]
    public List<ToolAction> Actions { get; set; } = new();

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

// ───────────────────────── Conversations ─────────────────────────

public class ConversationSession
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("agent_id")]
    public string AgentId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message_count")]
    public int MessageCount { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;
}

// ───────────────────────── Chat ─────────────────────────

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty; // "user", "assistant"

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}

public class ChatPayload
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("project_key")]
    public string ProjectKey { get; set; } = string.Empty;
}

// ───────────────────────── Common ─────────────────────────

public class AiCommonResponse
{
    [JsonPropertyName("is_success")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = string.Empty;

    [JsonPropertyName("item_id")]
    public string? ItemId { get; set; }

    [JsonPropertyName("error")]
    public object? Error { get; set; }
}
```

---

## Service Layer (`Services/AiService.cs`)

The AI service uses its own `HttpClient` instance because the blocksai-api has a different base URL from the main Blocks API. Do **not** use the DI-registered `HttpClient` with `TokenDelegatingHandler` — instead, construct requests with explicit headers.

```csharp
using System.Net.Http.Json;
using System.Text.Json;

public interface IAiService
{
    // Agents
    Task<HttpResponseMessage> CreateAgentAsync(CreateAgentPayload payload);
    Task<HttpResponseMessage> GetAgentsAsync(GetAgentsPayload payload);
    Task<HttpResponseMessage> GetAgentAsync(string agentId);
    Task<HttpResponseMessage> UpdateAgentPersonaAsync(UpdateAgentPersonaPayload payload);
    Task<HttpResponseMessage> UpdateAiConfigAsync(UpdateAiConfigPayload payload);
    Task<HttpResponseMessage> ChangeAgentStatusAsync(ChangeAgentStatusPayload payload);
    Task<HttpResponseMessage> DeleteAgentAsync(string agentId);

    // Knowledge Base
    Task<HttpResponseMessage> CreateKBFolderAsync(KBFolder payload);
    Task<HttpResponseMessage> UploadKBFileAsync(MultipartFormDataContent formData);
    Task<HttpResponseMessage> IngestKBTextAsync(KBTextIngestPayload payload);
    Task<HttpResponseMessage> IngestKBQAAsync(KBQAIngestPayload payload);
    Task<HttpResponseMessage> IngestKBLinkAsync(KBLinkIngestPayload payload);
    Task<HttpResponseMessage> DeleteKBAsync(string kbId);
    Task<HttpResponseMessage> TestKBRetrievalAsync(RetrievalTestPayload payload);

    // Models
    Task<HttpResponseMessage> GetModelsAsync(string projectKey);
    Task<HttpResponseMessage> GetModelAsync(string modelId);
    Task<HttpResponseMessage> CreateModelAsync(CreateModelPayload payload);

    // Tools
    Task<HttpResponseMessage> GetToolsAsync(string projectKey);
    Task<HttpResponseMessage> CreateApiToolAsync(CreateApiToolPayload payload);
    Task<HttpResponseMessage> DeleteToolAsync(string toolId);

    // Conversations
    Task<HttpResponseMessage> InitiateConversationAsync(string agentId, string projectKey);
    Task<HttpResponseMessage> GetConversationsAsync(string agentId, string projectKey, int limit = 20, int offset = 0);
    Task<HttpResponseMessage> DeleteConversationAsync(string sessionId);

    // Chat
    Task<HttpResponseMessage> ChatAsync(ChatPayload payload);
    Task<Stream> ChatStreamAsync(string sessionId, string message, string projectKey, CancellationToken cancellationToken);
}

public class AiService : IAiService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _blocksKey;
    private readonly AuthState _authState;

    public AiService(AppSettings settings, AuthState authState)
    {
        _http = new HttpClient();
        _baseUrl = $"{settings.ApiBaseUrl}/blocksai-api/v1";
        _blocksKey = settings.BlocksKey;
        _authState = authState;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, object? body = null)
    {
        var request = new HttpRequestMessage(method, $"{_baseUrl}{path}");
        request.Headers.Add("Authorization", $"Bearer {_authState.AccessToken}");
        request.Headers.Add("x-blocks-key", _blocksKey);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
        }
        return request;
    }

    // ── Agents ──

    public Task<HttpResponseMessage> CreateAgentAsync(CreateAgentPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, "/agents/create", payload));

    public Task<HttpResponseMessage> GetAgentsAsync(GetAgentsPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, "/agents/queries", payload));

    public Task<HttpResponseMessage> GetAgentAsync(string agentId) =>
        _http.SendAsync(CreateRequest(HttpMethod.Get, $"/agents/query/{agentId}"));

    public Task<HttpResponseMessage> UpdateAgentPersonaAsync(UpdateAgentPersonaPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Put, "/agents/persona", payload));

    public Task<HttpResponseMessage> UpdateAiConfigAsync(UpdateAiConfigPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Put, "/agents/ai-config", payload));

    public Task<HttpResponseMessage> ChangeAgentStatusAsync(ChangeAgentStatusPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Put, "/agents/status", payload));

    public Task<HttpResponseMessage> DeleteAgentAsync(string agentId) =>
        _http.SendAsync(CreateRequest(HttpMethod.Delete, $"/agents/{agentId}"));

    // ── Knowledge Base ──

    public Task<HttpResponseMessage> CreateKBFolderAsync(KBFolder payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, "/kb/folders/create", payload));

    public async Task<HttpResponseMessage> UploadKBFileAsync(MultipartFormDataContent formData)
    {
        // Never set Content-Type manually — let MultipartFormDataContent set the boundary
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/kb/file")
        {
            Content = formData
        };
        request.Headers.Add("Authorization", $"Bearer {_authState.AccessToken}");
        request.Headers.Add("x-blocks-key", _blocksKey);
        return await _http.SendAsync(request);
    }

    public Task<HttpResponseMessage> IngestKBTextAsync(KBTextIngestPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, "/kb/text", payload));

    public Task<HttpResponseMessage> IngestKBQAAsync(KBQAIngestPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, "/kb/qa", payload));

    public Task<HttpResponseMessage> IngestKBLinkAsync(KBLinkIngestPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, "/kb/link", payload));

    public Task<HttpResponseMessage> DeleteKBAsync(string kbId) =>
        _http.SendAsync(CreateRequest(HttpMethod.Delete, $"/kb/{kbId}"));

    public Task<HttpResponseMessage> TestKBRetrievalAsync(RetrievalTestPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, "/kb/retrieval-test", payload));

    // ── Models ──

    public Task<HttpResponseMessage> GetModelsAsync(string projectKey) =>
        _http.SendAsync(CreateRequest(HttpMethod.Get, $"/models?project_key={projectKey}"));

    public Task<HttpResponseMessage> GetModelAsync(string modelId) =>
        _http.SendAsync(CreateRequest(HttpMethod.Get, $"/models/{modelId}"));

    public Task<HttpResponseMessage> CreateModelAsync(CreateModelPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, "/models/create", payload));

    // ── Tools ──

    public Task<HttpResponseMessage> GetToolsAsync(string projectKey) =>
        _http.SendAsync(CreateRequest(HttpMethod.Get, $"/tools?project_key={projectKey}"));

    public Task<HttpResponseMessage> CreateApiToolAsync(CreateApiToolPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, "/tools/api/create", payload));

    public Task<HttpResponseMessage> DeleteToolAsync(string toolId) =>
        _http.SendAsync(CreateRequest(HttpMethod.Delete, $"/tools/{toolId}"));

    // ── Conversations ──

    public Task<HttpResponseMessage> InitiateConversationAsync(string agentId, string projectKey) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, $"/conversations/initiate",
            new { agent_id = agentId, project_key = projectKey }));

    public Task<HttpResponseMessage> GetConversationsAsync(string agentId, string projectKey, int limit = 20, int offset = 0) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, "/conversations/queries",
            new { agent_id = agentId, project_key = projectKey, limit, offset }));

    public Task<HttpResponseMessage> DeleteConversationAsync(string sessionId) =>
        _http.SendAsync(CreateRequest(HttpMethod.Delete, $"/conversations/{sessionId}"));

    // ── Chat ──

    public Task<HttpResponseMessage> ChatAsync(ChatPayload payload) =>
        _http.SendAsync(CreateRequest(HttpMethod.Post, $"/chat/{payload.SessionId}", payload));

    public async Task<Stream> ChatStreamAsync(string sessionId, string message, string projectKey, CancellationToken cancellationToken)
    {
        var request = CreateRequest(HttpMethod.Post, $"/chat/{sessionId}",
            new { message, project_key = projectKey });
        var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<IAiService, AiService>();
```

---

## Component Patterns

### AgentCard (`Components/AgentCard.razor`)

```razor
@inject ILocalizationService Localizer
@inject NavigationManager Navigation

<div class="card flex flex-col">
    <div class="flex items-start justify-between p-4">
        <h3 class="text-base font-semibold text-gray-900 truncate">@Agent.Name</h3>
        <span class="@GetStatusBadge()">
            @Localizer[$"ai.agent.status.{Agent.Status}"]
        </span>
    </div>
    <div class="flex-1 px-4 pb-2">
        <p class="text-sm text-gray-500 line-clamp-2">@Agent.Description</p>
    </div>
    <div class="border-t border-gray-100 px-4 py-3">
        <button class="text-sm font-medium text-primary hover:text-primary/80"
                @onclick="@(() => Navigation.NavigateTo($"/ai/agents/{Agent.AgentId}"))">
            @Localizer["ai.agent.configure"]
        </button>
    </div>
</div>

@code {
    [Parameter, EditorRequired] public Agent Agent { get; set; } = default!;

    private string GetStatusBadge() => Agent.Status switch
    {
        "active" => "badge-success",
        "inactive" => "badge-gray",
        "archived" => "badge-danger",
        _ => "badge-gray"
    };
}
```

### AgentCardSkeleton (`Components/AgentCardSkeleton.razor`)

```razor
<div class="card animate-pulse">
    <div class="flex items-start justify-between p-4">
        <div class="h-5 w-3/5 rounded bg-gray-200"></div>
        <div class="h-5 w-16 rounded bg-gray-200"></div>
    </div>
    <div class="px-4 pb-2 space-y-2">
        <div class="h-4 w-full rounded bg-gray-200"></div>
        <div class="h-4 w-4/5 rounded bg-gray-200"></div>
    </div>
    <div class="border-t border-gray-100 px-4 py-3">
        <div class="h-5 w-20 rounded bg-gray-200"></div>
    </div>
</div>
```

---

### ChatWindow (`Components/ChatWindow.razor`)

Streaming SSE chat using `HttpClient.SendAsync` with `HttpCompletionOption.ResponseHeadersRead`, then `StreamReader` to parse SSE lines.

```razor
@inject IAiService AiService
@inject ILocalizationService Localizer
@implements IDisposable

<div class="flex-1 overflow-y-auto p-4 space-y-3" @ref="_messagesContainer">
    @foreach (var msg in Messages)
    {
        <ChatMessage Message="msg" />
    }

    @if (_isStreaming && !string.IsNullOrEmpty(_streamingContent))
    {
        <ChatMessage Message="@(new ChatMessage { Role = "assistant", Content = _streamingContent })" />
    }

    @if (_isAwaitingFirstToken)
    {
        <TypingIndicator />
    }
</div>

@if (_isStreaming)
{
    <div class="px-4 pb-2">
        <button class="flex items-center gap-1.5 text-sm text-red-600 hover:text-red-700"
                @onclick="StopGenerating">
            @* Heroicon: mini/stop *@
            <svg class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                <path d="M5.25 3A2.25 2.25 0 0 0 3 5.25v9.5A2.25 2.25 0 0 0 5.25 17h9.5A2.25 2.25 0 0 0 17 14.75v-9.5A2.25 2.25 0 0 0 14.75 3h-9.5Z" />
            </svg>
            @Localizer["ai.chat.stopGenerating"]
        </button>
    </div>
}

@code {
    [Parameter] public List<ChatMessage> Messages { get; set; } = new();
    [Parameter] public string SessionId { get; set; } = string.Empty;
    [Parameter] public string ProjectKey { get; set; } = string.Empty;
    [Parameter] public EventCallback<ChatMessage> OnMessageReceived { get; set; }

    private ElementReference _messagesContainer;
    private CancellationTokenSource? _cts;
    private bool _isStreaming;
    private bool _isAwaitingFirstToken;
    private string _streamingContent = string.Empty;

    public async Task SendMessageAsync(string message)
    {
        _cts = new CancellationTokenSource();
        _isStreaming = true;
        _isAwaitingFirstToken = true;
        _streamingContent = string.Empty;
        StateHasChanged();

        try
        {
            using var stream = await AiService.ChatStreamAsync(
                SessionId, message, ProjectKey, _cts.Token);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !_cts.Token.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(_cts.Token);
                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("data: ") && line != "data: [DONE]")
                {
                    var json = line.Substring(6);
                    var parsed = JsonSerializer.Deserialize<JsonElement>(json);
                    if (parsed.TryGetProperty("token", out var tokenElement))
                    {
                        var token = tokenElement.GetString() ?? string.Empty;
                        _isAwaitingFirstToken = false;
                        _streamingContent += token;
                        StateHasChanged();
                    }
                }
                else if (line == "data: [DONE]")
                {
                    break;
                }
            }

            // Finalize: add completed message
            var assistantMessage = new ChatMessage
            {
                Role = "assistant",
                Content = _streamingContent,
                Timestamp = DateTime.UtcNow.ToString("o")
            };
            await OnMessageReceived.InvokeAsync(assistantMessage);
        }
        catch (OperationCanceledException)
        {
            // User cancelled — keep partial content
        }
        finally
        {
            _isStreaming = false;
            _isAwaitingFirstToken = false;
            _streamingContent = string.Empty;
            StateHasChanged();
        }
    }

    private void StopGenerating()
    {
        _cts?.Cancel();
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

### TypingIndicator (`Components/TypingIndicator.razor`)

```razor
@inject ILocalizationService Localizer

<div class="flex items-center gap-2 px-2 py-1">
    <span class="inline-block h-4 w-4 animate-spin rounded-full border-2 border-gray-300 border-t-primary"></span>
    <span class="text-xs text-gray-500">@Localizer["ai.chat.thinking"]</span>
</div>
```

### ChatInput (`Components/ChatInput.razor`)

```razor
@inject ILocalizationService Localizer

<div class="flex items-center gap-2 border-t border-gray-200 bg-white p-3">
    <input type="text"
           @bind="_message"
           @bind:event="oninput"
           @onkeydown="HandleKeyDown"
           placeholder="@Localizer["ai.chat.placeholder"]"
           disabled="@Disabled"
           class="input flex-1" />
    <button class="btn-primary"
            @onclick="Send"
            disabled="@(Disabled || string.IsNullOrWhiteSpace(_message))">
        @* Heroicon: mini/paper-airplane *@
        <svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
            <path d="M3.105 2.288a.75.75 0 0 0-.826.95l1.414 4.926A1.5 1.5 0 0 0 5.135 9.25h6.115a.75.75 0 0 1 0 1.5H5.135a1.5 1.5 0 0 0-1.442 1.086L2.28 16.762a.75.75 0 0 0 .826.95l15.5-5.5a.75.75 0 0 0 0-1.424l-15.5-5.5Z" />
        </svg>
    </button>
</div>

@code {
    [Parameter] public EventCallback<string> OnSend { get; set; }
    [Parameter] public bool Disabled { get; set; }

    private string _message = string.Empty;

    private async Task Send()
    {
        if (string.IsNullOrWhiteSpace(_message)) return;
        var msg = _message;
        _message = string.Empty;
        await OnSend.InvokeAsync(msg);
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await Send();
        }
    }
}
```

### ChatMessage (`Components/ChatMessage.razor`)

```razor
<div class="flex @(Message.Role == "user" ? "justify-end" : "justify-start")">
    <div class="@MessageClass() max-w-[70%] rounded-lg px-4 py-2.5">
        <p class="text-sm whitespace-pre-wrap">@Message.Content</p>
    </div>
</div>

@code {
    [Parameter, EditorRequired] public ChatMessage Message { get; set; } = default!;

    private string MessageClass() => Message.Role == "user"
        ? "bg-primary text-white"
        : "bg-gray-100 text-gray-900";
}
```

---

### KBUpload (`Components/KBUpload.razor`)

```razor
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject ToastService Toast

<label class="block cursor-pointer rounded-lg border-2 border-dashed border-gray-300 p-8 text-center transition hover:border-primary hover:bg-primary/5">
    <InputFile OnChange="OnFilesSelected" multiple
               accept=".pdf,.docx,.txt,.md,.csv"
               class="sr-only" />
    @* Heroicon: outline/cloud-arrow-up *@
    <svg class="mx-auto h-10 w-10 text-primary" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" d="M12 16.5V9.75m0 0 3 3m-3-3-3 3M6.75 19.5a4.5 4.5 0 0 1-1.41-8.775 5.25 5.25 0 0 1 10.233-2.33 3 3 0 0 1 3.758 3.848A3.752 3.752 0 0 1 18 19.5H6.75Z" />
    </svg>
    <p class="mt-2 text-sm font-medium text-gray-700">@Localizer["ai.kb.dropFilesHere"]</p>
    <p class="mt-1 text-xs text-gray-500">@Localizer["ai.kb.acceptedFormats"]</p>
</label>

@foreach (var upload in _uploads)
{
    <KBUploadProgress FileName="@upload.FileName" Progress="@upload.Progress" />
}

@if (_isIndexing)
{
    <KBProcessingStatus />
}

@code {
    [Parameter] public string KbFolderId { get; set; } = string.Empty;
    [Parameter] public string ProjectKey { get; set; } = string.Empty;
    [Parameter] public EventCallback OnUploadComplete { get; set; }

    private List<UploadItem> _uploads = new();
    private bool _isIndexing;

    private async Task OnFilesSelected(InputFileChangeEventArgs e)
    {
        foreach (var file in e.GetMultipleFiles())
        {
            var item = new UploadItem(file.Name, 0);
            _uploads.Add(item);
            StateHasChanged();

            try
            {
                var formData = new MultipartFormDataContent();
                var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
                formData.Add(new StreamContent(stream), "file", file.Name);
                formData.Add(new StringContent(ProjectKey), "project_key");
                formData.Add(new StringContent(KbFolderId), "kb_folder_id");

                item.Progress = 50;
                StateHasChanged();

                var response = await AiService.UploadKBFileAsync(formData);
                response.EnsureSuccessStatusCode();

                item.Progress = 100;
                StateHasChanged();
            }
            catch
            {
                item.Progress = -1; // error state
                Toast.ShowError(Localizer["ai.kb.uploadFailed"]);
            }
        }

        // Show indexing feedback
        _isIndexing = true;
        StateHasChanged();

        // Indexing takes 10–30 seconds — show feedback then notify parent
        await Task.Delay(5000);
        _isIndexing = false;
        _uploads.Clear();
        StateHasChanged();
        await OnUploadComplete.InvokeAsync();
    }

    private class UploadItem
    {
        public string FileName { get; set; }
        public int Progress { get; set; }
        public UploadItem(string fileName, int progress) { FileName = fileName; Progress = progress; }
    }
}
```

### KBUploadProgress (`Components/KBUploadProgress.razor`)

```razor
@inject ILocalizationService Localizer

<div class="flex items-center gap-2 py-1">
    @* Heroicon: mini/document *@
    <svg class="h-4 w-4 shrink-0 text-gray-400" viewBox="0 0 20 20" fill="currentColor">
        <path d="M3 3.5A1.5 1.5 0 0 1 4.5 2h6.879a1.5 1.5 0 0 1 1.06.44l4.122 4.12A1.5 1.5 0 0 1 17 7.622V16.5a1.5 1.5 0 0 1-1.5 1.5h-11A1.5 1.5 0 0 1 3 16.5v-13Z" />
    </svg>
    <span class="flex-1 truncate text-sm text-gray-700">@FileName</span>
    @if (Progress >= 0)
    {
        <div class="h-1.5 w-28 overflow-hidden rounded-full bg-gray-200">
            <div class="h-full rounded-full bg-primary transition-all"
                 style="width: @(Progress)%"></div>
        </div>
    }
    else
    {
        @* Heroicon: mini/exclamation-circle *@
        <svg class="h-4 w-4 text-red-500" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M18 10a8 8 0 1 1-16 0 8 8 0 0 1 16 0Zm-8-5a.75.75 0 0 1 .75.75v4.5a.75.75 0 0 1-1.5 0v-4.5A.75.75 0 0 1 10 5Zm0 10a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z" clip-rule="evenodd" />
        </svg>
    }
</div>

@code {
    [Parameter] public string FileName { get; set; } = string.Empty;
    [Parameter] public int Progress { get; set; }
}
```

### KBProcessingStatus (`Components/KBProcessingStatus.razor`)

```razor
@inject ILocalizationService Localizer

<div class="mt-2 flex items-center gap-2 rounded-md bg-blue-50 border border-blue-200 px-4 py-3">
    <span class="inline-block h-4 w-4 animate-spin rounded-full border-2 border-blue-300 border-t-blue-600"></span>
    <p class="text-sm text-blue-700">@Localizer["ai.kb.indexingMessage"]</p>
</div>
```

> The `ai.kb.indexingMessage` key should resolve to something like "Indexing your file — this may take up to 30 seconds".

---

## Page Patterns

### AgentsPage (`Pages/AgentsPage.razor`)

```razor
@page "/ai/agents"
@attribute [Authorize]
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject AppSettings Settings
@inject ToastService Toast

<h1 class="mb-6 text-2xl font-bold text-gray-900">@Localizer["ai.agents.title"]</h1>

<div class="mb-6 flex items-center justify-between gap-4">
    <div class="relative max-w-sm flex-1">
        @* Heroicon: mini/magnifying-glass *@
        <svg class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M9 3.5a5.5 5.5 0 1 0 0 11 5.5 5.5 0 0 0 0-11ZM2 9a7 7 0 1 1 12.452 4.391l3.328 3.329a.75.75 0 1 1-1.06 1.06l-3.329-3.328A7 7 0 0 1 2 9Z" clip-rule="evenodd" />
        </svg>
        <input type="text"
               @bind="_search"
               @bind:after="LoadAgents"
               placeholder="@Localizer["ai.agents.searchPlaceholder"]"
               class="input pl-9" />
    </div>
    <button class="btn-primary flex items-center gap-1.5"
            @onclick="OpenCreateModal">
        @* Heroicon: mini/plus *@
        <svg class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
            <path d="M10.75 4.75a.75.75 0 0 0-1.5 0v4.5h-4.5a.75.75 0 0 0 0 1.5h4.5v4.5a.75.75 0 0 0 1.5 0v-4.5h4.5a.75.75 0 0 0 0-1.5h-4.5v-4.5Z" />
        </svg>
        @Localizer["ai.agents.create"]
    </button>
</div>

@if (_loading)
{
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        @for (int i = 0; i < 6; i++)
        {
            <AgentCardSkeleton />
        }
    </div>
}
else if (_agents.Count == 0)
{
    <div class="rounded-md bg-blue-50 border border-blue-200 px-4 py-8 text-center">
        <p class="text-sm text-blue-700">@Localizer["ai.agents.emptyState"]</p>
    </div>
}
else
{
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        @foreach (var agent in _agents)
        {
            <AgentCard Agent="agent" />
        }
    </div>
}

@* Create Agent Modal *@
<Modal IsOpen="_showCreateModal" Title="@Localizer["ai.agents.createTitle"]" OnClose="CloseCreateModal">
    <EditForm Model="_createModel" OnValidSubmit="SubmitCreate" class="space-y-4">
        <DataAnnotationsValidator />
        <div>
            <label class="label">@Localizer["ai.agent.nameLabel"]</label>
            <InputText @bind-Value="_createModel.Name" class="input" />
            <ValidationMessage For="@(() => _createModel.Name)" class="text-red-500 text-xs mt-1" />
        </div>
        <div>
            <label class="label">@Localizer["ai.agent.descriptionLabel"]</label>
            <InputTextArea @bind-Value="_createModel.Description" class="input" rows="3" />
            <ValidationMessage For="@(() => _createModel.Description)" class="text-red-500 text-xs mt-1" />
        </div>
        <div class="flex justify-end gap-3 pt-2">
            <button type="button" class="btn-outline" @onclick="CloseCreateModal" disabled="@_submitting">
                @Localizer["common.cancel"]
            </button>
            <button type="submit" class="btn-primary" disabled="@_submitting">
                @if (_submitting)
                {
                    <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
                }
                @Localizer["common.create"]
            </button>
        </div>
    </EditForm>
</Modal>

@code {
    private List<Agent> _agents = new();
    private string _search = string.Empty;
    private bool _loading = true;
    private bool _showCreateModal;
    private bool _submitting;
    private CreateAgentPayload _createModel = new();

    protected override async Task OnInitializedAsync() => await LoadAgents();

    private async Task LoadAgents()
    {
        _loading = true;
        StateHasChanged();

        var response = await AiService.GetAgentsAsync(new GetAgentsPayload
        {
            Search = string.IsNullOrWhiteSpace(_search) ? null : _search,
            ProjectKey = Settings.ProjectSlug
        });

        if (response.IsSuccessStatusCode)
        {
            _agents = await response.Content.ReadFromJsonAsync<List<Agent>>() ?? new();
        }

        _loading = false;
        StateHasChanged();
    }

    private void OpenCreateModal()
    {
        _createModel = new();
        _showCreateModal = true;
    }

    private void CloseCreateModal() => _showCreateModal = false;

    private async Task SubmitCreate()
    {
        _submitting = true;
        _createModel.ProjectKey = Settings.ProjectSlug;

        var response = await AiService.CreateAgentAsync(_createModel);
        if (response.IsSuccessStatusCode)
        {
            Toast.ShowSuccess(Localizer["ai.agent.created"]);
            _showCreateModal = false;
            await LoadAgents();
        }
        else
        {
            Toast.ShowError(Localizer["common.error"]);
        }

        _submitting = false;
    }
}
```

### AgentDetailPage (`Pages/AgentDetailPage.razor`)

```razor
@page "/ai/agents/{AgentId}"
@attribute [Authorize]
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject NavigationManager Navigation
@inject AppSettings Settings

@if (_loading)
{
    <div class="animate-pulse space-y-4">
        <div class="h-8 w-2/5 rounded bg-gray-200"></div>
        <div class="h-96 w-full rounded bg-gray-200"></div>
    </div>
}
else if (_agent is not null)
{
    <h1 class="mb-6 text-2xl font-bold text-gray-900">@_agent.Name</h1>

    @* Tab navigation *@
    <div class="border-b border-gray-200 mb-6">
        <nav class="-mb-px flex gap-6">
            @foreach (var tab in _tabs)
            {
                <button class="@TabClass(tab.Key)"
                        @onclick="() => _activeTab = tab.Key">
                    @Localizer[tab.LabelKey]
                </button>
            }
        </nav>
    </div>

    @* Tab content *@
    <div>
        @switch (_activeTab)
        {
            case "persona":
                <PersonaTab Agent="_agent" OnSaved="ReloadAgent" />
                break;
            case "aiConfig":
                <AiConfigTab Agent="_agent" OnSaved="ReloadAgent" />
                break;
            case "kb":
                <KBTab Agent="_agent" />
                break;
            case "tools":
                <ToolsTab Agent="_agent" />
                break;
        }
    </div>
}

@code {
    [Parameter] public string AgentId { get; set; } = string.Empty;

    private Agent? _agent;
    private bool _loading = true;
    private string _activeTab = "persona";

    private record TabItem(string Key, string LabelKey);

    private readonly List<TabItem> _tabs = new()
    {
        new("persona", "ai.agent.tab.persona"),
        new("aiConfig", "ai.agent.tab.aiConfig"),
        new("kb", "ai.agent.tab.knowledgeBase"),
        new("tools", "ai.agent.tab.tools"),
    };

    protected override async Task OnInitializedAsync() => await ReloadAgent();

    private async Task ReloadAgent()
    {
        _loading = true;
        StateHasChanged();

        var response = await AiService.GetAgentAsync(AgentId);
        if (response.IsSuccessStatusCode)
        {
            _agent = await response.Content.ReadFromJsonAsync<Agent>();
        }

        _loading = false;
        StateHasChanged();
    }

    private string TabClass(string tabKey) => tabKey == _activeTab
        ? "whitespace-nowrap border-b-2 border-primary px-1 pb-3 text-sm font-medium text-primary"
        : "whitespace-nowrap border-b-2 border-transparent px-1 pb-3 text-sm font-medium text-gray-500 hover:border-gray-300 hover:text-gray-700";
}
```

### PersonaTab (`Pages/PersonaTab.razor`)

```razor
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject ToastService Toast
@inject AppSettings Settings

<EditForm Model="_form" OnValidSubmit="Save" class="max-w-2xl space-y-4">
    <DataAnnotationsValidator />

    <div>
        <label class="label">@Localizer["ai.agent.nameLabel"]</label>
        <InputText @bind-Value="_form.Name" class="input" />
        <ValidationMessage For="@(() => _form.Name)" class="text-red-500 text-xs mt-1" />
    </div>

    <div>
        <label class="label">@Localizer["ai.agent.descriptionLabel"]</label>
        <InputTextArea @bind-Value="_form.Description" class="input" rows="3" />
    </div>

    <div>
        <label class="label">@Localizer["ai.agent.personaLabel"]</label>
        <InputTextArea @bind-Value="_form.Persona" class="input" rows="6" />
        <p class="mt-1 text-xs text-gray-500">@Localizer["ai.agent.personaHelper"]</p>
    </div>

    <button type="submit" class="btn-primary" disabled="@_saving">
        @if (_saving)
        {
            <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
        }
        @Localizer["common.save"]
    </button>
</EditForm>

@code {
    [Parameter] public Agent Agent { get; set; } = default!;
    [Parameter] public EventCallback OnSaved { get; set; }

    private UpdateAgentPersonaPayload _form = new();
    private bool _saving;

    protected override void OnParametersSet()
    {
        _form = new UpdateAgentPersonaPayload
        {
            AgentId = Agent.AgentId,
            Name = Agent.Name,
            Description = Agent.Description,
            Persona = Agent.Persona,
            ProjectKey = Settings.ProjectSlug
        };
    }

    private async Task Save()
    {
        _saving = true;
        var response = await AiService.UpdateAgentPersonaAsync(_form);

        if (response.IsSuccessStatusCode)
        {
            Toast.ShowSuccess(Localizer["common.saved"]);
            await OnSaved.InvokeAsync();
        }
        else
        {
            Toast.ShowError(Localizer["common.error"]);
        }
        _saving = false;
    }
}
```

### AiConfigTab (`Pages/AiConfigTab.razor`)

```razor
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject ToastService Toast
@inject AppSettings Settings

<EditForm Model="_form" OnValidSubmit="Save" class="max-w-2xl space-y-4">
    <DataAnnotationsValidator />

    <div>
        <label class="label">@Localizer["ai.config.modelLabel"]</label>
        <select @bind="_form.ModelId" class="input">
            <option value="">@Localizer["common.selectPlaceholder"]</option>
            @foreach (var model in _models)
            {
                <option value="@model.ModelId">@model.Name</option>
            }
        </select>
    </div>

    <div>
        <label class="label">@Localizer["ai.config.temperature"]</label>
        <div class="flex items-center gap-3">
            <span class="text-xs text-gray-500">@Localizer["ai.config.precise"]</span>
            <input type="range"
                   @bind="_form.Temperature"
                   @bind:event="oninput"
                   min="0" max="1" step="0.1"
                   class="flex-1 accent-primary" />
            <span class="text-xs text-gray-500">@Localizer["ai.config.creative"]</span>
            <span class="w-10 text-right text-sm font-medium text-gray-700">@_form.Temperature.ToString("F1")</span>
        </div>
    </div>

    <div>
        <label class="label">@Localizer["ai.config.maxTokens"]</label>
        <InputNumber @bind-Value="_form.MaxTokens" class="input" min="256" max="8192" step="256" />
    </div>

    <div>
        <label class="label">@Localizer["ai.config.systemPrompt"]</label>
        <InputTextArea @bind-Value="_form.SystemPrompt" class="input" rows="6" />
    </div>

    <button type="submit" class="btn-primary" disabled="@_saving">
        @if (_saving)
        {
            <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
        }
        @Localizer["common.save"]
    </button>
</EditForm>

@code {
    [Parameter] public Agent Agent { get; set; } = default!;
    [Parameter] public EventCallback OnSaved { get; set; }

    private List<AIModel> _models = new();
    private UpdateAiConfigPayload _form = new();
    private bool _saving;

    protected override async Task OnInitializedAsync()
    {
        var response = await AiService.GetModelsAsync(Settings.ProjectSlug);
        if (response.IsSuccessStatusCode)
        {
            _models = await response.Content.ReadFromJsonAsync<List<AIModel>>() ?? new();
        }
    }

    protected override void OnParametersSet()
    {
        _form = new UpdateAiConfigPayload
        {
            AgentId = Agent.AgentId,
            ModelId = Agent.ModelId,
            Temperature = Agent.Temperature,
            MaxTokens = Agent.MaxTokens,
            SystemPrompt = Agent.SystemPrompt,
            KbIds = Agent.KbIds,
            ToolIds = Agent.ToolIds,
            ProjectKey = Settings.ProjectSlug
        };
    }

    private async Task Save()
    {
        _saving = true;
        var response = await AiService.UpdateAiConfigAsync(_form);

        if (response.IsSuccessStatusCode)
        {
            Toast.ShowSuccess(Localizer["common.saved"]);
            await OnSaved.InvokeAsync();
        }
        else
        {
            Toast.ShowError(Localizer["common.error"]);
        }
        _saving = false;
    }
}
```

### ChatPage (`Pages/ChatPage.razor`)

```razor
@page "/ai/chat"
@page "/ai/chat/{AgentId}"
@attribute [Authorize]
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject AppSettings Settings
@implements IDisposable

<div class="flex h-[calc(100vh-7.5rem)]">
    @* Session sidebar *@
    <aside class="w-72 shrink-0 overflow-y-auto border-r border-gray-200 bg-white">
        <SessionSidebar AgentId="@AgentId"
                        SelectedSessionId="@_selectedSessionId"
                        OnSessionSelected="OnSessionSelected"
                        OnNewSession="OnNewSession" />
    </aside>

    @* Chat area *@
    <div class="flex flex-1 flex-col">
        @if (!string.IsNullOrEmpty(_selectedSessionId))
        {
            <ChatWindow @ref="_chatWindow"
                        Messages="_messages"
                        SessionId="@_selectedSessionId"
                        ProjectKey="@Settings.ProjectSlug"
                        OnMessageReceived="OnMessageReceived" />
            <ChatInput OnSend="SendMessage" Disabled="_isSending" />
        }
        else
        {
            <div class="flex flex-1 items-center justify-center">
                <p class="text-lg text-gray-400">@Localizer["ai.chat.selectSession"]</p>
            </div>
        }
    </div>
</div>

@code {
    [Parameter] public string? AgentId { get; set; }

    private ChatWindow? _chatWindow;
    private List<ChatMessage> _messages = new();
    private string _selectedSessionId = string.Empty;
    private bool _isSending;

    private async Task OnSessionSelected(string sessionId)
    {
        _selectedSessionId = sessionId;
        _messages.Clear();
        StateHasChanged();

        // Load conversation history
        var response = await AiService.GetConversationsAsync(
            AgentId ?? string.Empty, Settings.ProjectSlug);
        if (response.IsSuccessStatusCode)
        {
            // Messages are loaded from conversation history
            StateHasChanged();
        }
    }

    private async Task OnNewSession()
    {
        if (string.IsNullOrEmpty(AgentId)) return;

        var response = await AiService.InitiateConversationAsync(AgentId, Settings.ProjectSlug);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AiCommonResponse>();
            if (result?.ItemId is not null)
            {
                _selectedSessionId = result.ItemId;
                _messages.Clear();
                StateHasChanged();
            }
        }
    }

    private void OnMessageReceived(ChatMessage msg)
    {
        _messages.Add(msg);
        _isSending = false;
        StateHasChanged();
    }

    private async Task SendMessage(string message)
    {
        _isSending = true;
        _messages.Add(new ChatMessage
        {
            Role = "user",
            Content = message,
            Timestamp = DateTime.UtcNow.ToString("o")
        });
        StateHasChanged();

        if (_chatWindow is not null)
        {
            _chatWindow.OnMessageReceived = EventCallback.Factory.Create<ChatMessage>(this, OnMessageReceived);
            await _chatWindow.SendMessageAsync(message);
        }

        _isSending = false;
        StateHasChanged();
    }

    public void Dispose()
    {
        // ChatWindow handles its own CancellationTokenSource disposal
    }
}
```

### SessionSidebar (`Pages/SessionSidebar.razor`)

```razor
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject AppSettings Settings

<div class="p-3 space-y-2">
    <button class="btn-primary flex w-full items-center justify-center gap-1.5"
            @onclick="@(() => OnNewSession.InvokeAsync())">
        @* Heroicon: mini/plus *@
        <svg class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
            <path d="M10.75 4.75a.75.75 0 0 0-1.5 0v4.5h-4.5a.75.75 0 0 0 0 1.5h4.5v4.5a.75.75 0 0 0 1.5 0v-4.5h4.5a.75.75 0 0 0 0-1.5h-4.5v-4.5Z" />
        </svg>
        @Localizer["ai.chat.newSession"]
    </button>

    @if (_loading)
    {
        <div class="animate-pulse space-y-2">
            @for (int i = 0; i < 5; i++)
            {
                <div class="h-12 rounded-md bg-gray-200"></div>
            }
        </div>
    }
    else
    {
        @foreach (var session in _sessions)
        {
            <button class="@SessionClass(session.SessionId) w-full rounded-md px-3 py-2.5 text-left transition"
                    @onclick="@(() => OnSessionSelected.InvokeAsync(session.SessionId))">
                <p class="truncate text-sm font-medium text-gray-900">@session.Title</p>
                <p class="text-xs text-gray-500">
                    @session.MessageCount @Localizer["ai.chat.messages"]
                </p>
            </button>
        }
    }
</div>

@code {
    [Parameter] public string? AgentId { get; set; }
    [Parameter] public string SelectedSessionId { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> OnSessionSelected { get; set; }
    [Parameter] public EventCallback OnNewSession { get; set; }

    private List<ConversationSession> _sessions = new();
    private bool _loading = true;

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrEmpty(AgentId)) return;

        _loading = true;
        var response = await AiService.GetConversationsAsync(AgentId, Settings.ProjectSlug);
        if (response.IsSuccessStatusCode)
        {
            _sessions = await response.Content.ReadFromJsonAsync<List<ConversationSession>>() ?? new();
        }
        _loading = false;
    }

    private string SessionClass(string sessionId) => sessionId == SelectedSessionId
        ? "bg-primary/10 hover:bg-primary/15"
        : "hover:bg-gray-100";
}
```

---

## Streaming Chat Implementation — Full Example

Complete C# code for SSE streaming in Blazor WebAssembly:

```csharp
// Inside a Razor component or service method

private CancellationTokenSource? _cts;
private string _streamingContent = string.Empty;
private bool _isStreaming;

public async Task StreamChatAsync(string sessionId, string message, string projectKey)
{
    _cts = new CancellationTokenSource();
    _isStreaming = true;
    _streamingContent = string.Empty;

    try
    {
        // 1. Get the raw stream — HttpCompletionOption.ResponseHeadersRead
        //    ensures we start reading before the full response arrives
        using var stream = await AiService.ChatStreamAsync(
            sessionId, message, projectKey, _cts.Token);

        // 2. Read line-by-line with StreamReader
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !_cts.Token.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(_cts.Token);

            // SSE protocol: blank lines are event delimiters — skip them
            if (string.IsNullOrEmpty(line)) continue;

            // 3. Parse SSE "data:" lines
            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6); // remove "data: " prefix

                // End-of-stream signal
                if (data == "[DONE]") break;

                // 4. Parse JSON token
                var json = JsonSerializer.Deserialize<JsonElement>(data);
                if (json.TryGetProperty("token", out var tokenElement))
                {
                    _streamingContent += tokenElement.GetString();

                    // 5. Re-render after each token so the UI updates incrementally
                    StateHasChanged();
                }
            }
        }
    }
    catch (OperationCanceledException)
    {
        // User clicked "Stop generating" — partial content is kept
    }
    catch (Exception ex)
    {
        // Handle network errors, JSON parse errors, etc.
        Toast.ShowError(Localizer["ai.chat.streamError"]);
    }
    finally
    {
        _isStreaming = false;
        StateHasChanged();
    }
}

// Cancel on navigation or user action
public void StopGenerating() => _cts?.Cancel();

// MUST cancel on Dispose to prevent orphaned streams
public void Dispose()
{
    _cts?.Cancel();
    _cts?.Dispose();
}
```

Key points:
- Use `HttpCompletionOption.ResponseHeadersRead` so the stream begins before the full body is buffered
- Use `StreamReader.ReadLineAsync` with `CancellationToken` for cancellable reads
- Call `StateHasChanged()` after each token to render incrementally
- Always cancel via `CancellationTokenSource` on `Dispose` to prevent orphaned HTTP streams
- Parse `data: [DONE]` as the end-of-stream signal

---

## Route Definitions

```razor
@page "/ai/agents"              → AgentsPage.razor
@page "/ai/agents/{AgentId}"    → AgentDetailPage.razor
@page "/ai/chat"                → ChatPage.razor
@page "/ai/chat/{AgentId}"      → ChatPage.razor
```

All AI routes require `@attribute [Authorize]`.

---

## Error Handling

### HTTP Error Responses

All `AiService` methods return `HttpResponseMessage`. Components must check `IsSuccessStatusCode` and handle failures:

```razor
@code {
    private string? _error;

    private async Task LoadData()
    {
        _error = null;
        _loading = true;

        try
        {
            var response = await AiService.GetAgentsAsync(new GetAgentsPayload
            {
                ProjectKey = Settings.ProjectSlug
            });

            if (response.IsSuccessStatusCode)
            {
                _agents = await response.Content.ReadFromJsonAsync<List<Agent>>() ?? new();
            }
            else
            {
                _error = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => Localizer["common.error.unauthorized"],
                    System.Net.HttpStatusCode.Forbidden => Localizer["common.error.forbidden"],
                    System.Net.HttpStatusCode.NotFound => Localizer["common.error.notFound"],
                    _ => Localizer["common.error"]
                };
            }
        }
        catch (Exception)
        {
            _error = Localizer["common.error.network"];
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }
}
```

### Displaying Errors in Pages

Use `<ErrorAlert>` for inline errors:

```razor
@if (_error is not null)
{
    <ErrorAlert Message="@_error" Class="mb-4" />
}
```

### Toast Notifications for Actions

Use `ToastService` for success/error feedback on mutations (create, update, delete):

```razor
@inject ToastService Toast

@code {
    private async Task Save()
    {
        var response = await AiService.UpdateAgentPersonaAsync(payload);
        if (response.IsSuccessStatusCode)
        {
            Toast.ShowSuccess(Localizer["common.saved"]);
        }
        else
        {
            Toast.ShowError(Localizer["common.error"]);
        }
    }
}
```

### Stream Error Handling

Chat streaming must handle cancellation gracefully and show errors via toast:

```csharp
catch (OperationCanceledException)
{
    // User cancelled — keep partial content, no error shown
}
catch (HttpRequestException)
{
    Toast.ShowError(Localizer["ai.chat.streamError"]);
}
```

### Page State Pattern

Every page must handle three states: **loading**, **error**, and **data**:

```razor
@if (_loading)
{
    @* Skeleton or spinner *@
}
else if (_error is not null)
{
    <ErrorAlert Message="@_error" />
}
else if (!_items.Any())
{
    @* Empty state *@
}
else
{
    @* Data display *@
}
```

---

## Component Mapping — MudBlazor to Tailwind

| MudBlazor Component | Tailwind Equivalent |
|---------------------|---------------------|
| `MudCard` | `<div class="card">` |
| `MudChip` | `<span class="badge-*">` (badge-success, badge-gray, badge-danger) |
| `MudTabs` / `MudTabPanel` | Custom tab nav with `border-b-2` active state |
| `MudFileUpload` | `<InputFile>` inside a styled `<label>` |
| `MudTable` | HTML `<table>` with Tailwind + `<Pagination />` |
| `MudDialog` | `<Modal>` component |
| `MudSnackbar` / `ISnackbar` | `ToastService` (ShowSuccess, ShowError) |
| `MudTextField` | `<InputText class="input">` or `<input class="input">` |
| `MudSelect` | `<select class="input">` |
| `MudButton` (primary) | `<button class="btn-primary">` |
| `MudButton` (outline) | `<button class="btn-outline">` |
| `MudButton` (text) | `<button class="text-primary hover:text-primary/80">` |
| `MudProgressCircular` | `<span class="animate-spin rounded-full border-2 border-t-primary">` |
| `MudProgressLinear` | `<div class="bg-gray-200"><div class="bg-primary" style="width: X%">` |
| `MudSkeleton` | `<div class="animate-pulse rounded bg-gray-200">` |
| `MudAlert` | Styled `<div>` with bg color + border (e.g. `bg-blue-50 border-blue-200`) |
| `MudGrid` / `MudItem` | `<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">` |
| `MudSlider` | `<input type="range" class="accent-primary">` |
| `MudNumericField` | `<InputNumber class="input">` |
| `MudIcon` | Heroicons (inline SVG) |
| `MudList` / `MudListItem` | Styled `<button>` list with hover/active states |
| `IDialogService` | `_showModal` boolean + `<Modal>` component |

---

## Rules

- All UI uses plain HTML + Tailwind utility classes — never add MudBlazor or any component library
- Use the component classes from `Styles/app.css` (`.btn-primary`, `.input`, `.card`, `.badge-*`, `.label`) for consistency
- Use `EditForm` + `DataAnnotationsValidator` or FluentValidation for all form validation
- Use `ToastService` for success/error notifications — never `ISnackbar` or browser alerts
- Use `<Modal>` for dialogs — never `IDialogService`
- Use Heroicons (inline SVG) for all icons — never Material icons
- Handle loading, error, and empty states on every page
- **Every user-visible string must use `Localizer["key.name"]` — no hardcoded strings**
- **Look up existing keys with `get-keys-by-names` before creating new ones**
- `AiService` creates its own `HttpClient` — do not use the DI-registered one
- All C# model properties must have `[JsonPropertyName("snake_case")]` attributes
