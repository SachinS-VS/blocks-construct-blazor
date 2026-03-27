# Localization — Frontend Guide (Blazor + Tailwind)

This file covers two distinct concerns:

1. **App-level i18n** — how every feature in the app uses translation keys (mandatory for all features)
2. **Localization admin UI** — the management pages for languages, modules, and keys

Always read `core/frontend-blazor-tailwind.md` first, then apply the additions here.

---

## Part 1: App-Level i18n Integration

This section applies to **every feature** in the app, not just the localization admin pages.

### Startup — Load Translations

On app boot, `ILocalizationService.LoadTranslationsAsync()` fetches the UILM file for the stored language before rendering protected routes. This is already handled in `Program.cs` (see `core/app-scaffold-blazor-tailwind.md`).

`BlocksLocalizationService` loads translations from `/uilm/v1/UilmFile/Get`:

```csharp
public async Task LoadTranslationsAsync()
{
    var projectKey = _appSettings.ProjectSlug;
    var response = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(
        $"/uilm/v1/UilmFile/Get?projectKey={projectKey}&languageCode={CurrentLanguage}");
    _translations = response ?? new();
}
```

The app wraps all routes so translations are available before any component renders.

### Re-fetch on Language Change

When the user switches language via `LanguageSwitcher`, `ILocalizationService.SetLanguageAsync()` is called. This reloads translations and fires the `OnLanguageChanged` event. Components subscribe to re-render.

```csharp
public async Task SetLanguageAsync(string languageCode)
{
    CurrentLanguage = languageCode;
    await _localStorage.SetItemAsStringAsync("language", languageCode);
    await LoadTranslationsAsync();
    OnLanguageChanged?.Invoke();
}
```

Components that inject `ILocalizationService` should subscribe to `OnLanguageChanged` and call `StateHasChanged()`:

```razor
@inject ILocalizationService Localizer
@implements IDisposable

@code {
    protected override void OnInitialized()
    {
        Localizer.OnLanguageChanged += HandleLanguageChanged;
    }

    private void HandleLanguageChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Localizer.OnLanguageChanged -= HandleLanguageChanged;
    }
}
```

### Key Lookup Before Feature Generation — Required Workflow

When generating any new feature component, follow this order:

```
Step 1 — List all strings needed
  e.g. title, form labels, button text, empty state, error messages

Step 2 — Call get-keys-by-names
  Check which keys already exist in the project

Step 3 — Reuse existing keys
  Map existing key names to the component's Localizer["..."] calls

Step 4 — Create missing keys
  Call save-keys (batch) for any key not found in Step 2
  Include translations for all configured languages if known

Step 5 — Generate component
  Use confirmed key names in all Localizer["..."] calls
```

### Common Keys to Always Check First

Before creating any new key, check if these common keys already exist:

```
common.submit         common.cancel        common.save
common.delete         common.edit          common.create
common.search         common.filter        common.reset
common.loading        common.error         common.success
common.confirm        common.back          common.close
common.yes            common.no            common.actions
common.noData         common.required      common.optional
```

---

## Part 2: Localization Admin UI

### Module Structure

All localization UI lives inside `Modules/Localization/`:

```
Modules/Localization/
├── Components/
│   ├── LanguageList.razor           <- table of languages with default badge and delete
│   ├── LanguageForm.razor           <- add/edit language form
│   ├── ModuleList.razor             <- list of translation modules
│   ├── ModuleForm.razor             <- add/edit module form
│   ├── KeyList.razor                <- paginated, filterable key table (central UI)
│   ├── KeyForm.razor                <- create/edit key with per-language translation inputs
│   ├── KeyTimeline.razor            <- version history drawer for a key
│   ├── TranslateProgress.razor      <- progress indicator for AI translate-all
│   └── ImportExport.razor           <- file upload and export controls
├── Pages/
│   ├── LanguagesPage.razor          <- language management page
│   ├── ModulesPage.razor            <- module management page
│   ├── KeysPage.razor               <- key management page (main localization editor)
│   └── SettingsPage.razor           <- webhook and project-level config
├── Services/
│   └── LocalizationAdminService.cs  <- all localization admin API calls
└── Models/
    ├── LocalizationModels.cs        <- C# types for all payloads and responses
    └── LocalizationValidators.cs    <- FluentValidation validators
```

