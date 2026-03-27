# Localization — Frontend Guide (Blazor)

This file covers two distinct concerns:

1. **App-level i18n** — how every feature in the app uses translation keys (mandatory for all features)
2. **Localization admin UI** — the management pages for languages, modules, and keys

Always read `core/frontend-blazor.md` first, then apply the additions here.

---

## Part 1: App-Level i18n Integration

This section applies to **every feature** in the app, not just the localization admin pages.

### Startup — Load Translations

On app boot, `ILocalizationService.LoadTranslationsAsync()` fetches the UILM file for the stored language before rendering protected routes. This is already handled in `Program.cs` (see `core/app-scaffold-blazor.md`).

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
│   ├── LanguageList.razor           ← table of languages with default badge and delete
│   ├── LanguageForm.razor           ← add/edit language form
│   ├── ModuleList.razor             ← list of translation modules
│   ├── ModuleForm.razor             ← add/edit module form
│   ├── KeyList.razor                ← paginated, filterable key table (central UI)
│   ├── KeyForm.razor                ← create/edit key with per-language translation inputs
│   ├── KeyTimeline.razor            ← version history drawer for a key
│   ├── TranslateProgress.razor      ← progress indicator for AI translate-all
│   └── ImportExport.razor           ← file upload and export controls
├── Pages/
│   ├── LanguagesPage.razor          ← language management page
│   ├── ModulesPage.razor            ← module management page
│   ├── KeysPage.razor               ← key management page (main localization editor)
│   └── SettingsPage.razor           ← webhook and project-level config
├── Services/
│   └── LocalizationAdminService.cs  ← all localization admin API calls
└── Models/
    ├── LocalizationModels.cs        ← C# types for all payloads and responses
    └── LocalizationValidators.cs    ← FluentValidation validators
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

The key list is the central UI for localization management. It uses `MudTable` with server-side pagination.

