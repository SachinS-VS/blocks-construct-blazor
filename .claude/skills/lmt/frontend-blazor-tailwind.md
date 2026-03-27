# LMT — Frontend Guide (Blazor + Tailwind)

This file extends `core/frontend-blazor-tailwind.md` with LMT-specific patterns for the Logging, Monitoring & Tracing domain.
Always read `core/frontend-blazor-tailwind.md` first, then apply the additions here.

---

## Module Structure

All LMT UI lives inside `Modules/Lmt/`:

```
Modules/Lmt/
├── Components/
│   ├── LogFilter.razor            ← date inputs, level select, service name, search
│   ├── LogTable.razor             ← paginated log rows with level badges
│   ├── LiveLogPanel.razor         ← SSE streaming terminal view
│   ├── TraceList.razor            ← paginated trace rows with duration bars
│   ├── TraceDetail.razor          ← span waterfall / call tree
│   └── AnalyticsCharts.razor      ← status code chart, latency bar chart (Chart.js)
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

public record StatusCodeDistribution(int StatusCode, int Count, double Percentage);
```

---

## Service Layer

```csharp
// Modules/Lmt/Services/LmtService.cs

public interface ILmtService
{
    Task<HttpResponseMessage> GetLogs(GetLogsRequest request);
    Task<HttpResponseMessage> GetLogsByDate(GetLogsRequest request);
    Task<HttpResponseMessage> GetTraces(GetTracesRequest request);
    Task<HttpResponseMessage> GetTrace(string traceId, string projectKey);
    Task<HttpResponseMessage> GetOperationalAnalytics(GetApiAnalyticsRequest request);
    string GetLiveLogStreamUrl(string serviceName, string projectKey);
}

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

Use Tailwind badge classes for consistent level badges. Map each log level to a badge class:

```csharp
public static class LogLevelBadges
{
    public static string GetBadgeClass(LogLevel level) => level switch
    {
        LogLevel.Trace       => "badge-gray",
        LogLevel.Debug       => "badge-info",
        LogLevel.Information => "badge-success",
        LogLevel.Warning     => "badge-warning",
        LogLevel.Error       => "badge-danger",
        LogLevel.Critical    => "badge-danger font-bold",
        _                    => "badge-gray"
    };
}
```

Badge classes are defined in `Styles/app.css`:

```css
/* Styles/app.css — add these if not already present */
.badge-gray    { @apply inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800; }
.badge-info    { @apply inline-flex items-center rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-medium text-blue-800; }
.badge-success { @apply inline-flex items-center rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800; }
.badge-warning { @apply inline-flex items-center rounded-full bg-yellow-100 px-2.5 py-0.5 text-xs font-medium text-yellow-800; }
.badge-danger  { @apply inline-flex items-center rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800; }
```

Usage in templates:

```razor
<span class="@LogLevelBadges.GetBadgeClass(entry.Level)">
    @entry.Level
</span>
```

---

## LogFilter Component

```razor
@* Modules/Lmt/Components/LogFilter.razor *@
@inject ILocalizationService Localizer

<div class="card mb-4 p-4">
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        @* Date range — two date inputs *@
        <div>
            <label class="label">@Localizer["lmt.filter.startDate"]</label>
            <input type="date" class="input" @bind="_startDate" @bind:event="onchange"
                   @onchange="OnFilterChanged" />
        </div>
        <div>
            <label class="label">@Localizer["lmt.filter.endDate"]</label>
            <input type="date" class="input" @bind="_endDate" @bind:event="onchange"
                   @onchange="OnFilterChanged" />
        </div>

        @* Log level select *@
        <div>
            <label class="label">@Localizer["lmt.filter.logLevel"]</label>
            <select class="input" @bind="_selectedLevel" @bind:event="onchange"
                    @onchange="OnFilterChanged">
                <option value="">@Localizer["common.selectPlaceholder"]</option>
                @foreach (var level in Enum.GetValues<LogLevel>())
                {
                    <option value="@level.ToString()">@level</option>
                }
            </select>
        </div>

        @* Search *@
        <div>
            <label class="label">@Localizer["lmt.filter.search"]</label>
            <div class="relative">
                <svg class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400"
                     fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round"
                          d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" />
                </svg>
                <input type="text" class="input pl-10" placeholder="@Localizer["lmt.filter.searchPlaceholder"]"
                       @bind="_search" @bind:event="oninput" @oninput="OnSearchDebounced" />
            </div>
        </div>
    </div>

    @* Service name — separate row *@
    <div class="mt-4 max-w-sm">
        <label class="label">@Localizer["lmt.filter.serviceName"]</label>
        <input type="text" class="input" @bind="_serviceName" @bind:event="oninput"
               @oninput="OnSearchDebounced" />
    </div>