---

### State Management

Use a scoped `LocalizationUiState` service for localization UI state (selected module, active language filter). No persistence needed.

```csharp
// Modules/Localization/State/LocalizationUiState.cs
public class LocalizationUiState
{
    public string? SelectedModuleId { get; private set; }
    public string? ActiveLanguageCode { get; private set; }

    public event Action? OnStateChanged;

    public void SetSelectedModule(string? moduleId)
    {
        SelectedModuleId = moduleId;
        OnStateChanged?.Invoke();
    }

    public void SetActiveLanguage(string? code)
    {
        ActiveLanguageCode = code;
        OnStateChanged?.Invoke();
    }
}
```

Register in DI:

```csharp
builder.Services.AddScoped<LocalizationUiState>();
```

Server state (languages, modules, keys) is fetched via `LocalizationAdminService` — no duplication in the state container.

---

### Service Layer

```csharp
// Modules/Localization/Services/LocalizationAdminService.cs
public class LocalizationAdminService : ILocalizationAdminService
{
    private readonly HttpClient _http;
    private const string Base = "/uilm/v1";

    public LocalizationAdminService(HttpClient http) => _http = http;

    // Languages
    public Task<HttpResponseMessage> GetLanguages(string projectKey) =>
        _http.GetAsync($"{Base}/Language/Gets?projectKey={projectKey}");

    public Task<HttpResponseMessage> SaveLanguage(SaveLanguagePayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Language/Save", payload);

    public Task<HttpResponseMessage> DeleteLanguage(string itemId, string projectKey) =>
        _http.DeleteAsync($"{Base}/Language/Delete?itemId={itemId}&projectKey={projectKey}");

    // Modules
    public Task<HttpResponseMessage> GetModules(string projectKey) =>
        _http.GetAsync($"{Base}/Module/Gets?projectKey={projectKey}");

    public Task<HttpResponseMessage> SaveModule(SaveModulePayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Module/Save", payload);

    public Task<HttpResponseMessage> DeleteModule(string itemId, string projectKey) =>
        _http.DeleteAsync($"{Base}/Module/Delete?itemId={itemId}&projectKey={projectKey}");

    // Keys
    public Task<HttpResponseMessage> GetKeys(GetKeysParams queryParams) =>
        _http.PostAsJsonAsync($"{Base}/Key/Gets", queryParams);

    public Task<HttpResponseMessage> SaveKey(SaveKeyPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Key/Save", payload);

    public Task<HttpResponseMessage> SaveKeys(SaveKeysPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Key/Saves", payload);

    public Task<HttpResponseMessage> DeleteKey(string itemId, string projectKey) =>
        _http.DeleteAsync($"{Base}/Key/Delete?itemId={itemId}&projectKey={projectKey}");

    public Task<HttpResponseMessage> GetKeysByNames(GetKeysByNamesPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Key/GetByKeyNames", payload);

    public Task<HttpResponseMessage> GetKeyTimeline(string keyId, int pageNumber, int pageSize) =>
        _http.GetAsync($"{Base}/Key/GetTimeline?keyId={keyId}&pageNumber={pageNumber}&pageSize={pageSize}");

    // AI Translation
    public Task<HttpResponseMessage> TranslateAll(TranslateAllPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Translation/TranslateAll", payload);

    public Task<HttpResponseMessage> TranslateKey(TranslateKeyPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/Translation/TranslateKey", payload);

    // Import / Export
    public Task<HttpResponseMessage> ImportUilm(MultipartFormDataContent formData) =>
        _http.PostAsync($"{Base}/UilmFile/Import", formData);

    public Task<HttpResponseMessage> ExportUilm(ExportUilmPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/UilmFile/Export", payload);

    public Task<HttpResponseMessage> GetUilmFile(string projectKey, string languageCode, string moduleId) =>
        _http.GetAsync($"{Base}/UilmFile/Get?projectKey={projectKey}&languageCode={languageCode}&moduleId={moduleId}");
}
```

Register in DI:

```csharp
builder.Services.AddScoped<ILocalizationAdminService, LocalizationAdminService>();
```

---

### C# Models

