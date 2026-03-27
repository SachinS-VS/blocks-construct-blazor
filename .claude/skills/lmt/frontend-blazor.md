# LMT — Frontend Guide (Blazor)

This file extends `core/frontend-blazor.md` with LMT-specific patterns for the Logging, Monitoring & Tracing domain.
Always read `core/frontend-blazor.md` first, then apply the additions here.

---

## Module Structure

All LMT UI lives inside `Modules/Lmt/`:

```
Modules/Lmt/
├── Components/
│   ├── LogFilter.razor            ← date range, level, service name selectors
│   ├── LogTable.razor             ← paginated log rows with level badges
│   ├── LiveLogPanel.razor         ← SSE streaming terminal view
│   ├── TraceList.razor            ← paginated trace rows with duration bars
│   ├── TraceDetail.razor          ← span waterfall / call tree
│   └── AnalyticsCharts.razor      ← status code pie chart, latency bar chart
├── Pages/
│   ├── LogsPage.razor             ← log viewer with filter panel
│   ├── TracesPage.razor           ← trace list with filter
│   └── AnalyticsPage.razor        ← charts for status codes and latency
├── Services/
│   └── LmtService.cs              ← all LMT API calls
└── Models/
    └── LmtModels.cs               ← C# request/response models
```

---

## C# Models

```csharp
// Modules/Lmt/Models/LmtModels.cs

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical
}

public record LogEntry(
    string Timestamp,
    LogLevel Level,
    string Message,
    string ServiceName,
    string? TraceId,
    string? SpanId,
    Dictionary<string, object>? Properties
);

public record LogFilter(
    string? StartDate,
    string? EndDate,
    string? LogLevel,
    string? TraceId,
    string? SpanId
);

public record GetLogsRequest(
    string ServiceName,
    int Page = 1,
    int PageSize = 50,
    SortRequest? Sort = null,
    LogFilter? Filter = null,
    string? Search = null,
    string? ProjectKey = null
);

public record SortRequest(string Property, bool IsDescending);

public record TraceSpan(
    string SpanId,
    string? ParentSpanId,
    string OperationName,
    string ServiceName,
    string StartTime,
    string EndTime,
    double Duration,
    int StatusCode,
    Dictionary<string, string>? Tags,
    List<object>? Logs
);

public record Trace(
    string TraceId,
    TraceRootSpan RootSpan,
    int SpanCount,
    double TotalDuration
);

public record TraceRootSpan(
    string SpanId,
    string OperationName,
    string ServiceName,
    string StartTime,
    double Duration,
    int StatusCode
);

public record OperationMetric(
    string OperationName,
    int CallCount,
    double AvgDurationMs,
    double P95DurationMs,
    double P99DurationMs,
    double ErrorRate
);

public record GetTracesRequest(
    TracesFilter? Filter = null,
    int Page = 1,
    int PageSize = 50,
    SortRequest? Sort = null,
    string? Search = null,
    string? ProjectKey = null
);

public record TracesFilter(
    string? StartDate,
    string? EndDate,
    List<string>? Services,
    List<int>? StatusCodes
);

public record GetApiAnalyticsRequest(
    string StartTime,
    string EndTime,
    string ServiceName,
    string? OperationName = null,
    string? ProjectKey = null
);
```

---

## Service Layer

```csharp
// Modules/Lmt/Services/LmtService.cs
public class LmtService : ILmtService
{
    private readonly HttpClient _http;
    private readonly AppSettings _settings;
    private const string Base = "/lmt/v1";

    public LmtService(HttpClient http, AppSettings settings)
    {
        _http = http;
        _settings = settings;
    }

    public Task<HttpResponseMessage> GetLogs(GetLogsRequest request) =>
        _http.PostAsJsonAsync($"{Base}/Log/GetLogs", request);

    public Task<HttpResponseMessage> GetLogsByDate(GetLogsRequest request) =>
        _http.PostAsJsonAsync($"{Base}/Log/GetLogsByDate", request);

    public Task<HttpResponseMessage> GetTraces(GetTracesRequest request) =>
        _http.PostAsJsonAsync($"{Base}/Trace/GetTraces", request);

    public Task<HttpResponseMessage> GetTrace(string traceId, string projectKey) =>
        _http.GetAsync($"{Base}/Trace/{traceId}?projectKey={projectKey}");

    public Task<HttpResponseMessage> GetOperationalAnalytics(GetApiAnalyticsRequest request) =>
        _http.PostAsJsonAsync($"{Base}/Analytics/GetOperationalAnalytics", request);

    public string GetLiveLogStreamUrl(string serviceName, string projectKey) =>
        $"{_settings.ApiBaseUrl}{Base}/Log/Live?serviceName={serviceName}&projectKey={projectKey}";
}
```

