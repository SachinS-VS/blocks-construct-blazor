# AI Services Frontend (Blazor)

This file extends `core/frontend-blazor.md` with AI-specific patterns for the ai-services skill.
Always read `core/frontend-blazor.md` first, then apply the overrides and additions here.

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
│   ├── CreateAgentDialog.razor        ← MudDialog to create new agent
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

<MudCard Elevation="2" Class="agent-card">
    <MudCardHeader>
        <CardHeaderContent>
            <MudText Typo="Typo.h6">@Agent.Name</MudText>
        </CardHeaderContent>
        <CardHeaderActions>
            <MudChip T="string"
                     Color="@GetStatusColor()"
                     Size="Size.Small">
                @Localizer[$"ai.agent.status.{Agent.Status}"]
            </MudChip>
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        <MudText Typo="Typo.body2" Class="text-muted">
            @Agent.Description
        </MudText>
    </MudCardContent>
    <MudCardActions>
        <MudButton Color="Color.Primary"
                   Variant="Variant.Text"
                   OnClick="@(() => Navigation.NavigateTo($"/ai/agents/{Agent.AgentId}"))">
            @Localizer["ai.agent.configure"]
        </MudButton>
    </MudCardActions>
</MudCard>

@code {
    [Parameter, EditorRequired] public Agent Agent { get; set; } = default!;

    private Color GetStatusColor() => Agent.Status switch
    {
        "active" => Color.Success,
        "inactive" => Color.Default,
        "archived" => Color.Error,
        _ => Color.Default
    };
}
```

### AgentCardSkeleton (`Components/AgentCardSkeleton.razor`)

```razor
<MudCard Elevation="2">
    <MudCardHeader>
        <CardHeaderContent>
            <MudSkeleton Width="60%" Height="28px" />
        </CardHeaderContent>
        <CardHeaderActions>
            <MudSkeleton Width="70px" Height="24px" SkeletonType="SkeletonType.Rectangle" />
        </CardHeaderActions>
    </MudCardHeader>
    <MudCardContent>
        <MudSkeleton Width="100%" />
        <MudSkeleton Width="80%" />
    </MudCardContent>
    <MudCardActions>
        <MudSkeleton Width="90px" Height="36px" SkeletonType="SkeletonType.Rectangle" />
    </MudCardActions>
</MudCard>
```

---

### ChatWindow (`Components/ChatWindow.razor`)

Streaming SSE chat using `HttpClient.SendAsync` with `HttpCompletionOption.ResponseHeadersRead`, then `StreamReader` to parse SSE lines.

```razor
@inject IAiService AiService
@inject ILocalizationService Localizer
@implements IDisposable

<div class="chat-messages" @ref="_messagesContainer" style="overflow-y: auto; flex: 1;">
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
    <MudButton Color="Color.Error"
               Variant="Variant.Text"
               StartIcon="@Icons.Material.Filled.Stop"
               OnClick="StopGenerating">
        @Localizer["ai.chat.stopGenerating"]
    </MudButton>
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

<div class="d-flex align-center gap-2 pa-2">
    <MudProgressCircular Size="Size.Small" Color="Color.Primary" Indeterminate="true" />
    <MudText Typo="Typo.caption" Class="text-muted">
        @Localizer["ai.chat.thinking"]
    </MudText>
</div>
```

### ChatInput (`Components/ChatInput.razor`)

```razor
@inject ILocalizationService Localizer

<MudPaper Class="d-flex align-center pa-2" Elevation="0">
    <MudTextField @bind-Value="_message"
                  Placeholder="@Localizer["ai.chat.placeholder"]"
                  Variant="Variant.Outlined"
                  FullWidth="true"
                  Adornment="Adornment.End"
                  AdornmentIcon="@Icons.Material.Filled.Send"
                  OnAdornmentClick="Send"
                  OnKeyDown="HandleKeyDown"
                  Disabled="Disabled" />