```csharp
// Modules/Localization/Models/LocalizationModels.cs

// Language
public class SaveLanguagePayload
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
}

public class Language
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
}

// Module
public class SaveModulePayload
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
}

public class TranslationModule
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
}

// Key
public class Translation
{
    public string LanguageCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class SaveKeyPayload
{
    public string? Id { get; set; }
    public string KeyName { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public List<Translation> Translations { get; set; } = new();
}

public class SaveKeysPayload
{
    public string ProjectKey { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public List<SaveKeysItem> Keys { get; set; } = new();
}

public class SaveKeysItem
{
    public string KeyName { get; set; } = string.Empty;
    public List<Translation> Translations { get; set; } = new();
}

public class TranslationKey
{
    public string Id { get; set; } = string.Empty;
    public string KeyName { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public List<Translation> Translations { get; set; } = new();
}

// Filters
public class GetKeysParams
{
    public string ProjectKey { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public GetKeysFilter? Filter { get; set; }
}

public class GetKeysFilter
{
    public string? Search { get; set; }
    public string? LanguageCode { get; set; }
    public bool? UntranslatedOnly { get; set; }
}

public class GetKeysByNamesPayload
{
    public string ProjectKey { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public List<string> KeyNames { get; set; } = new();
}

// Translate
public class TranslateAllPayload
{
    public string ProjectKey { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
}

public class TranslateKeyPayload
{
    public string KeyId { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
}

// Import / Export
public class ExportUilmPayload
{
    public string ProjectKey { get; set; } = string.Empty;
    public List<string> ModuleIds { get; set; } = new();
}

public class RollbackKeyPayload
{
    public string KeyId { get; set; } = string.Empty;
    public string TimelineId { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
}

// Timeline
public class KeyTimelineEntry
{
    public string Id { get; set; } = string.Empty;
    public string KeyId { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}
```

---

### KeyList Component

The key list is the central UI for localization management. It uses a plain HTML `<table>` with Tailwind and the `<Pagination />` shared component for server-side pagination.