Register in DI:

```csharp
// Program.cs or ServiceCollectionExtensions.cs
builder.Services.AddScoped<ILmtService, LmtService>();
```

---

## Log Level Colors

Use MudBlazor `Color` and `Severity` mappings for consistent level badges:

```csharp
public static class LogLevelColors
{
    public static Color GetColor(LogLevel level) => level switch
    {
        LogLevel.Trace       => Color.Default,
        LogLevel.Debug       => Color.Info,
        LogLevel.Information => Color.Success,
        LogLevel.Warning     => Color.Warning,
        LogLevel.Error       => Color.Error,
        LogLevel.Critical    => Color.Error,
        _                    => Color.Default
    };

    public static string GetStyle(LogLevel level) => level switch
    {
        LogLevel.Critical => "font-weight: bold;",
        _                 => string.Empty
    };
}
```

Use `MudChip` with the appropriate `Color` for level badges in log tables:

```razor
<MudChip T="string"
         Color="@LogLevelColors.GetColor(entry.Level)"
         Size="Size.Small"
         Style="@LogLevelColors.GetStyle(entry.Level)">
    @entry.Level
</MudChip>
```

---

## LogFilter Component

```razor
@* Modules/Lmt/Components/LogFilter.razor *@
@inject ILocalizationService Localizer

<MudPaper Class="pa-4 mb-4">
    <MudGrid>
        <MudItem xs="12" sm="6" md="3">
            <MudDateRangePicker @bind-DateRange="_dateRange"
                                Label="@Localizer["lmt.filter.dateRange"]"
                                Variant="Variant.Outlined"
                                DateRangeChanged="OnFilterChanged" />
        </MudItem>
        <MudItem xs="12" sm="6" md="3">
            <MudSelect T="string"
                       @bind-Value="_selectedLevel"
                       Label="@Localizer["lmt.filter.logLevel"]"
                       Variant="Variant.Outlined"
                       Clearable="true"
                       ValueChanged="@(_ => OnFilterChanged())">
                @foreach (var level in Enum.GetValues<LogLevel>())
                {
                    <MudSelectItem T="string" Value="@level.ToString()">@level</MudSelectItem>
                }
            </MudSelect>
        </MudItem>
        <MudItem xs="12" sm="6" md="3">
            <MudTextField T="string"
                          @bind-Value="_serviceName"
                          Label="@Localizer["lmt.filter.serviceName"]"
                          Variant="Variant.Outlined"
                          DebounceInterval="400"
                          OnDebounceIntervalElapsed="OnFilterChanged" />
        </MudItem>
        <MudItem xs="12" sm="6" md="3">
            <MudTextField T="string"
                          @bind-Value="_search"
                          Label="@Localizer["lmt.filter.search"]"
                          Variant="Variant.Outlined"
                          Adornment="Adornment.Start"
                          AdornmentIcon="@Icons.Material.Filled.Search"
                          DebounceInterval="400"
                          OnDebounceIntervalElapsed="OnFilterChanged" />
        </MudItem>
    </MudGrid>
</MudPaper>

@code {
    [Parameter] public EventCallback<GetLogsRequest> OnFilterApplied { get; set; }
    [Parameter] public string ProjectKey { get; set; } = string.Empty;

    private DateRange? _dateRange;
    private string? _selectedLevel;
    private string _serviceName = string.Empty;
    private string _search = string.Empty;

    private async Task OnFilterChanged()
    {
        var request = new GetLogsRequest(
            ServiceName: _serviceName,
            Filter: new LogFilter(
                StartDate: _dateRange?.Start?.ToString("o"),
                EndDate: _dateRange?.End?.ToString("o"),
                LogLevel: _selectedLevel,
                TraceId: null,
                SpanId: null
            ),
            Search: string.IsNullOrWhiteSpace(_search) ? null : _search,
            ProjectKey: ProjectKey
        );
        await OnFilterApplied.InvokeAsync(request);
    }
}
```