```razor
@* Modules/Localization/Components/KeyList.razor *@
@inject ILocalizationAdminService AdminService
@inject ILocalizationService Localizer
@inject ISnackbar Snackbar
@inject LocalizationUiState UiState

<MudPaper Class="pa-4">
    <MudGrid>
        <MudItem xs="12" sm="4">
            <MudTextField @bind-Value="_searchText"
                          Label="@Localizer["common.search"]"
                          Adornment="Adornment.Start"
                          AdornmentIcon="@Icons.Material.Filled.Search"
                          Immediate="true"
                          DebounceInterval="300"
                          OnDebounceIntervalElapsed="OnSearchChanged" />
        </MudItem>
        <MudItem xs="12" sm="3">
            <MudSelect T="string" Value="_filterLanguageCode" ValueChanged="OnLanguageFilterChanged"
                       Label="@Localizer["localization.keys.filterByLanguage"]"
                       Clearable="true">
                @foreach (var lang in Languages)
                {
                    <MudSelectItem Value="@lang.Code">@lang.Name (@lang.Code)</MudSelectItem>
                }
            </MudSelect>
        </MudItem>
        <MudItem xs="12" sm="3">
            <MudCheckBox @bind-Value="_untranslatedOnly"
                         Label="@Localizer["localization.keys.untranslatedOnly"]"
                         Class="mt-3"
                         ValueChanged="OnUntranslatedFilterChanged" />
        </MudItem>
        <MudItem xs="12" sm="2" Class="d-flex align-end">
            <MudButton Color="Color.Primary" Variant="Variant.Filled"
                       StartIcon="@Icons.Material.Filled.Add"
                       OnClick="OnCreateKey">
                @Localizer["common.create"]
            </MudButton>
        </MudItem>
    </MudGrid>

    <MudTable ServerData="LoadKeys" @ref="_table" Dense="true" Hover="true" Class="mt-4">
        <HeaderContent>
            <MudTh>@Localizer["localization.keys.keyName"]</MudTh>
            @foreach (var lang in Languages)
            {
                <MudTh>@lang.Name (@lang.Code)</MudTh>
            }
            <MudTh>@Localizer["common.actions"]</MudTh>
        </HeaderContent>
        <RowTemplate>
            <MudTd DataLabel="@Localizer["localization.keys.keyName"]">
                @context.KeyName
            </MudTd>
            @foreach (var lang in Languages)
            {
                var translation = context.Translations.FirstOrDefault(t => t.LanguageCode == lang.Code);
                <MudTd>
                    <MudTextField T="string"
                                  Value="@(translation?.Value ?? string.Empty)"
                                  ValueChanged="@(v => OnInlineEdit(context, lang.Code, v))"
                                  Variant="Variant.Text"
                                  Immediate="true" />
                </MudTd>
            }
            <MudTd>
                <MudIconButton Icon="@Icons.Material.Filled.Edit" Color="Color.Primary"
                               Size="Size.Small" OnClick="@(() => OnEditKey.InvokeAsync(context))" />
                <MudIconButton Icon="@Icons.Material.Filled.History" Color="Color.Default"
                               Size="Size.Small" OnClick="@(() => OnViewTimeline.InvokeAsync(context))" />
                @if (_translatingKeyId == context.Id)
                {
                    <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="ml-1" />
                }
                else
                {
                    <MudIconButton Icon="@Icons.Material.Filled.Translate" Color="Color.Secondary"
                                   Size="Size.Small" OnClick="@(() => TranslateSingleKey(context))" />
                }
                <MudIconButton Icon="@Icons.Material.Filled.Delete" Color="Color.Error"
                               Size="Size.Small" OnClick="@(() => OnDeleteKey.InvokeAsync(context))" />
            </MudTd>
        </RowTemplate>
        <PagerContent>
            <MudTablePager />
        </PagerContent>
    </MudTable>
</MudPaper>

@code {
    [Parameter] public string ProjectKey { get; set; } = string.Empty;
    [Parameter] public string ModuleId { get; set; } = string.Empty;
    [Parameter] public List<Language> Languages { get; set; } = new();
    [Parameter] public EventCallback<TranslationKey> OnEditKey { get; set; }
    [Parameter] public EventCallback<TranslationKey> OnDeleteKey { get; set; }
    [Parameter] public EventCallback<TranslationKey> OnViewTimeline { get; set; }
    [Parameter] public EventCallback OnCreateKey { get; set; }

    private MudTable<TranslationKey>? _table;
    private string _searchText = string.Empty;
    private string? _filterLanguageCode;
    private bool _untranslatedOnly;
    private string? _translatingKeyId;

    private async Task<TableData<TranslationKey>> LoadKeys(TableState state, CancellationToken ct)
    {
        var queryParams = new GetKeysParams
        {
            ProjectKey = ProjectKey,
            ModuleId = ModuleId,
            PageNumber = state.Page + 1,
            PageSize = state.PageSize,
            Filter = new GetKeysFilter
            {
                Search = _searchText,
                LanguageCode = _filterLanguageCode,
                UntranslatedOnly = _untranslatedOnly
            }
        };

        var response = await AdminService.GetKeys(queryParams);
        var result = await response.Content.ReadFromJsonAsync<KeysResponse>(cancellationToken: ct);

        return new TableData<TranslationKey>
        {
            Items = result?.Data ?? new(),
            TotalItems = result?.TotalCount ?? 0
        };
    }

    private async Task OnSearchChanged(string value)
    {
        _searchText = value;
        if (_table is not null) await _table.ReloadServerData();
    }

    private async Task OnLanguageFilterChanged(string? value)
    {
        _filterLanguageCode = value;
        if (_table is not null) await _table.ReloadServerData();
    }

    private async Task OnUntranslatedFilterChanged(bool value)
    {
        _untranslatedOnly = value;
        if (_table is not null) await _table.ReloadServerData();
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
            Snackbar.Add(Localizer["common.success"], Severity.Success);
            if (_table is not null) await _table.ReloadServerData();
        }
        catch
        {
            Snackbar.Add(Localizer["localization.error.translationFailed"], Severity.Error);
        }
        finally
        {
            _translatingKeyId = null;
            StateHasChanged();
        }
    }

    public async Task Reload()
    {
        if (_table is not null) await _table.ReloadServerData();
    }

    private record KeysResponse(List<TranslationKey> Data, int TotalCount, bool Success);
}
```

---

### AI Translation

**TranslateAll** — triggered from the keys page toolbar:

```razor
<MudButton Color="Color.Secondary" Variant="Variant.Filled"
           StartIcon="@Icons.Material.Filled.Translate"
           Disabled="_isTranslatingAll"
           OnClick="HandleTranslateAll">
    @if (_isTranslatingAll)
    {
        <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
    }
    @Localizer["localization.translateAll"]
</MudButton>

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
            Snackbar.Add(Localizer["localization.translateAllSuccess"], Severity.Success);
            await _keyList!.Reload();
        }
        catch
        {
            Snackbar.Add(Localizer["localization.error.translationFailed"], Severity.Error);
        }
        finally
        {
            _isTranslatingAll = false;
            StateHasChanged();
        }
    }
}
```

**TranslateKey** — inline on specific row. Shows `MudProgressCircular` on the row being translated. Updates value in-place after success. See the `TranslateSingleKey` method in the KeyList component above.

---

### File Import

```razor
@* Modules/Localization/Components/ImportExport.razor (import section) *@
@inject ILocalizationAdminService AdminService
@inject ILocalizationService Localizer
@inject ISnackbar Snackbar

<MudPaper Class="pa-4">
    <MudText Typo="Typo.h6" Class="mb-3">@Localizer["localization.import.title"]</MudText>

    <MudGrid>
        <MudItem xs="12" sm="4">
            <MudSelect T="string" @bind-Value="_importModuleId"
                       Label="@Localizer["localization.import.selectModule"]"
                       Required="true">
                @foreach (var mod in Modules)
                {
                    <MudSelectItem Value="@mod.Id">@mod.Name</MudSelectItem>
                }
            </MudSelect>
        </MudItem>
        <MudItem xs="12" sm="4">
            <MudSelect T="string" @bind-Value="_importLanguageCode"
                       Label="@Localizer["localization.import.selectLanguage"]"
                       Required="true">
                @foreach (var lang in Languages)
                {
                    <MudSelectItem Value="@lang.Code">@lang.Name (@lang.Code)</MudSelectItem>
                }
            </MudSelect>
        </MudItem>
    </MudGrid>

    <MudFileUpload T="IBrowserFile" FilesChanged="OnFileSelected" Accept=".json" Class="mt-3">
        <ActivatorContent>
            <MudPaper Outlined="true" Class="d-flex flex-column align-center justify-center pa-6 cursor-pointer"
                      Style="border-style: dashed;">
                <MudIcon Icon="@Icons.Material.Filled.UploadFile" Size="Size.Large" Color="Color.Primary" />
                <MudText Typo="Typo.body1" Class="mt-2">@Localizer["localization.import.dropFile"]</MudText>
                <MudText Typo="Typo.caption" Color="Color.Default">.json</MudText>
            </MudPaper>
        </ActivatorContent>
    </MudFileUpload>

    @if (_keyCountPreview > 0)
    {
        <MudAlert Severity="Severity.Info" Class="mt-3">
            @Localizer["localization.import.keyCount", _keyCountPreview]
        </MudAlert>
        <MudButton Color="Color.Primary" Variant="Variant.Filled" Class="mt-2"
                   Disabled="_isUploading || string.IsNullOrEmpty(_importModuleId) || string.IsNullOrEmpty(_importLanguageCode)"
                   OnClick="HandleImport">
            @if (_isUploading)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
            }
            @Localizer["localization.import.upload"]
        </MudButton>
    }
</MudPaper>

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

    private async Task OnFileSelected(IBrowserFile file)
    {
        if (!file.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            Snackbar.Add(Localizer["localization.import.invalidFormat"], Severity.Warning);
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

            Snackbar.Add(Localizer["common.success"], Severity.Success);
            _keyCountPreview = 0;
            _selectedFile = null;
            await OnImportComplete.InvokeAsync();
        }
        catch
        {
            Snackbar.Add(Localizer["common.error"], Severity.Error);
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
@page "/localization/languages"    → LanguagesPage.razor
@page "/localization/modules"      → ModulesPage.razor
@page "/localization/keys"         → KeysPage.razor
@page "/localization/settings"     → SettingsPage.razor
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
- Show validation errors inline below the relevant field
- Show API-level errors in `<ErrorAlert />` or `<MudAlert Severity="Severity.Error">`
- Never expose raw error codes to the user
- All error messages use `ILocalizationService` — no hardcoded strings