```razor
@* Modules/Localization/Components/KeyList.razor *@
@inject ILocalizationAdminService AdminService
@inject ILocalizationService Localizer
@inject ToastService Toast
@inject LocalizationUiState UiState

<div class="card">
    <div class="p-4 space-y-4">
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-12">
            <div class="sm:col-span-4">
                <label class="label">@Localizer["common.search"]</label>
                <input type="text" class="input" placeholder="@Localizer["common.search"]"
                       @bind="_searchText" @bind:event="oninput"
                       @onkeyup="OnSearchChanged" />
            </div>
            <div class="sm:col-span-3">
                <label class="label">@Localizer["localization.keys.filterByLanguage"]</label>
                <select class="input" @onchange="OnLanguageFilterChanged">
                    <option value="">@Localizer["common.selectPlaceholder"]</option>
                    @foreach (var lang in Languages)
                    {
                        <option value="@lang.Code" selected="@(_filterLanguageCode == lang.Code)">
                            @lang.Name (@lang.Code)
                        </option>
                    }
                </select>
            </div>
            <div class="sm:col-span-3 flex items-end">
                <label class="inline-flex items-center gap-2 cursor-pointer">
                    <input type="checkbox" class="rounded border-gray-300 text-primary focus:ring-primary"
                           @bind="_untranslatedOnly" @bind:after="OnUntranslatedFilterChanged" />
                    <span class="text-sm text-gray-700">@Localizer["localization.keys.untranslatedOnly"]</span>
                </label>
            </div>
            <div class="sm:col-span-2 flex items-end">
                <button class="btn-primary w-full" @onclick="OnCreateKey">
                    @Localizer["common.create"]
                </button>
            </div>
        </div>
    </div>

    <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
                <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                        @Localizer["localization.keys.keyName"]
                    </th>
                    @foreach (var lang in Languages)
                    {
                        <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                            @lang.Name (@lang.Code)
                        </th>
                    }
                    <th class="relative px-6 py-3">
                        <span class="sr-only">@Localizer["common.actions"]</span>
                    </th>
                </tr>
            </thead>
            <tbody class="divide-y divide-gray-200 bg-white">
                @if (_loading)
                {
                    <tr>
                        <td colspan="@(Languages.Count + 2)" class="px-6 py-12 text-center text-sm text-gray-500">
                            <span class="inline-block h-5 w-5 animate-spin rounded-full border-2 border-gray-300 border-t-primary"></span>
                        </td>
                    </tr>
                }
                else if (!_keys.Any())
                {
                    <tr>
                        <td colspan="@(Languages.Count + 2)" class="px-6 py-12 text-center text-sm text-gray-500">
                            @Localizer["common.noData"]
                        </td>
                    </tr>
                }
                else
                {
                    @foreach (var key in _keys)
                    {
                        <tr class="hover:bg-gray-50">
                            <td class="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                                @key.KeyName
                            </td>
                            @foreach (var lang in Languages)
                            {
                                var translation = key.Translations.FirstOrDefault(t => t.LanguageCode == lang.Code);
                                var langCode = lang.Code;
                                <td class="px-6 py-4">
                                    <input type="text" class="input text-sm"
                                           value="@(translation?.Value ?? string.Empty)"
                                           @onchange="@(e => OnInlineEdit(key, langCode, e.Value?.ToString() ?? string.Empty))" />
                                </td>
                            }
                            <td class="whitespace-nowrap px-6 py-4 text-right text-sm">
                                <div class="flex items-center justify-end gap-1">
                                    <button class="btn-ghost btn-sm" title="@Localizer["common.edit"]"
                                            @onclick="@(() => OnEditKey.InvokeAsync(key))">
                                        @* Heroicon: outline/pencil-square *@
                                        <svg class="h-4 w-4 text-primary" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" d="m16.862 4.487 1.687-1.688a1.875 1.875 0 1 1 2.652 2.652L10.582 16.07a4.5 4.5 0 0 1-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 0 1 1.13-1.897l8.932-8.931Zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0 1 15.75 21H5.25A2.25 2.25 0 0 1 3 18.75V8.25A2.25 2.25 0 0 1 5.25 6H10" />
                                        </svg>
                                    </button>
                                    <button class="btn-ghost btn-sm" title="@Localizer["localization.keys.timeline"]"
                                            @onclick="@(() => OnViewTimeline.InvokeAsync(key))">
                                        @* Heroicon: outline/clock *@
                                        <svg class="h-4 w-4 text-gray-500" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
                                        </svg>
                                    </button>
                                    @if (_translatingKeyId == key.Id)
                                    {
                                        <span class="inline-block h-4 w-4 animate-spin rounded-full border-2 border-gray-300 border-t-secondary ml-1"></span>
                                    }
                                    else
                                    {
                                        <button class="btn-ghost btn-sm" title="@Localizer["localization.translateKey"]"
                                                @onclick="@(() => TranslateSingleKey(key))">
                                            @* Heroicon: outline/language *@
                                            <svg class="h-4 w-4 text-secondary" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                                <path stroke-linecap="round" stroke-linejoin="round" d="m10.5 21 5.25-11.25L21 21m-9-3h7.5M3 5.621a48.474 48.474 0 0 1 6-.371m0 0c1.12 0 2.233.038 3.334.114M9 5.25V3m3.334 2.364C11.176 10.658 7.69 15.08 3 17.502m9.334-12.138c.896.061 1.785.147 2.666.257m-4.589 8.495a18.023 18.023 0 0 1-3.827-5.802" />
                                            </svg>
                                        </button>
                                    }
                                    <button class="btn-ghost btn-sm" title="@Localizer["common.delete"]"
                                            @onclick="@(() => OnDeleteKey.InvokeAsync(key))">
                                        @* Heroicon: outline/trash *@
                                        <svg class="h-4 w-4 text-red-500" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                                        </svg>
                                    </button>
                                </div>
                            </td>
                        </tr>
                    }
                }
            </tbody>
        </table>
    </div>

    <Pagination CurrentPage="_page" PageSize="_pageSize" TotalCount="_totalCount"
                OnPageChanged="LoadPage" />
</div>

@code {
    [Parameter] public string ProjectKey { get; set; } = string.Empty;
    [Parameter] public string ModuleId { get; set; } = string.Empty;
    [Parameter] public List<Language> Languages { get; set; } = new();
    [Parameter] public EventCallback<TranslationKey> OnEditKey { get; set; }
    [Parameter] public EventCallback<TranslationKey> OnDeleteKey { get; set; }
    [Parameter] public EventCallback<TranslationKey> OnViewTimeline { get; set; }
    [Parameter] public EventCallback OnCreateKey { get; set; }

    private List<TranslationKey> _keys = new();
    private bool _loading;
    private int _page = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private string _searchText = string.Empty;
    private string? _filterLanguageCode;
    private bool _untranslatedOnly;
    private string? _translatingKeyId;
    private System.Timers.Timer? _debounceTimer;

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(ModuleId))
        {
            _page = 1;
            await LoadKeysAsync();
        }
    }

    private async Task LoadKeysAsync()
    {
        _loading = true;
        StateHasChanged();

        try
        {
            var queryParams = new GetKeysParams
            {
                ProjectKey = ProjectKey,
                ModuleId = ModuleId,
                PageNumber = _page,
                PageSize = _pageSize,
                Filter = new GetKeysFilter
                {
                    Search = _searchText,
                    LanguageCode = _filterLanguageCode,
                    UntranslatedOnly = _untranslatedOnly
                }
            };

            var response = await AdminService.GetKeys(queryParams);
            var result = await response.Content.ReadFromJsonAsync<KeysResponse>();

            _keys = result?.Data ?? new();
            _totalCount = result?.TotalCount ?? 0;
        }
        catch
        {
            Toast.Show(Localizer["common.error"], ToastLevel.Error);
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }

    private void OnSearchChanged()
    {
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
        _debounceTimer = new System.Timers.Timer(300);
        _debounceTimer.Elapsed += async (_, _) =>
        {
            _debounceTimer?.Stop();
            _page = 1;
            await InvokeAsync(LoadKeysAsync);
        };
        _debounceTimer.AutoReset = false;
        _debounceTimer.Start();
    }

    private async Task OnLanguageFilterChanged(ChangeEventArgs e)
    {
        _filterLanguageCode = string.IsNullOrEmpty(e.Value?.ToString()) ? null : e.Value.ToString();
        _page = 1;
        await LoadKeysAsync();
    }

    private async Task OnUntranslatedFilterChanged()
    {
        _page = 1;
        await LoadKeysAsync();
    }

    private async Task LoadPage(int page)
    {
        _page = page;
        await LoadKeysAsync();
    }

    private async Task OnInlineEdit(TranslationKey key, string languageCode, string value)
    {
        var translation = key.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (translation is not null)
            translation.Value = value;
        else
            key.Translations.Add(new Translation { LanguageCode = languageCode, Value = value });

        var payload = new SaveKeyPayload
        {
            Id = key.Id,
            KeyName = key.KeyName,
            ModuleId = key.ModuleId,
            ProjectKey = key.ProjectKey,
            Translations = key.Translations
        };
        await AdminService.SaveKey(payload);
    }

    private async Task TranslateSingleKey(TranslationKey key)
    {
        _translatingKeyId = key.Id;
        StateHasChanged();

        try
        {
            var payload = new TranslateKeyPayload
            {
                KeyId = key.Id,
                ProjectKey = ProjectKey,
                LanguageCode = _filterLanguageCode ?? Languages.FirstOrDefault()?.Code ?? "en"
            };
            await AdminService.TranslateKey(payload);
            Toast.Show(Localizer["common.success"], ToastLevel.Success);
            await LoadKeysAsync();
        }
        catch
        {
            Toast.Show(Localizer["localization.error.translationFailed"], ToastLevel.Error);
        }
        finally
        {
            _translatingKeyId = null;
            StateHasChanged();
        }
    }

    public async Task Reload() => await LoadKeysAsync();

    private record KeysResponse(List<TranslationKey> Data, int TotalCount, bool Success);
}
```