Rules:
- All filter changes trigger data reload via `OnFilterApplied` callback
- Use debounce on text fields to avoid excessive API calls
- Date range picker uses ISO 8601 format for API requests

---

## LogTable Component

```razor
@* Modules/Lmt/Components/LogTable.razor *@
@inject ILocalizationService Localizer
@inject NavigationManager Nav

<MudTable T="LogEntry"
          ServerData="LoadServerData"
          @ref="_table"
          Dense="true"
          Hover="true"
          Striped="true"
          Loading="_loading"
          LoadingProgressColor="Color.Primary">
    <HeaderContent>
        <MudTh>@Localizer["lmt.logs.timestamp"]</MudTh>
        <MudTh>@Localizer["lmt.logs.level"]</MudTh>
        <MudTh>@Localizer["lmt.logs.service"]</MudTh>
        <MudTh>@Localizer["lmt.logs.message"]</MudTh>
        <MudTh>@Localizer["lmt.logs.traceId"]</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>@context.Timestamp</MudTd>
        <MudTd>
            <MudChip T="string"
                     Color="@LogLevelColors.GetColor(context.Level)"
                     Size="Size.Small"
                     Style="@LogLevelColors.GetStyle(context.Level)">
                @context.Level
            </MudChip>
        </MudTd>
        <MudTd>@context.ServiceName</MudTd>
        <MudTd>@Truncate(context.Message, 80)</MudTd>
        <MudTd>
            @if (!string.IsNullOrEmpty(context.TraceId))
            {
                <MudLink Href="@($"/lmt/traces/{context.TraceId}")"
                         Color="Color.Primary">
                    @Truncate(context.TraceId, 12)
                </MudLink>
            }
        </MudTd>
    </RowTemplate>
    <NoRecordsContent>
        <MudText Typo="Typo.body1">
            @Localizer["lmt.logs.empty"]
        </MudText>
    </NoRecordsContent>
    <LoadingContent>
        @for (var i = 0; i < 5; i++)
        {
            <MudTr>
                <MudTd><MudSkeleton Width="120px" /></MudTd>
                <MudTd><MudSkeleton Width="80px" /></MudTd>
                <MudTd><MudSkeleton Width="100px" /></MudTd>
                <MudTd><MudSkeleton Width="200px" /></MudTd>
                <MudTd><MudSkeleton Width="80px" /></MudTd>
            </MudTr>
        }
    </LoadingContent>
    <PagerContent>
        <MudTablePager />
    </PagerContent>
</MudTable>

@code {
    [Parameter] public GetLogsRequest? CurrentRequest { get; set; }

    private MudTable<LogEntry>? _table;
    private bool _loading;

    private async Task<TableData<LogEntry>> LoadServerData(TableState state, CancellationToken ct)
    {
        if (CurrentRequest is null)
            return new TableData<LogEntry> { Items = [], TotalItems = 0 };

        _loading = true;
        StateHasChanged();

        var request = CurrentRequest with
        {
            Page = state.Page + 1,
            PageSize = state.PageSize
        };

        // Call LmtService.GetLogs and deserialize response
        // Return TableData<LogEntry> with items and totalCount

        _loading = false;
        return new TableData<LogEntry> { Items = items, TotalItems = totalCount };
    }

    public void Reload() => _table?.ReloadServerData();

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : $"{text[..maxLength]}...";
}
```

---

## Live Log Streaming

Use `HttpClient.GetStreamAsync` with `StreamReader` to consume SSE from `/lmt/v1/Log/Live`. Parse `data:` lines and append to the log buffer, keeping the last 500 entries. Show in a terminal-style panel with auto-scroll.