</MudPaper>

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
<div class="d-flex @(Message.Role == "user" ? "justify-end" : "justify-start") mb-2">
    <MudPaper Class="pa-3"
              Style="max-width: 70%;"
              Elevation="@(Message.Role == "user" ? 2 : 0)"
              Square="false">
        <MudText Typo="Typo.body1">@Message.Content</MudText>
    </MudPaper>
</div>

@code {
    [Parameter, EditorRequired] public ChatMessage Message { get; set; } = default!;
}
```

---

### KBUpload (`Components/KBUpload.razor`)

```razor
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject ISnackbar Snackbar

<MudFileUpload T="IReadOnlyList<IBrowserFile>"
               FilesChanged="OnFilesSelected"
               Accept=".pdf,.docx,.txt,.md,.csv">
    <ActivatorContent>
        <MudPaper Outlined="true"
                  Class="d-flex flex-column align-center justify-center pa-8 cursor-pointer"
                  Style="border-style: dashed;">
            <MudIcon Icon="@Icons.Material.Filled.CloudUpload" Size="Size.Large" Color="Color.Primary" />
            <MudText Typo="Typo.body1" Class="mt-2">@Localizer["ai.kb.dropFilesHere"]</MudText>
            <MudText Typo="Typo.caption" Class="text-muted">
                @Localizer["ai.kb.acceptedFormats"]
            </MudText>
        </MudPaper>
    </ActivatorContent>