---

### AI Translation

**TranslateAll** — triggered from the keys page toolbar:

```razor
<button class="btn-secondary inline-flex items-center gap-2"
        disabled="@_isTranslatingAll"
        @onclick="HandleTranslateAll">
    @if (_isTranslatingAll)
    {
        <span class="inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
    }
    else
    {
        @* Heroicon: outline/language *@
        <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="m10.5 21 5.25-11.25L21 21m-9-3h7.5M3 5.621a48.474 48.474 0 0 1 6-.371m0 0c1.12 0 2.233.038 3.334.114M9 5.25V3m3.334 2.364C11.176 10.658 7.69 15.08 3 17.502m9.334-12.138c.896.061 1.785.147 2.666.257m-4.589 8.495a18.023 18.023 0 0 1-3.827-5.802" />
        </svg>
    }
    @Localizer["localization.translateAll"]
</button>

@code {
    private bool _isTranslatingAll;

    private async Task HandleTranslateAll()
    {
        _isTranslatingAll = true;
        StateHasChanged();

        try
        {
            var payload = new TranslateAllPayload
            {
                ProjectKey = _projectKey,
                ModuleId = UiState.SelectedModuleId!
            };
            await AdminService.TranslateAll(payload);
            Toast.Show(Localizer["localization.translateAllSuccess"], ToastLevel.Success);
            await _keyList!.Reload();
        }
        catch
        {
            Toast.Show(Localizer["localization.error.translationFailed"], ToastLevel.Error);
        }
        finally
        {
            _isTranslatingAll = false;
            StateHasChanged();
        }
    }
}
```