```razor
@* Modules/Lmt/Components/LiveLogPanel.razor *@
@inject HttpClient Http
@inject ILmtService LmtService
@inject ILocalizationService Localizer
@implements IAsyncDisposable

<MudPaper Class="pa-3"
          Style="background-color: #1e1e1e; color: #d4d4d4; font-family: 'Cascadia Code', 'Consolas', monospace; font-size: 0.85rem; height: 500px; overflow-y: auto;"
          @ref="_panelRef"
          id="live-log-panel">
    @if (_connectionError)
    {
        <MudAlert Severity="Severity.Warning" Dense="true" Class="mb-2">
            @Localizer["lmt.liveLogs.connectionLost"]
        </MudAlert>
    }
    @foreach (var entry in _logEntries)
    {
        <div style="@GetEntryStyle(entry.Level)">
            <span style="color: #888;">@entry.Timestamp</span>
            <span style="@GetLevelStyle(entry.Level)">[@entry.Level]</span>
            <span>@entry.ServiceName</span>
            <span>@entry.Message</span>
        </div>
    }
</MudPaper>

@code {
    [Parameter] public string ServiceName { get; set; } = string.Empty;
    [Parameter] public string ProjectKey { get; set; } = string.Empty;

    private readonly List<LogEntry> _logEntries = new();
    private CancellationTokenSource? _cts;
    private bool _connectionError;
    private MudPaper? _panelRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !string.IsNullOrEmpty(ServiceName))
        {
            await StartStreaming();
        }
    }

    private async Task StartStreaming()
    {
        _cts = new CancellationTokenSource();
        _connectionError = false;

        try
        {
            var url = LmtService.GetLiveLogStreamUrl(ServiceName, ProjectKey);
            using var stream = await Http.GetStreamAsync(url, _cts.Token);
            using var reader = new StreamReader(stream);

            while (!_cts.Token.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(_cts.Token);
                if (line is null) break;

                if (line.StartsWith("data:"))
                {
                    var json = line["data:".Length..].Trim();
                    try
                    {
                        var entry = JsonSerializer.Deserialize<LogEntry>(json);
                        if (entry is not null)
                        {
                            _logEntries.Add(entry);
                            if (_logEntries.Count > 500)
                                _logEntries.RemoveAt(0);

                            await InvokeAsync(StateHasChanged);
                            // Auto-scroll to bottom via JS interop
                            // await JsRuntime.InvokeVoidAsync("scrollToBottom", "live-log-panel");
                        }
                    }
                    catch (JsonException)
                    {
                        // Malformed event data — skip
                    }
                }
            }
        }
        catch (Exception) when (!_cts.Token.IsCancellationRequested)
        {
            _connectionError = true;
            await InvokeAsync(StateHasChanged);

            // Auto-retry after 3 seconds
            await Task.Delay(3000, _cts.Token);
            if (!_cts.Token.IsCancellationRequested)
            {
                await StartStreaming();
            }
        }
    }

    private static string GetLevelStyle(LogLevel level) => level switch
    {
        LogLevel.Trace       => "color: #888;",
        LogLevel.Debug       => "color: #569cd6;",
        LogLevel.Information => "color: #6a9955;",
        LogLevel.Warning     => "color: #d7ba7d;",
        LogLevel.Error       => "color: #f44747;",
        LogLevel.Critical    => "color: #f44747; font-weight: bold;",
        _                    => string.Empty
    };

    private static string GetEntryStyle(LogLevel level) => level switch
    {
        LogLevel.Error    => "border-left: 2px solid #f44747; padding-left: 8px; margin: 2px 0;",
        LogLevel.Critical => "border-left: 2px solid #f44747; padding-left: 8px; margin: 2px 0; background-color: rgba(244,71,71,0.1);",
        _                 => "padding-left: 10px; margin: 2px 0;"
    };

    public async ValueTask DisposeAsync()
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
    }
}
```

Rules:
- Keep last 500 log entries to prevent memory exhaustion
- Auto-scroll to bottom on new entries
- Use `CancellationTokenSource` for cleanup on dispose
- Show "Live log connection lost. Reconnecting..." on error with auto-retry after 3 seconds
- Terminal-style dark background with monospace font

---

## Trace Waterfall Component

Display spans as horizontal bars in a waterfall timeline:

- Width = proportional to duration relative to root span
- Indent = nesting level (child spans indented under parent)
- Color: green for 2xx, yellow for 4xx, red for 5xx
- Build span tree from flat array using `ParentSpanId`