</MudFileUpload>

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

    private async Task OnFilesSelected(IReadOnlyList<IBrowserFile> files)
    {
        foreach (var file in files)
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
                Snackbar.Add(Localizer["ai.kb.uploadFailed"], Severity.Error);
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

<div class="d-flex align-center gap-2 my-1">
    <MudIcon Icon="@Icons.Material.Filled.InsertDriveFile" Size="Size.Small" />
    <MudText Typo="Typo.body2" Class="flex-grow-1">@FileName</MudText>
    @if (Progress >= 0)
    {
        <MudProgressLinear Value="@Progress" Color="Color.Primary" Style="width: 120px;" />
    }
    else
    {
        <MudIcon Icon="@Icons.Material.Filled.Error" Color="Color.Error" Size="Size.Small" />
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

<MudAlert Severity="Severity.Info" Class="mt-2" Icon="@Icons.Material.Filled.HourglassTop">
    <div class="d-flex align-center gap-2">
        <MudProgressCircular Size="Size.Small" Indeterminate="true" Color="Color.Info" />
        <MudText Typo="Typo.body2">@Localizer["ai.kb.indexingMessage"]</MudText>
    </div>
</MudAlert>
```

> The `ai.kb.indexingMessage` key should resolve to something like "Indexing your file — this may take up to 30 seconds".

---

## Pages

### AgentsPage (`Pages/AgentsPage.razor`)

```razor
@page "/ai/agents"
@attribute [Authorize]
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject IDialogService DialogService
@inject AppSettings Settings

<MudText Typo="Typo.h4" Class="mb-4">@Localizer["ai.agents.title"]</MudText>

<div class="d-flex justify-space-between align-center mb-4">
    <MudTextField @bind-Value="_search"
                  Placeholder="@Localizer["ai.agents.searchPlaceholder"]"
                  Adornment="Adornment.Start"
                  AdornmentIcon="@Icons.Material.Filled.Search"
                  Immediate="true"
                  DebounceInterval="400"
                  OnDebounceIntervalElapsed="LoadAgents"
                  Style="max-width: 400px;" />
    <MudButton Color="Color.Primary"
               Variant="Variant.Filled"
               StartIcon="@Icons.Material.Filled.Add"
               OnClick="OpenCreateDialog">
        @Localizer["ai.agents.create"]
    </MudButton>
</div>

@if (_loading)
{
    <MudGrid>
        @for (int i = 0; i < 6; i++)
        {
            <MudItem xs="12" sm="6" md="4">
                <AgentCardSkeleton />
            </MudItem>
        }
    </MudGrid>
}
else if (_agents.Count == 0)
{
    <MudAlert Severity="Severity.Info">
        @Localizer["ai.agents.emptyState"]
    </MudAlert>
}
else
{
    <MudGrid>
        @foreach (var agent in _agents)
        {
            <MudItem xs="12" sm="6" md="4">
                <AgentCard Agent="agent" />
            </MudItem>
        }
    </MudGrid>
}

@code {
    private List<Agent> _agents = new();
    private string _search = string.Empty;
    private bool _loading = true;

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

    private async Task OpenCreateDialog()
    {
        var dialog = await DialogService.ShowAsync<CreateAgentDialog>(
            Localizer["ai.agents.createTitle"]);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            await LoadAgents();
        }
    }
}
```

### CreateAgentDialog (`Pages/CreateAgentDialog.razor`)

```razor
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject ISnackbar Snackbar
@inject AppSettings Settings

<MudDialog>
    <DialogContent>
        <MudForm @ref="_form">
            <MudTextField @bind-Value="_model.Name"
                          Label="@Localizer["ai.agent.nameLabel"]"
                          Required="true"
                          RequiredError="@Localizer["ai.agent.nameRequired"]" />
            <MudTextField @bind-Value="_model.Description"
                          Label="@Localizer["ai.agent.descriptionLabel"]"
                          Lines="3"
                          Required="true"
                          RequiredError="@Localizer["ai.agent.descriptionRequired"]" />
        </MudForm>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">@Localizer["common.cancel"]</MudButton>
        <MudButton Color="Color.Primary" OnClick="Submit" Disabled="_submitting">
            @if (_submitting)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
            }
            @Localizer["common.create"]
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private MudForm _form = default!;
    private CreateAgentPayload _model = new();
    private bool _submitting;

    private void Cancel() => MudDialog.Cancel();

    private async Task Submit()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        _submitting = true;
        _model.ProjectKey = Settings.ProjectSlug;

        var response = await AiService.CreateAgentAsync(_model);
        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add(Localizer["ai.agent.created"], Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
        else
        {
            Snackbar.Add(Localizer["common.error"], Severity.Error);
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
    <MudSkeleton Width="40%" Height="32px" Class="mb-4" />
    <MudSkeleton Width="100%" Height="400px" />
}
else if (_agent is not null)
{
    <MudText Typo="Typo.h4" Class="mb-4">@_agent.Name</MudText>

    <MudTabs Elevation="0" Rounded="true" ApplyEffectsToContainer="true" PanelClass="pa-4">
        <MudTabPanel Text="@Localizer["ai.agent.tab.persona"]">
            <PersonaTab Agent="_agent" OnSaved="ReloadAgent" />
        </MudTabPanel>
        <MudTabPanel Text="@Localizer["ai.agent.tab.aiConfig"]">
            <AiConfigTab Agent="_agent" OnSaved="ReloadAgent" />
        </MudTabPanel>
        <MudTabPanel Text="@Localizer["ai.agent.tab.knowledgeBase"]">
            <KBTab Agent="_agent" />
        </MudTabPanel>
        <MudTabPanel Text="@Localizer["ai.agent.tab.tools"]">
            <ToolsTab Agent="_agent" />
        </MudTabPanel>
    </MudTabs>
}

@code {
    [Parameter] public string AgentId { get; set; } = string.Empty;

    private Agent? _agent;
    private bool _loading = true;

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
}
```

### PersonaTab (`Pages/PersonaTab.razor`)

```razor
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject ISnackbar Snackbar
@inject AppSettings Settings

<MudForm @ref="_form">
    <MudTextField @bind-Value="_name"
                  Label="@Localizer["ai.agent.nameLabel"]"
                  Required="true" Class="mb-4" />
    <MudTextField @bind-Value="_description"
                  Label="@Localizer["ai.agent.descriptionLabel"]"
                  Lines="3" Class="mb-4" />
    <MudTextField @bind-Value="_persona"
                  Label="@Localizer["ai.agent.personaLabel"]"
                  Lines="6"
                  HelperText="@Localizer["ai.agent.personaHelper"]"
                  Class="mb-4" />
    <MudButton Color="Color.Primary" Variant="Variant.Filled"
               OnClick="Save" Disabled="_saving">
        @Localizer["common.save"]
    </MudButton>
</MudForm>

@code {
    [Parameter] public Agent Agent { get; set; } = default!;
    [Parameter] public EventCallback OnSaved { get; set; }

    private MudForm _form = default!;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _persona = string.Empty;
    private bool _saving;

    protected override void OnParametersSet()
    {
        _name = Agent.Name;
        _description = Agent.Description;
        _persona = Agent.Persona;
    }

    private async Task Save()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        _saving = true;
        var response = await AiService.UpdateAgentPersonaAsync(new UpdateAgentPersonaPayload
        {
            AgentId = Agent.AgentId,
            Name = _name,
            Description = _description,
            Persona = _persona,
            ProjectKey = Settings.ProjectSlug
        });

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add(Localizer["common.saved"], Severity.Success);
            await OnSaved.InvokeAsync();
        }
        else
        {
            Snackbar.Add(Localizer["common.error"], Severity.Error);
        }
        _saving = false;
    }
}
```

### AiConfigTab (`Pages/AiConfigTab.razor`)

```razor
@inject IAiService AiService
@inject ILocalizationService Localizer
@inject ISnackbar Snackbar
@inject AppSettings Settings