**TranslateKey** — inline on specific row. Shows a spinning indicator on the row being translated. Updates value in-place after success. See the `TranslateSingleKey` method in the KeyList component above.

---

### File Import

```razor
@* Modules/Localization/Components/ImportExport.razor (import section) *@
@inject ILocalizationAdminService AdminService
@inject ILocalizationService Localizer
@inject ToastService Toast

<div class="card p-4 space-y-4">
    <h3 class="text-lg font-semibold text-gray-900">@Localizer["localization.import.title"]</h3>

    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
            <label class="label">@Localizer["localization.import.selectModule"]</label>
            <select class="input" @bind="_importModuleId">
                <option value="">@Localizer["common.selectPlaceholder"]</option>
                @foreach (var mod in Modules)
                {
                    <option value="@mod.Id">@mod.Name</option>
                }
            </select>
        </div>
        <div>
            <label class="label">@Localizer["localization.import.selectLanguage"]</label>
            <select class="input" @bind="_importLanguageCode">
                <option value="">@Localizer["common.selectPlaceholder"]</option>
                @foreach (var lang in Languages)
                {
                    <option value="@lang.Code">@lang.Name (@lang.Code)</option>
                }
            </select>
        </div>
    </div>

    <div class="mt-3">
        <label class="flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed border-gray-300 bg-gray-50 px-6 py-8 transition hover:border-primary hover:bg-primary/5">
            <svg class="h-10 w-10 text-primary" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M3 16.5v2.25A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75V16.5m-13.5-9L12 3m0 0 4.5 4.5M12 3v13.5" />
            </svg>
            <span class="mt-2 text-sm text-gray-600">@Localizer["localization.import.dropFile"]</span>
            <span class="text-xs text-gray-400">.json</span>
            <InputFile OnChange="OnFileSelected" accept=".json" class="hidden" />
        </label>
    </div>

    @if (_keyCountPreview > 0)
    {
        <div class="rounded-md bg-blue-50 border border-blue-200 p-4">
            <p class="text-sm text-blue-700">
                @Localizer["localization.import.keyCount", _keyCountPreview]
            </p>
        </div>

        <button class="btn-primary"
                disabled="@(_isUploading || string.IsNullOrEmpty(_importModuleId) || string.IsNullOrEmpty(_importLanguageCode))"
                @onclick="HandleImport">
            @if (_isUploading)
            {
                <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
            }
            @Localizer["localization.import.upload"]
        </button>
    }
</div>

@code {
    [Parameter] public string ProjectKey { get; set; } = string.Empty;
    [Parameter] public List<TranslationModule> Modules { get; set; } = new();
    [Parameter] public List<Language> Languages { get; set; } = new();
    [Parameter] public EventCallback OnImportComplete { get; set; }

    private string? _importModuleId;
    private string? _importLanguageCode;
    private IBrowserFile? _selectedFile;
    private int _keyCountPreview;
    private bool _isUploading;

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (!file.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            Toast.Show(Localizer["localization.import.invalidFormat"], ToastLevel.Warning);
            return;
        }

        _selectedFile = file;

        // Parse file to show key count preview
        using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
        var json = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream);
        _keyCountPreview = json?.Count ?? 0;
        StateHasChanged();
    }

    private async Task HandleImport()
    {
        if (_selectedFile is null || string.IsNullOrEmpty(_importModuleId) || string.IsNullOrEmpty(_importLanguageCode))
            return;

        _isUploading = true;
        StateHasChanged();

        try
        {
            var formData = new MultipartFormDataContent();
            var fileStream = _selectedFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            formData.Add(new StreamContent(fileStream), "file", _selectedFile.Name);
            formData.Add(new StringContent(ProjectKey), "projectKey");
            formData.Add(new StringContent(_importModuleId), "moduleId");
            formData.Add(new StringContent(_importLanguageCode), "languageCode");

            var response = await AdminService.ImportUilm(formData);
            response.EnsureSuccessStatusCode();

            Toast.Show(Localizer["common.success"], ToastLevel.Success);
            _keyCountPreview = 0;
            _selectedFile = null;
            await OnImportComplete.InvokeAsync();
        }
        catch
        {
            Toast.Show(Localizer["common.error"], ToastLevel.Error);
        }
        finally
        {
            _isUploading = false;
            StateHasChanged();
        }
    }
}
```