```razor
@* Modules/Lmt/Components/TraceDetail.razor *@
@inject ILocalizationService Localizer

<MudPaper Class="pa-4">
    <MudText Typo="Typo.h6" Class="mb-3">
        @Localizer["lmt.trace.waterfall"] — @TraceId
    </MudText>
    <div class="trace-waterfall">
        @foreach (var node in _spanTree)
        {
            RenderSpanNode(node, 0);
        }
    </div>
</MudPaper>

@code {
    [Parameter] public string TraceId { get; set; } = string.Empty;
    [Parameter] public List<TraceSpan> Spans { get; set; } = new();

    private List<SpanNode> _spanTree = new();
    private double _rootDuration;

    protected override void OnParametersSet()
    {
        _spanTree = BuildTree(Spans);
        _rootDuration = Spans.MaxBy(s => s.Duration)?.Duration ?? 1;
    }

    private void RenderSpanNode(SpanNode node, int depth)
    {
        var widthPercent = Math.Max(1, (node.Span.Duration / _rootDuration) * 100);
        var color = GetSpanColor(node.Span.StatusCode);
        var indent = depth * 24;

        <div class="span-row" style="padding-left: @(indent)px; margin-bottom: 4px;">
            <div class="d-flex align-center gap-2">
                <MudText Typo="Typo.caption" Style="min-width: 200px; white-space: nowrap;">
                    @node.Span.OperationName
                </MudText>
                <div style="flex: 1; position: relative; height: 20px; background: rgba(0,0,0,0.05); border-radius: 4px;">
                    <div style="width: @(widthPercent)%; height: 100%; background: @color; border-radius: 4px; min-width: 4px;"
                         title="@($"{node.Span.Duration:F1}ms — {node.Span.ServiceName}")">
                    </div>
                </div>
                <MudText Typo="Typo.caption" Style="min-width: 60px; text-align: right;">
                    @($"{node.Span.Duration:F1}ms")
                </MudText>
            </div>
        </div>

        @foreach (var child in node.Children)
        {
            RenderSpanNode(child, depth + 1);
        }
    }

    private static string GetSpanColor(int statusCode) => statusCode switch
    {
        >= 200 and < 300 => "#4caf50",
        >= 400 and < 500 => "#ff9800",
        >= 500           => "#f44336",
        _                => "#9e9e9e"
    };

    private static List<SpanNode> BuildTree(List<TraceSpan> spans)
    {
        var map = spans.ToDictionary(s => s.SpanId, s => new SpanNode(s));
        var roots = new List<SpanNode>();

        foreach (var node in map.Values)
        {
            if (!string.IsNullOrEmpty(node.Span.ParentSpanId) &&
                map.TryGetValue(node.Span.ParentSpanId, out var parent))
            {
                parent.Children.Add(node);
            }
            else
            {
                roots.Add(node);
            }
        }

        return roots;
    }

    private record SpanNode(TraceSpan Span)
    {
        public List<SpanNode> Children { get; } = new();
    }
}
```

---

## Analytics Charts

Use MudBlazor `MudChart` for all analytics visualizations:

```razor
@* Modules/Lmt/Components/AnalyticsCharts.razor *@
@inject ILocalizationService Localizer

@* Status code distribution — Pie chart *@
<MudPaper Class="pa-4 mb-4">
    <MudText Typo="Typo.h6" Class="mb-3">
        @Localizer["lmt.analytics.statusDistribution"]
    </MudText>
    <MudChart ChartType="ChartType.Pie"
              InputData="@_statusData"
              InputLabels="@_statusLabels"
              Width="300px"
              Height="300px" />
</MudPaper>

@* Latency by operation — Bar chart (avg, p95, p99 grouped) *@
<MudPaper Class="pa-4 mb-4">
    <MudText Typo="Typo.h6" Class="mb-3">
        @Localizer["lmt.analytics.latencyByOperation"]
    </MudText>
    <MudChart ChartType="ChartType.Bar"
              ChartSeries="@_latencySeries"
              XAxisLabels="@_operationLabels"
              Width="100%"
              Height="400px" />
</MudPaper>

@* Error rate over time — Line chart *@
<MudPaper Class="pa-4 mb-4">
    <MudText Typo="Typo.h6" Class="mb-3">
        @Localizer["lmt.analytics.errorRate"]
    </MudText>
    <MudChart ChartType="ChartType.Line"
              ChartSeries="@_errorRateSeries"
              XAxisLabels="@_timeLabels"
              Width="100%"
              Height="300px" />
</MudPaper>

@code {
    [Parameter] public List<StatusCodeDistribution> StatusDistribution { get; set; } = new();
    [Parameter] public List<OperationMetric> Metrics { get; set; } = new();

    private double[] _statusData = [];
    private string[] _statusLabels = [];
    private List<ChartSeries> _latencySeries = new();
    private string[] _operationLabels = [];
    private List<ChartSeries> _errorRateSeries = new();
    private string[] _timeLabels = [];

    protected override void OnParametersSet()
    {
        // Status code pie chart
        _statusData = StatusDistribution.Select(d => (double)d.Count).ToArray();
        _statusLabels = StatusDistribution.Select(d => $"{d.StatusCode} ({d.Percentage:F1}%)").ToArray();

        // Latency bar chart — grouped by operation with avg, p95, p99
        _operationLabels = Metrics.Select(m => m.OperationName).ToArray();
        _latencySeries = new List<ChartSeries>
        {
            new() { Name = "Avg (ms)", Data = Metrics.Select(m => m.AvgDurationMs).ToArray() },
            new() { Name = "P95 (ms)", Data = Metrics.Select(m => m.P95DurationMs).ToArray() },
            new() { Name = "P99 (ms)", Data = Metrics.Select(m => m.P99DurationMs).ToArray() }
        };
    }
}

// Supporting model for status distribution
public record StatusCodeDistribution(int StatusCode, int Count, double Percentage);
```