<MudForm @ref="_form">
    <MudSelect @bind-Value="_modelId"
               Label="@Localizer["ai.config.modelLabel"]"
               Required="true" Class="mb-4">
        @foreach (var model in _models)
        {
            <MudSelectItem Value="@model.ModelId">@model.Name</MudSelectItem>
        }
    </MudSelect>

    <MudText Typo="Typo.subtitle2" Class="mb-1">@Localizer["ai.config.temperature"]</MudText>
    <div class="d-flex align-center gap-2 mb-4">
        <MudText Typo="Typo.caption">@Localizer["ai.config.precise"]</MudText>
        <MudSlider @bind-Value="_temperature" Min="0.0" Max="1.0" Step="0.1"
                   Color="Color.Primary" Style="flex: 1;" />
        <MudText Typo="Typo.caption">@Localizer["ai.config.creative"]</MudText>
        <MudText Typo="Typo.body2" Class="ml-2">@_temperature.ToString("F1")</MudText>
    </div>

    <MudNumericField @bind-Value="_maxTokens"
                     Label="@Localizer["ai.config.maxTokens"]"
                     Min="256" Max="8192" Step="256" Class="mb-4" />

    <MudTextField @bind-Value="_systemPrompt"
                  Label="@Localizer["ai.config.systemPrompt"]"
                  Lines="6" Class="mb-4" />

    <MudButton Color="Color.Primary" Variant="Variant.Filled"
               OnClick="Save" Disabled="_saving">
        @Localizer["common.save"]
    </MudButton>
</MudForm>