Before upload: validate `.json` extension, show key count preview, require module + language selection. After upload: refresh key list via `OnImportComplete` callback.

---

### File Export

`ExportUilm` returns file data. Trigger download using JS interop with `URL.createObjectURL`:

```razor
@inject IJSRuntime JS

private async Task HandleExport()
{
    var payload = new ExportUilmPayload
    {
        ProjectKey = _projectKey,
        ModuleIds = new List<string> { UiState.SelectedModuleId! }
    };

    var response = await AdminService.ExportUilm(payload);
    response.EnsureSuccessStatusCode();

    var bytes = await response.Content.ReadAsByteArrayAsync();
    var fileName = $"localization-export-{DateTime.Now:yyyyMMdd}.json";

    await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, Convert.ToBase64String(bytes));
}
```

JS interop helper (add to `wwwroot/index.html` or a dedicated JS file):

```javascript
window.downloadFileFromBytes = (fileName, base64) => {
    const bytes = Uint8Array.from(atob(base64), c => c.charCodeAt(0));
    const blob = new Blob([bytes], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
};
```

---

### Route Definitions

```razor
@page "/localization/languages"    -> LanguagesPage.razor
@page "/localization/modules"      -> ModulesPage.razor
@page "/localization/keys"         -> KeysPage.razor
@page "/localization/settings"     -> SettingsPage.razor
```

All localization routes require `@attribute [Authorize]`.

---

### Error Handling

```csharp
// Modules/Localization/Models/LocalizationErrorMap.cs
public static class LocalizationErrorMap
{
    public static string GetMessage(string errorCode, ILocalizationService localizer) => errorCode switch
    {
        "LANGUAGE_ALREADY_EXISTS" => localizer["localization.error.languageAlreadyExists"],
        "MODULE_ALREADY_EXISTS" => localizer["localization.error.moduleAlreadyExists"],
        "KEY_ALREADY_EXISTS" => localizer["localization.error.keyAlreadyExists"],
        "TRANSLATION_FAILED" => localizer["localization.error.translationFailed"],
        _ => localizer["common.error"],
    };
}
```

Expected user-facing messages:

| Error Code | Message |
|------------|---------|
| `LANGUAGE_ALREADY_EXISTS` | "A language with this code already exists" |
| `MODULE_ALREADY_EXISTS` | "A module with this name already exists" |
| `KEY_ALREADY_EXISTS` | "A key with this name already exists in this module" |
| `TRANSLATION_FAILED` | "AI translation failed. Please try again." |

Rules:
- Show validation errors inline below the relevant field using `<ValidationMessage>` with `class="text-red-500 text-xs mt-1"`
- Show API-level errors in `<ErrorAlert />` (from shared components)
- Never expose raw error codes to the user
- All error messages use `ILocalizationService` — no hardcoded strings
- Use `ToastService` for success/error/info notifications — never `alert()` or `console.log()`