---

## Route Definitions

```razor
@* Pages use MainLayout — authenticated shell with sidebar *@

@page "/lmt/logs"                → LogsPage.razor
@page "/lmt/traces"              → TracesPage.razor
@page "/lmt/traces/{TraceId}"    → TraceDetailPage.razor
@page "/lmt/analytics"           → AnalyticsPage.razor
```

All LMT routes require authentication. Typically restricted to admin/devops roles. Use `ProtectedView` for role-based access:

```razor
@* Modules/Lmt/Pages/LogsPage.razor *@
@page "/lmt/logs"
@attribute [Authorize]
@inject ILocalizationService Localizer

<ProtectedView Roles="@(new[] { "admin", "devops" })">
    <Authorized>
        <MudText Typo="Typo.h4" Class="mb-4">@Localizer["lmt.logs.title"]</MudText>
        <LogFilter OnFilterApplied="OnFilterApplied" ProjectKey="@_projectKey" />
        <LogTable @ref="_logTable" CurrentRequest="@_currentRequest" />
    </Authorized>
    <NotAuthorized>
        <MudAlert Severity="Severity.Warning">
            @Localizer["common.accessDenied"]
        </MudAlert>
    </NotAuthorized>
</ProtectedView>

@code {
    private LogTable? _logTable;
    private GetLogsRequest? _currentRequest;
    private string _projectKey = string.Empty;

    private void OnFilterApplied(GetLogsRequest request)
    {
        _currentRequest = request;
        _logTable?.Reload();
    }
}
```

---

## Error Handling

```csharp
public static class LmtErrorMap
{
    public static string GetMessage(int statusCode, string context, ILocalizationService localizer) =>
        (statusCode, context) switch
        {
            (_, "empty-logs")    => localizer["lmt.error.noLogs"],
            (400, "get-logs")    => localizer["lmt.error.invalidServiceName"],
            (404, "get-trace")   => localizer["lmt.error.traceNotFound"],
            (_, "sse-dropped")   => localizer["lmt.error.connectionLost"],
            _                    => localizer["common.error.unexpected"]
        };
}
```

| Error | Localization Key | Message |
|-------|-----------------|---------|
| Empty log results | `lmt.error.noLogs` | "No logs found for the selected filters. Try widening the date range or changing the log level." |
| 400 on get-logs | `lmt.error.invalidServiceName` | "Invalid service name." |
| 404 on get-trace | `lmt.error.traceNotFound` | "Trace not found. Traces are retained for a limited window — this trace may have expired." |
| SSE connection dropped | `lmt.error.connectionLost` | "Live log connection lost. Reconnecting..." |

---

## Rules Specific to LMT Module

- One service class (`LmtService`) — components call service methods, not `HttpClient` directly
- All strings use `ILocalizationService` — no hardcoded user-facing text
- Use `MudSkeleton` for loading states in tables, never blank empty space
- Live log panel must dispose `CancellationTokenSource` via `IAsyncDisposable`
- Trace waterfall builds a tree from flat spans using `ParentSpanId` — do not assume spans arrive in order
- Keep log streaming buffer to 500 entries maximum to prevent memory issues
- All LMT pages use `MainLayout` (authenticated shell with sidebar)
- All LMT routes are protected with `[Authorize]` attribute and `ProtectedView` for role-based access