</div>

@code {
    [Parameter] public EventCallback<GetLogsRequest> OnFilterApplied { get; set; }
    [Parameter] public string ProjectKey { get; set; } = string.Empty;

    private DateTime? _startDate;
    private DateTime? _endDate;
    private string? _selectedLevel;
    private string _serviceName = string.Empty;
    private string _search = string.Empty;
    private CancellationTokenSource? _debounceCts;

    private async Task OnFilterChanged()
    {
        await EmitFilter();
    }

    private async Task OnSearchDebounced()
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        try
        {
            await Task.Delay(400, _debounceCts.Token);
            await EmitFilter();
        }
        catch (TaskCanceledException) { }
    }

    private async Task EmitFilter()
    {
        var request = new GetLogsRequest(
            ServiceName: _serviceName,
            Filter: new LogFilter(
                StartDate: _startDate?.ToString("o"),
                EndDate: _endDate?.ToString("o"),
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
- Use manual debounce (400ms) on text fields to avoid excessive API calls
- Date inputs use ISO 8601 format for API requests
- Two separate `<input type="date">` replace `MudDateRangePicker`

---

## LogTable Component

```razor
@* Modules/Lmt/Components/LogTable.razor *@
@inject ILocalizationService Localizer
@inject ILmtService LmtService
@inject NavigationManager Nav
@inject ToastService Toast

<div class="card overflow-hidden">
    <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
            <tr>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["lmt.logs.timestamp"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["lmt.logs.level"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["lmt.logs.service"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["lmt.logs.message"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["lmt.logs.traceId"]
                </th>
            </tr>
        </thead>
        <tbody class="divide-y divide-gray-200 bg-white">
            @if (_loading)
            {
                @for (var i = 0; i < 5; i++)
                {
                    <tr>
                        <td class="px-6 py-4"><div class="h-4 w-28 animate-pulse rounded bg-gray-200"></div></td>
                        <td class="px-6 py-4"><div class="h-4 w-20 animate-pulse rounded bg-gray-200"></div></td>
                        <td class="px-6 py-4"><div class="h-4 w-24 animate-pulse rounded bg-gray-200"></div></td>
                        <td class="px-6 py-4"><div class="h-4 w-48 animate-pulse rounded bg-gray-200"></div></td>
                        <td class="px-6 py-4"><div class="h-4 w-20 animate-pulse rounded bg-gray-200"></div></td>
                    </tr>
                }
            }
            else if (!_items.Any())
            {
                <tr>
                    <td colspan="5" class="px-6 py-12 text-center text-sm text-gray-500">
                        @Localizer["lmt.logs.empty"]
                    </td>
                </tr>
            }
            else
            {
                @foreach (var entry in _items)
                {
                    <tr class="hover:bg-gray-50">
                        <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                            @entry.Timestamp
                        </td>
                        <td class="whitespace-nowrap px-6 py-4">
                            <span class="@LogLevelBadges.GetBadgeClass(entry.Level)">
                                @entry.Level
                            </span>
                        </td>
                        <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-900">
                            @entry.ServiceName
                        </td>
                        <td class="px-6 py-4 text-sm text-gray-500 max-w-md truncate">
                            @Truncate(entry.Message, 80)
                        </td>
                        <td class="whitespace-nowrap px-6 py-4 text-sm">
                            @if (!string.IsNullOrEmpty(entry.TraceId))
                            {
                                <a href="/lmt/traces/@entry.TraceId"
                                   class="text-primary hover:underline">
                                    @Truncate(entry.TraceId, 12)
                                </a>
                            }
                        </td>
                    </tr>
                }
            }
        </tbody>
    </table>
    <Pagination CurrentPage="_page" PageSize="_pageSize" TotalCount="_totalCount"
                OnPageChanged="LoadPage" />
</div>

@code {
    [Parameter] public GetLogsRequest? CurrentRequest { get; set; }

    private List<LogEntry> _items = new();
    private bool _loading;
    private int _page = 1;
    private int _pageSize = 50;
    private int _totalCount;

    public async Task Reload()
    {
        _page = 1;
        await LoadData();
    }

    private async Task LoadPage(int page)
    {
        _page = page;
        await LoadData();
    }

    private async Task LoadData()
    {
        if (CurrentRequest is null) return;

        _loading = true;
        StateHasChanged();

        try
        {
            var request = CurrentRequest with { Page = _page, PageSize = _pageSize };
            var response = await LmtService.GetLogs(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content
                    .ReadFromJsonAsync<PagedResult<LogEntry>>();
                _items = result?.Items ?? new();
                _totalCount = result?.TotalCount ?? 0;
            }
            else
            {
                var message = LmtErrorMap.GetMessage(
                    (int)response.StatusCode, "get-logs", Localizer);
                Toast.ShowError(message);
            }
        }
        catch
        {
            Toast.ShowError(Localizer["common.error.unexpected"]);
        }

        _loading = false;
        StateHasChanged();
    }

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
@inject IJSRuntime JsRuntime
@implements IAsyncDisposable

<div class="rounded-lg font-mono text-sm leading-relaxed"
     style="background-color: #111827; color: #d1d5db; height: 500px; overflow-y: auto;"
     id="live-log-panel">

    @if (_connectionError)
    {
        <div class="mx-3 mt-3 rounded-md bg-yellow-900/50 border border-yellow-700 px-4 py-2 text-sm text-yellow-300">
            @Localizer["lmt.liveLogs.connectionLost"]
        </div>
    }

    <div class="p-3 space-y-0.5">
        @foreach (var entry in _logEntries)
        {
            <div class="@GetEntryClasses(entry.Level)">
                <span class="text-gray-500">@entry.Timestamp</span>
                <span class="@GetLevelClasses(entry.Level)">[@entry.Level]</span>
                <span class="text-cyan-400">@entry.ServiceName</span>
                <span>@entry.Message</span>
            </div>
        }
    </div>
</div>

@code {
    [Parameter] public string ServiceName { get; set; } = string.Empty;
    [Parameter] public string ProjectKey { get; set; } = string.Empty;

    private readonly List<LogEntry> _logEntries = new();
    private CancellationTokenSource? _cts;
    private bool _connectionError;

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
                            await JsRuntime.InvokeVoidAsync(
                                "eval",
                                "document.getElementById('live-log-panel')?.scrollTo(0, document.getElementById('live-log-panel').scrollHeight)");
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

    private static string GetLevelClasses(LogLevel level) => level switch
    {
        LogLevel.Trace       => "text-gray-500",
        LogLevel.Debug       => "text-blue-400",
        LogLevel.Information => "text-green-400",
        LogLevel.Warning     => "text-yellow-400",
        LogLevel.Error       => "text-red-400",
        LogLevel.Critical    => "text-red-400 font-bold",
        _                    => ""
    };

    private static string GetEntryClasses(LogLevel level) => level switch
    {
        LogLevel.Error    => "border-l-2 border-red-500 pl-2 my-0.5",
        LogLevel.Critical => "border-l-2 border-red-500 pl-2 my-0.5 bg-red-900/20",
        _                 => "pl-2.5 my-0.5"
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
- Show connection lost warning with auto-retry after 3 seconds
- Terminal-style dark background (`bg-gray-900` / `#111827`) with monospace font
- Level colors use Tailwind text color classes (green-400, yellow-400, red-400, blue-400)

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

<div class="card p-4">
    <h3 class="mb-4 text-lg font-semibold text-gray-900">
        @Localizer["lmt.trace.waterfall"] — @TraceId
    </h3>
    <div class="space-y-1">
        @foreach (var node in _spanTree)
        {
            RenderSpanNode(node, 0);
        }
    </div>
</div>

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
        var barColor = GetSpanBarColor(node.Span.StatusCode);
        var indent = depth * 24;

        <div style="padding-left: @(indent)px;" class="mb-1">
            <div class="flex items-center gap-2">
                <span class="min-w-[200px] shrink-0 truncate text-xs text-gray-600"
                      title="@node.Span.OperationName">
                    @node.Span.OperationName
                </span>
                <div class="relative h-5 flex-1 rounded bg-gray-100">
                    <div class="absolute left-0 top-0 h-full rounded @barColor"
                         style="width: @(widthPercent.ToString("F1"))%; min-width: 4px;"
                         title="@($"{node.Span.Duration:F1}ms — {node.Span.ServiceName}")">
                    </div>
                </div>
                <span class="min-w-[60px] text-right text-xs text-gray-500">
                    @($"{node.Span.Duration:F1}ms")
                </span>
            </div>
        </div>

        @foreach (var child in node.Children)
        {
            RenderSpanNode(child, depth + 1);
        }
    }

    private static string GetSpanBarColor(int statusCode) => statusCode switch
    {
        >= 200 and < 300 => "bg-green-500",
        >= 400 and < 500 => "bg-yellow-500",
        >= 500           => "bg-red-500",
        _                => "bg-gray-400"
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

Use Chart.js via JS interop for analytics visualizations. Include Chart.js from CDN in `index.html`:

```html
<!-- wwwroot/index.html — add before closing </body> -->
<script src="https://cdn.jsdelivr.net/npm/chart.js@4/dist/chart.umd.min.js"></script>
```

Add a JS helper for Chart.js interop:

```javascript
// wwwroot/js/charts.js
window.ChartInterop = {
    create: function (canvasId, config) {
        const ctx = document.getElementById(canvasId)?.getContext('2d');
        if (!ctx) return;
        // Destroy existing chart on same canvas if any
        if (window._charts && window._charts[canvasId]) {
            window._charts[canvasId].destroy();
        }
        window._charts = window._charts || {};
        window._charts[canvasId] = new Chart(ctx, config);
    },
    destroy: function (canvasId) {
        if (window._charts && window._charts[canvasId]) {
            window._charts[canvasId].destroy();
            delete window._charts[canvasId];
        }
    }
};
```

```razor
@* Modules/Lmt/Components/AnalyticsCharts.razor *@
@inject ILocalizationService Localizer
@inject IJSRuntime JsRuntime
@implements IAsyncDisposable

@* Status code distribution — Pie chart *@
<div class="card mb-4 p-4">
    <h3 class="mb-4 text-lg font-semibold text-gray-900">
        @Localizer["lmt.analytics.statusDistribution"]
    </h3>
    <div class="mx-auto" style="max-width: 300px; max-height: 300px;">
        <canvas id="status-pie-chart"></canvas>
    </div>
</div>

@* Latency by operation — Bar chart (avg, p95, p99 grouped) *@
<div class="card mb-4 p-4">
    <h3 class="mb-4 text-lg font-semibold text-gray-900">
        @Localizer["lmt.analytics.latencyByOperation"]
    </h3>
    <div style="height: 400px;">
        <canvas id="latency-bar-chart"></canvas>
    </div>
</div>

@* Error rate — simple HTML bar fallback *@
<div class="card mb-4 p-4">
    <h3 class="mb-4 text-lg font-semibold text-gray-900">
        @Localizer["lmt.analytics.errorRate"]
    </h3>
    <div class="space-y-3">
        @foreach (var metric in Metrics)
        {
            <div>
                <div class="flex items-center justify-between text-sm">
                    <span class="text-gray-700">@metric.OperationName</span>
                    <span class="font-medium @(metric.ErrorRate > 5 ? "text-red-600" : "text-gray-600")">
                        @($"{metric.ErrorRate:F1}%")
                    </span>
                </div>
                <div class="mt-1 h-2 w-full rounded-full bg-gray-200">
                    <div class="h-2 rounded-full @(metric.ErrorRate > 5 ? "bg-red-500" : "bg-green-500")"
                         style="width: @(Math.Min(metric.ErrorRate, 100).ToString("F1"))%">
                    </div>
                </div>
            </div>
        }
    </div>
</div>

@code {
    [Parameter] public List<StatusCodeDistribution> StatusDistribution { get; set; } = new();
    [Parameter] public List<OperationMetric> Metrics { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await RenderCharts();
        }
    }

    protected override async Task OnParametersSet()
    {
        await RenderCharts();
    }

    private async Task RenderCharts()
    {
        // Status code pie chart
        if (StatusDistribution.Any())
        {
            var pieConfig = new
            {
                type = "pie",
                data = new
                {
                    labels = StatusDistribution.Select(d => $"{d.StatusCode} ({d.Percentage:F1}%)").ToArray(),
                    datasets = new[]
                    {
                        new
                        {
                            data = StatusDistribution.Select(d => (double)d.Count).ToArray(),
                            backgroundColor = StatusDistribution.Select(d => d.StatusCode switch
                            {
                                >= 200 and < 300 => "#22c55e",
                                >= 400 and < 500 => "#eab308",
                                >= 500           => "#ef4444",
                                _                => "#9ca3af"
                            }).ToArray()
                        }
                    }
                }
            };
            await JsRuntime.InvokeVoidAsync("ChartInterop.create", "status-pie-chart", pieConfig);
        }

        // Latency bar chart
        if (Metrics.Any())
        {
            var barConfig = new
            {
                type = "bar",
                data = new
                {
                    labels = Metrics.Select(m => m.OperationName).ToArray(),
                    datasets = new object[]
                    {
                        new { label = "Avg (ms)", data = Metrics.Select(m => m.AvgDurationMs).ToArray(), backgroundColor = "#3b82f6" },
                        new { label = "P95 (ms)", data = Metrics.Select(m => m.P95DurationMs).ToArray(), backgroundColor = "#f59e0b" },
                        new { label = "P99 (ms)", data = Metrics.Select(m => m.P99DurationMs).ToArray(), backgroundColor = "#ef4444" }
                    }
                },
                options = new
                {
                    responsive = true,
                    maintainAspectRatio = false,
                    scales = new { y = new { beginAtZero = true } }
                }
            };
            await JsRuntime.InvokeVoidAsync("ChartInterop.create", "latency-bar-chart", barConfig);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await JsRuntime.InvokeVoidAsync("ChartInterop.destroy", "status-pie-chart");
        await JsRuntime.InvokeVoidAsync("ChartInterop.destroy", "latency-bar-chart");
    }
}
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

<ProtectedView Permission="lmt.view">
    <h1 class="mb-6 text-2xl font-bold text-gray-900">@Localizer["lmt.logs.title"]</h1>
    <LogFilter OnFilterApplied="OnFilterApplied" ProjectKey="@_projectKey" />
    <LogTable @ref="_logTable" CurrentRequest="@_currentRequest" />
</ProtectedView>

@code {
    private LogTable? _logTable;
    private GetLogsRequest? _currentRequest;
    private string _projectKey = string.Empty;

    private async Task OnFilterApplied(GetLogsRequest request)
    {
        _currentRequest = request;
        if (_logTable is not null)
            await _logTable.Reload();
    }
}
```

```razor
@* Modules/Lmt/Pages/TracesPage.razor *@
@page "/lmt/traces"
@attribute [Authorize]
@inject ILocalizationService Localizer
@inject ILmtService LmtService
@inject ToastService Toast

<ProtectedView Permission="lmt.view">
    <h1 class="mb-6 text-2xl font-bold text-gray-900">@Localizer["lmt.traces.title"]</h1>

    @* Filter row *@
    <div class="card mb-4 p-4">
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <div>
                <label class="label">@Localizer["lmt.filter.startDate"]</label>
                <input type="date" class="input" @bind="_startDate" @bind:event="onchange"
                       @onchange="LoadTraces" />
            </div>
            <div>
                <label class="label">@Localizer["lmt.filter.endDate"]</label>
                <input type="date" class="input" @bind="_endDate" @bind:event="onchange"
                       @onchange="LoadTraces" />
            </div>
        </div>
    </div>

    @* Trace list table *@
    <div class="card overflow-hidden">
        <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
                <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                        @Localizer["lmt.traces.traceId"]
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                        @Localizer["lmt.traces.rootOperation"]
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                        @Localizer["lmt.traces.service"]
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                        @Localizer["lmt.traces.duration"]
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                        @Localizer["lmt.traces.spans"]
                    </th>
                </tr>
            </thead>
            <tbody class="divide-y divide-gray-200 bg-white">
                @if (_loading)
                {
                    @for (var i = 0; i < 5; i++)
                    {
                        <tr>
                            <td class="px-6 py-4"><div class="h-4 w-28 animate-pulse rounded bg-gray-200"></div></td>
                            <td class="px-6 py-4"><div class="h-4 w-32 animate-pulse rounded bg-gray-200"></div></td>
                            <td class="px-6 py-4"><div class="h-4 w-24 animate-pulse rounded bg-gray-200"></div></td>
                            <td class="px-6 py-4"><div class="h-4 w-16 animate-pulse rounded bg-gray-200"></div></td>
                            <td class="px-6 py-4"><div class="h-4 w-10 animate-pulse rounded bg-gray-200"></div></td>
                        </tr>
                    }
                }
                else if (!_traces.Any())
                {
                    <tr>
                        <td colspan="5" class="px-6 py-12 text-center text-sm text-gray-500">
                            @Localizer["lmt.traces.empty"]
                        </td>
                    </tr>
                }
                else
                {
                    @foreach (var trace in _traces)
                    {
                        <tr class="cursor-pointer hover:bg-gray-50"
                            @onclick="() => Nav.NavigateTo($&quot;/lmt/traces/{trace.TraceId}&quot;)">
                            <td class="whitespace-nowrap px-6 py-4 text-sm text-primary">
                                @trace.TraceId[..Math.Min(12, trace.TraceId.Length)]...
                            </td>
                            <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-900">
                                @trace.RootSpan.OperationName
                            </td>
                            <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                                @trace.RootSpan.ServiceName
                            </td>
                            <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                                @($"{trace.TotalDuration:F1}ms")
                            </td>
                            <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                                @trace.SpanCount
                            </td>
                        </tr>
                    }
                }
            </tbody>
        </table>
        <Pagination CurrentPage="_page" PageSize="_pageSize" TotalCount="_totalCount"
                    OnPageChanged="LoadPage" />
    </div>
</ProtectedView>

@code {
    @inject NavigationManager Nav;

    private List<Trace> _traces = new();
    private bool _loading;
    private int _page = 1;
    private int _pageSize = 50;
    private int _totalCount;
    private DateTime? _startDate;
    private DateTime? _endDate;

    private async Task LoadPage(int page)
    {
        _page = page;
        await LoadTraces();
    }

    private async Task LoadTraces()
    {
        _loading = true;
        StateHasChanged();

        try
        {
            var request = new GetTracesRequest(
                Filter: new TracesFilter(
                    StartDate: _startDate?.ToString("o"),
                    EndDate: _endDate?.ToString("o"),
                    Services: null,
                    StatusCodes: null
                ),
                Page: _page,
                PageSize: _pageSize
            );
            var response = await LmtService.GetTraces(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content
                    .ReadFromJsonAsync<PagedResult<Trace>>();
                _traces = result?.Items ?? new();
                _totalCount = result?.TotalCount ?? 0;
            }
        }
        catch
        {
            Toast.ShowError(Localizer["common.error.unexpected"]);
        }

        _loading = false;
        StateHasChanged();
    }
}
```

```razor
@* Modules/Lmt/Pages/AnalyticsPage.razor *@
@page "/lmt/analytics"
@attribute [Authorize]
@inject ILocalizationService Localizer
@inject ILmtService LmtService
@inject ToastService Toast

<ProtectedView Permission="lmt.view">
    <h1 class="mb-6 text-2xl font-bold text-gray-900">@Localizer["lmt.analytics.title"]</h1>

    <div class="card mb-4 p-4">
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <div>
                <label class="label">@Localizer["lmt.filter.startDate"]</label>
                <input type="date" class="input" @bind="_startDate" />
            </div>
            <div>
                <label class="label">@Localizer["lmt.filter.endDate"]</label>
                <input type="date" class="input" @bind="_endDate" />
            </div>
            <div>
                <label class="label">@Localizer["lmt.filter.serviceName"]</label>
                <input type="text" class="input" @bind="_serviceName" />
            </div>
        </div>
        <div class="mt-4">
            <button class="btn-primary" @onclick="LoadAnalytics">
                @Localizer["common.apply"]
            </button>
        </div>
    </div>

    @if (_loading)
    {
        <LoadingOverlay />
    }
    else
    {
        <AnalyticsCharts StatusDistribution="_statusDistribution" Metrics="_metrics" />
    }
</ProtectedView>

@code {
    private DateTime _startDate = DateTime.UtcNow.AddDays(-7);
    private DateTime _endDate = DateTime.UtcNow;
    private string _serviceName = string.Empty;
    private bool _loading;
    private List<StatusCodeDistribution> _statusDistribution = new();
    private List<OperationMetric> _metrics = new();

    private async Task LoadAnalytics()
    {
        _loading = true;
        StateHasChanged();

        try
        {
            var request = new GetApiAnalyticsRequest(
                StartTime: _startDate.ToString("o"),
                EndTime: _endDate.ToString("o"),
                ServiceName: _serviceName
            );
            var response = await LmtService.GetOperationalAnalytics(request);
            if (response.IsSuccessStatusCode)
            {
                // Deserialize analytics response and populate chart data
                // Exact structure depends on API response shape
            }
        }
        catch
        {
            Toast.ShowError(Localizer["common.error.unexpected"]);
        }

        _loading = false;
        StateHasChanged();
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

## Component Mapping: MudBlazor to Tailwind

| MudBlazor Component | Tailwind Replacement |
|---------------------|---------------------|
| `MudTable` | HTML `<table>` with Tailwind classes + `<Pagination />` |
| `MudDateRangePicker` | Two `<input type="date" class="input">` |
| `MudSelect` | `<select class="input">` |
| `MudTextField` | `<input type="text" class="input">` |
| `MudChip` | `<span class="badge-*">` (badge-success, badge-danger, etc.) |
| `MudPaper` | `<div class="card">` |
| `MudText Typo.h4` | `<h1 class="text-2xl font-bold text-gray-900">` |
| `MudText Typo.h6` | `<h3 class="text-lg font-semibold text-gray-900">` |
| `MudText Typo.caption` | `<span class="text-xs text-gray-500">` |
| `MudLink` | `<a class="text-primary hover:underline">` |
| `MudAlert` | `<ErrorAlert />` or inline `div` with Tailwind alert classes |
| `MudSkeleton` | `<div class="animate-pulse rounded bg-gray-200">` with width/height |
| `MudChart` | Chart.js via JS interop or HTML bars with Tailwind |
| `MudGrid` / `MudItem` | `<div class="grid grid-cols-*">` |
| `MudSnackbar` | `ToastService` |
| `MudTablePager` | `<Pagination />` |

---

## Rules Specific to LMT Module

- One service class (`LmtService`) — components call service methods, not `HttpClient` directly
- All strings use `ILocalizationService` — no hardcoded user-facing text
- Use `animate-pulse` skeleton rows for loading states in tables, never blank empty space
- Live log panel must dispose `CancellationTokenSource` via `IAsyncDisposable`
- Trace waterfall builds a tree from flat spans using `ParentSpanId` — do not assume spans arrive in order
- Keep log streaming buffer to 500 entries maximum to prevent memory issues
- All LMT pages use `MainLayout` (authenticated shell with sidebar)
- All LMT routes are protected with `[Authorize]` attribute and `ProtectedView` for permission-based access
- All UI uses plain HTML + Tailwind utility classes — never import MudBlazor or any component library
- Use component classes from `Styles/app.css` (`.btn-primary`, `.input`, `.card`, `.badge-*`) for consistent styling
- Use `ToastService` for error/success notifications — not browser alerts
- Chart.js is used via JS interop for pie and bar charts; simple HTML bars with Tailwind are acceptable for simpler visualizations