@code {
    [Parameter] public Agent Agent { get; set; } = default!;
    [Parameter] public EventCallback OnSaved { get; set; }

    private MudForm _form = default!;
    private List<AIModel> _models = new();
    private string _modelId = string.Empty;
    private double _temperature = 0.7;
    private int _maxTokens = 2048;
    private string _systemPrompt = string.Empty;
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
        _modelId = Agent.ModelId;
        _temperature = Agent.Temperature;
        _maxTokens = Agent.MaxTokens;
        _systemPrompt = Agent.SystemPrompt;
    }

    private async Task Save()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        _saving = true;
        var response = await AiService.UpdateAiConfigAsync(new UpdateAiConfigPayload
        {
            AgentId = Agent.AgentId,
            ModelId = _modelId,
            Temperature = _temperature,
            MaxTokens = _maxTokens,
            SystemPrompt = _systemPrompt,
            KbIds = Agent.KbIds,
            ToolIds = Agent.ToolIds,
            ProjectKey = Settings.ProjectSlug
        });

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add(Localizer["common.saved"], Severity.Success);
            await OnSaved.InvokeAsync();
        }
        else
        {
            Snackbar.Add(Localizer["common.error"], Severity.Error);
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

<MudLayout>
    <MudDrawer Open="true" Variant="DrawerVariant.Persistent" Width="300px" Elevation="1">
        <SessionSidebar AgentId="@AgentId"
                        SelectedSessionId="@_selectedSessionId"
                        OnSessionSelected="OnSessionSelected"
                        OnNewSession="OnNewSession" />
    </MudDrawer>
    <MudMainContent Class="d-flex flex-column" Style="height: calc(100vh - 64px);">
        @if (!string.IsNullOrEmpty(_selectedSessionId))
        {
            <ChatWindow @ref="_chatWindow"
                        Messages="_messages"
                        SessionId="@_selectedSessionId"
                        ProjectKey="@Settings.ProjectSlug" />
            <ChatInput OnSend="SendMessage" Disabled="_isSending" />
        }
        else
        {
            <div class="d-flex align-center justify-center flex-grow-1">
                <MudText Typo="Typo.h6" Class="text-muted">
                    @Localizer["ai.chat.selectSession"]
                </MudText>
            </div>
        }
    </MudMainContent>
</MudLayout>

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
            _chatWindow.OnMessageReceived = EventCallback.Factory.Create<ChatMessage>(this, msg =>
            {
                _messages.Add(msg);
                _isSending = false;
                StateHasChanged();
            });
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

<div class="pa-2">
    <MudButton Color="Color.Primary" Variant="Variant.Filled" FullWidth="true"
               StartIcon="@Icons.Material.Filled.Add"
               OnClick="@(() => OnNewSession.InvokeAsync())" Class="mb-2">
        @Localizer["ai.chat.newSession"]
    </MudButton>

    @if (_loading)
    {
        @for (int i = 0; i < 5; i++)
        {
            <MudSkeleton Width="100%" Height="48px" Class="mb-1" />
        }
    }
    else
    {
        <MudList T="string" Dense="true" @bind-SelectedValue="@SelectedSessionId">
            @foreach (var session in _sessions)
            {
                <MudListItem Value="@session.SessionId"
                             OnClick="@(() => OnSessionSelected.InvokeAsync(session.SessionId))">
                    <div class="d-flex flex-column">
                        <MudText Typo="Typo.body2">@session.Title</MudText>
                        <MudText Typo="Typo.caption" Class="text-muted">
                            @session.MessageCount @Localizer["ai.chat.messages"]
                        </MudText>
                    </div>
                </MudListItem>
            }
        </MudList>
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
        Snackbar.Add(Localizer["ai.chat.streamError"], Severity.Error);
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

## Rules Specific to AI Module

- Use `[JsonPropertyName("snake_case")]` on every model property — never rely on naming policy alone for clarity
- For file uploads (`UploadKBFileAsync`), never set `Content-Type` manually — let `MultipartFormDataContent` set the multipart boundary
- Always use `CancellationTokenSource` for SSE streaming and cancel on `Dispose` — failing to do this leaks HTTP connections
- Show "Indexing..." feedback (`KBProcessingStatus`) after KB file upload — do not assume instant completion
- Temperature: `MudSlider` with `Min="0.0"` `Max="1.0"` `Step="0.1"`, label the extremes with `Localizer["ai.config.precise"]` / `Localizer["ai.config.creative"]`
- Max tokens: `MudNumericField` with `Min="256"` `Max="8192"` `Step="256"`
- Never display raw `agent_id` or `model_id` values to the user — always show the `Name` field
- The `AiService` uses its own `HttpClient` (not the DI one with `TokenDelegatingHandler`) because AI endpoints use a different base URL (`{ApiBaseUrl}/blocksai-api/v1`)
- Every user-visible string must use `Localizer["key"]` — no hardcoded strings
