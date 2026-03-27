# Data Management Frontend (Blazor)

This file extends `core/frontend-blazor.md` with data-management-specific patterns for the SELISE Blocks data management domain.
Always read `core/frontend-blazor.md` first, then apply the overrides and additions here.

**Stack:** .NET 10 Blazor WebAssembly, MudBlazor for UI, scoped services for state, HttpClient + DelegatingHandler for HTTP, FluentValidation for validation, ILocalizationService for i18n, GraphQL.Client for GraphQL queries.

---

## Module Structure

All data management UI lives in `Modules/DataManagement/`:

```
Modules/DataManagement/
├── Components/
│   ├── SchemaBuilder.razor           ← visual field definition UI
│   ├── FieldRow.razor                ← single field row in schema builder
│   ├── FieldTypeSelect.razor         ← field type dropdown
│   ├── FileUpload.razor              ← drag-drop upload with progress
│   ├── UploadProgress.razor          ← per-file progress bar
│   ├── FileDropZone.razor            ← drag-drop target area
│   ├── FileBrowser.razor             ← DMS folder/file split-pane view
│   ├── FolderTree.razor              ← recursive folder hierarchy (left pane)
│   └── FileList.razor                ← files in selected folder (right pane)
├── Pages/
│   ├── SchemasPage.razor             ← schema list (@page "/data-management/schemas")
│   ├── SchemaDetailPage.razor        ← fields, validations, access (@page "/data-management/schemas/{Id}")
│   └── FilesPage.razor               ← file manager (@page "/data-management/files")
├── Services/
│   └── DataManagementService.cs      ← all API call methods
└── Models/
    ├── DataManagementModels.cs       ← C# types (enums, classes, records)
    └── DataManagementValidators.cs   ← FluentValidation validators
```

---

## Service Layer

```csharp
// Modules/DataManagement/Services/DataManagementService.cs
public interface IDataManagementService
{
    // Schema
    Task<HttpResponseMessage> GetSchemas(object queryParams);
    Task<HttpResponseMessage> GetSchemaById(object queryParams);
    Task<HttpResponseMessage> GetSchemasAggregation(object queryParams);
    Task<HttpResponseMessage> GetSchemaCollections(object queryParams);
    Task<HttpResponseMessage> GetSchemaByCollection(object queryParams);
    Task<HttpResponseMessage> DefineSchema(DefineSchemaPayload payload);
    Task<HttpResponseMessage> UpdateSchema(DefineSchemaPayload payload);
    Task<HttpResponseMessage> SaveSchemaInfo(SaveSchemaInfoPayload payload);
    Task<HttpResponseMessage> UpdateSchemaInfo(SaveSchemaInfoPayload payload);
    Task<HttpResponseMessage> SaveSchemaFields(SaveSchemaFieldsPayload payload);
    Task<HttpResponseMessage> DeleteSchema(object queryParams);
    Task<HttpResponseMessage> GetUnadaptedChanges(object queryParams);
    Task<HttpResponseMessage> ReloadConfiguration();

    // DataSource
    Task<HttpResponseMessage> GetDataSource();
    Task<HttpResponseMessage> AddDataSource(object payload);
    Task<HttpResponseMessage> UpdateDataSource(object payload);

    // DataAccess
    Task<HttpResponseMessage> ChangeSecurity(object payload);
    Task<HttpResponseMessage> CreateAccessPolicy(object payload);
    Task<HttpResponseMessage> UpdateAccessPolicy(object payload);
    Task<HttpResponseMessage> DeleteAccessPolicy(object queryParams);
    Task<HttpResponseMessage> GetAccessPolicies(object queryParams);

    // DataValidation
    Task<HttpResponseMessage> GetValidations(object queryParams);
    Task<HttpResponseMessage> GetValidationById(object queryParams);
    Task<HttpResponseMessage> CreateValidation(object payload);
    Task<HttpResponseMessage> UpdateValidation(object payload);
    Task<HttpResponseMessage> DeleteValidation(object queryParams);
    Task<HttpResponseMessage> GetSchemaValidations(object queryParams);
    Task<HttpResponseMessage> GetFieldValidation(object queryParams);

    // Files — S3 pre-signed upload
    Task<HttpResponseMessage> GetPreSignedUploadUrl(object payload);
    Task UploadToS3(string url, Stream fileStream, string contentType);
    Task<HttpResponseMessage> UpdateFileInfo(object payload);

    // Files — DMS
    Task<HttpResponseMessage> UploadToDms(MultipartFormDataContent formData);
    Task<HttpResponseMessage> UploadToLocalStorage(MultipartFormDataContent formData);
    Task<HttpResponseMessage> GetFile(object queryParams);
    Task<HttpResponseMessage> GetFiles(object payload);
    Task<HttpResponseMessage> GetFilesInfo(object payload);
    Task<HttpResponseMessage> GetDmsFiles(object payload);
    Task<HttpResponseMessage> CreateFolder(object payload);
    Task<HttpResponseMessage> DeleteFile(object payload);
    Task<HttpResponseMessage> DeleteFolder(object payload);

    // DataManage
    Task<HttpResponseMessage> GetMockData();
    Task<HttpResponseMessage> DeleteMockData(object payload);
}

public class DataManagementService : IDataManagementService
{
    private readonly HttpClient _http;
    private const string Base = "/uds/v1";

    public DataManagementService(HttpClient http) => _http = http;

    // ── Schema ──────────────────────────────────────────────────────────
    public Task<HttpResponseMessage> GetSchemas(object queryParams) =>
        _http.GetAsync($"{Base}/schemas?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> GetSchemaById(object queryParams) =>
        _http.GetAsync($"{Base}/schemas/get-by-id?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> GetSchemasAggregation(object queryParams) =>
        _http.GetAsync($"{Base}/schemas/aggregation?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> GetSchemaCollections(object queryParams) =>
        _http.GetAsync($"{Base}/schemas/collections?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> GetSchemaByCollection(object queryParams) =>
        _http.GetAsync($"{Base}/schemas/get-by-collection?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> DefineSchema(DefineSchemaPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/schemas/define", payload);

    public Task<HttpResponseMessage> UpdateSchema(DefineSchemaPayload payload) =>
        _http.PutAsJsonAsync($"{Base}/schemas/define", payload);

    public Task<HttpResponseMessage> SaveSchemaInfo(SaveSchemaInfoPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/schemas/info", payload);

    public Task<HttpResponseMessage> UpdateSchemaInfo(SaveSchemaInfoPayload payload) =>
        _http.PutAsJsonAsync($"{Base}/schemas/info", payload);

    public Task<HttpResponseMessage> SaveSchemaFields(SaveSchemaFieldsPayload payload) =>
        _http.PostAsJsonAsync($"{Base}/schemas/fields", payload);

    public Task<HttpResponseMessage> DeleteSchema(object queryParams) =>
        _http.DeleteAsync($"{Base}/schemas?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> GetUnadaptedChanges(object queryParams) =>
        _http.GetAsync($"{Base}/schemas/unadapted-changes?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> ReloadConfiguration() =>
        _http.PostAsync($"{Base}/configurations/reload", null);

    // ── DataSource ──────────────────────────────────────────────────────
    public Task<HttpResponseMessage> GetDataSource() =>
        _http.GetAsync($"{Base}/data-sources/get");

    public Task<HttpResponseMessage> AddDataSource(object payload) =>
        _http.PostAsJsonAsync($"{Base}/data-sources/add", payload);

    public Task<HttpResponseMessage> UpdateDataSource(object payload) =>
        _http.PutAsJsonAsync($"{Base}/data-sources/update", payload);

    // ── DataAccess ──────────────────────────────────────────────────────
    public Task<HttpResponseMessage> ChangeSecurity(object payload) =>
        _http.PostAsJsonAsync($"{Base}/data-access/security/change", payload);

    public Task<HttpResponseMessage> CreateAccessPolicy(object payload) =>
        _http.PostAsJsonAsync($"{Base}/data-access/policy/create", payload);

    public Task<HttpResponseMessage> UpdateAccessPolicy(object payload) =>
        _http.PutAsJsonAsync($"{Base}/data-access/policy/update", payload);

    public Task<HttpResponseMessage> DeleteAccessPolicy(object queryParams) =>
        _http.DeleteAsync($"{Base}/data-access/policy?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> GetAccessPolicies(object queryParams) =>
        _http.GetAsync($"{Base}/data-access/policy/get?{queryParams.ToQueryString()}");

    // ── DataValidation ──────────────────────────────────────────────────
    public Task<HttpResponseMessage> GetValidations(object queryParams) =>
        _http.GetAsync($"{Base}/data-validations?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> GetValidationById(object queryParams) =>
        _http.GetAsync($"{Base}/data-validations/get-by-id?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> CreateValidation(object payload) =>
        _http.PostAsJsonAsync($"{Base}/data-validations", payload);

    public Task<HttpResponseMessage> UpdateValidation(object payload) =>
        _http.PutAsJsonAsync($"{Base}/data-validations", payload);

    public Task<HttpResponseMessage> DeleteValidation(object queryParams) =>
        _http.DeleteAsync($"{Base}/data-validations?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> GetSchemaValidations(object queryParams) =>
        _http.GetAsync($"{Base}/data-validations/schema?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> GetFieldValidation(object queryParams) =>
        _http.GetAsync($"{Base}/data-validations/field?{queryParams.ToQueryString()}");

    // ── Files — S3 pre-signed upload ────────────────────────────────────
    public Task<HttpResponseMessage> GetPreSignedUploadUrl(object payload) =>
        _http.PostAsJsonAsync($"{Base}/Files/GetPreSignedUrlForUpload", payload);

    public async Task UploadToS3(string url, Stream fileStream, string contentType)
    {
        using var client = new HttpClient(); // direct S3 — no auth header
        var content = new StreamContent(fileStream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        await client.PutAsync(url, content);
    }

    public Task<HttpResponseMessage> UpdateFileInfo(object payload) =>
        _http.PutAsJsonAsync($"{Base}/Files/UpdateFileAdditionalInfo", payload);

    // ── Files — DMS ─────────────────────────────────────────────────────
    public Task<HttpResponseMessage> UploadToDms(MultipartFormDataContent formData) =>
        _http.PostAsync($"{Base}/Files/UploadFile", formData);

    public Task<HttpResponseMessage> UploadToLocalStorage(MultipartFormDataContent formData) =>
        _http.PostAsync($"{Base}/Files/UploadToLocalStorage", formData);

    public Task<HttpResponseMessage> GetFile(object queryParams) =>
        _http.GetAsync($"{Base}/Files/GetFile?{queryParams.ToQueryString()}");

    public Task<HttpResponseMessage> GetFiles(object payload) =>
        _http.PostAsJsonAsync($"{Base}/Files/GetFiles", payload);

    public Task<HttpResponseMessage> GetFilesInfo(object payload) =>
        _http.PostAsJsonAsync($"{Base}/Files/GetFilesInfo", payload);

    public Task<HttpResponseMessage> GetDmsFiles(object payload) =>
        _http.PostAsJsonAsync($"{Base}/Files/GetDmsFileAndFolder", payload);

    public Task<HttpResponseMessage> CreateFolder(object payload) =>
        _http.PostAsJsonAsync($"{Base}/Files/CreateFolder", payload);

    public Task<HttpResponseMessage> DeleteFile(object payload) =>
        _http.PostAsJsonAsync($"{Base}/Files/DeleteFile", payload);

    public Task<HttpResponseMessage> DeleteFolder(object payload) =>
        _http.PostAsJsonAsync($"{Base}/Files/DeleteFolder", payload);

    // ── DataManage ──────────────────────────────────────────────────────
    public Task<HttpResponseMessage> GetMockData() =>
        _http.GetAsync($"{Base}/data-manage/mock-data");

    public Task<HttpResponseMessage> DeleteMockData(object payload) =>
        _http.PostAsJsonAsync($"{Base}/data-manage/mock-data/delete", payload);
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<IDataManagementService, DataManagementService>();
```

---

## C# Models

```csharp
// Modules/DataManagement/Models/DataManagementModels.cs

// ── Enums ───────────────────────────────────────────────────────────────

public enum SchemaType { Collection, SingleObject }
public enum FieldType { String, Number, Boolean, Date, ObjectId, Object, Array }
public enum SecurityType { Public, Private, RoleBased }
public enum Operation { Read, Create, Update, Delete }
public enum AccessModifier { Public, Private }
public enum ValidationType { Required, MinLength, MaxLength, Regex, Min, Max, Email, Unique }

// ── Schema ──────────────────────────────────────────────────────────────

public class Schema
{
    public string Id { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
    public SchemaType SchemaType { get; set; }
    public string? Description { get; set; }
    public List<SchemaField>? Fields { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SchemaField
{
    public string Name { get; set; } = string.Empty;
    public FieldType Type { get; set; } = FieldType.String;
    public bool IsArray { get; set; }
    public bool IsRequired { get; set; }
    public string? Description { get; set; }
    public string? DefaultValue { get; set; }
}

public class DefineSchemaPayload
{
    public string CollectionName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;
    public SchemaType SchemaType { get; set; }
    public string? Description { get; set; }
}

public class SaveSchemaInfoPayload
{
    public string SchemaId { get; set; } = string.Empty;
    public List<SchemaField> Fields { get; set; } = new();
    public string ProjectKey { get; set; } = string.Empty;
}

public class SaveSchemaFieldsPayload
{
    public string SchemaId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public bool IsArray { get; set; }
    public bool IsRequired { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
}

// ── Access Policy ───────────────────────────────────────────────────────

public class AccessPolicy
{
    public string ItemId { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public List<string> AllowedRoles { get; set; } = new();
    public List<Operation> Operations { get; set; } = new();
    public string ProjectKey { get; set; } = string.Empty;
}

// ── Validation ──────────────────────────────────────────────────────────

public class ValidationRule
{
    public string Id { get; set; } = string.Empty;
    public string SchemaId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public ValidationType Type { get; set; }
    public string? Value { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

// ── Files ───────────────────────────────────────────────────────────────

public class DmsFile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsFolder { get; set; }
    public string? ParentDirectoryId { get; set; }
    public long? Size { get; set; }
    public string? ContentType { get; set; }
    public AccessModifier AccessModifier { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string>? Tags { get; set; }
}

public record UploadProgress(string FileName, int Progress, string Status);
```

### FluentValidation Validators

```csharp
// Modules/DataManagement/Models/DataManagementValidators.cs
using FluentValidation;

public class DefineSchemaPayloadValidator : AbstractValidator<DefineSchemaPayload>
{
    public DefineSchemaPayloadValidator(ILocalizationService localizer)
    {
        RuleFor(x => x.SchemaName)
            .NotEmpty().WithMessage(localizer["dataManagement.validation.schemaNameRequired"])
            .MaximumLength(100).WithMessage(localizer["dataManagement.validation.schemaNameMaxLength"]);

        RuleFor(x => x.CollectionName)
            .NotEmpty().WithMessage(localizer["dataManagement.validation.collectionNameRequired"])
            .Matches(@"^[a-z][a-z0-9_]*$").WithMessage(localizer["dataManagement.validation.collectionNameFormat"]);

        RuleFor(x => x.ProjectKey)
            .NotEmpty();
    }
}

public class SchemaFieldValidator : AbstractValidator<SchemaField>
{
    public SchemaFieldValidator(ILocalizationService localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(localizer["dataManagement.validation.fieldNameRequired"])
            .Matches(@"^[a-zA-Z][a-zA-Z0-9]*$").WithMessage(localizer["dataManagement.validation.fieldNameFormat"]);
    }
}
```

---

## Component Patterns

### SchemaBuilder

Dynamic list of `FieldRow` components rendered in a `MudTable` with add/remove. Each row contains: field name (`MudTextField`), field type (`MudSelect`), IsArray (`MudCheckBox`), IsRequired (`MudCheckBox`), Description (`MudTextField`). Validates with FluentValidation on save.

```razor
@* Modules/DataManagement/Components/SchemaBuilder.razor *@
@inject ILocalizationService Localizer

<MudTable Items="_fields" Dense="true" Hover="true" Elevation="0" Bordered="true">
    <HeaderContent>
        <MudTh>@Localizer["dataManagement.field.name"]</MudTh>
        <MudTh>@Localizer["dataManagement.field.type"]</MudTh>
        <MudTh>@Localizer["dataManagement.field.isArray"]</MudTh>
        <MudTh>@Localizer["dataManagement.field.isRequired"]</MudTh>
        <MudTh>@Localizer["dataManagement.field.description"]</MudTh>
        <MudTh>@Localizer["common.actions"]</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>
            <MudTextField @bind-Value="context.Name"
                          Variant="Variant.Text"
                          Placeholder="@Localizer["dataManagement.field.namePlaceholder"]"
                          Immediate="true" />
        </MudTd>
        <MudTd>
            <FieldTypeSelect @bind-Value="context.Type" />
        </MudTd>
        <MudTd>
            <MudCheckBox @bind-Value="context.IsArray" Color="Color.Primary" />
        </MudTd>
        <MudTd>
            <MudCheckBox @bind-Value="context.IsRequired" Color="Color.Primary" />
        </MudTd>
        <MudTd>
            <MudTextField @bind-Value="context.Description"
                          Variant="Variant.Text"
                          Placeholder="@Localizer["dataManagement.field.descriptionPlaceholder"]" />
        </MudTd>
        <MudTd>
            <MudIconButton Icon="@Icons.Material.Filled.Delete"
                           Color="Color.Error"
                           Size="Size.Small"
                           OnClick="@(() => RemoveField(context))" />
        </MudTd>
    </RowTemplate>
    <NoRecordsContent>
        <MudText Typo="Typo.body2" Class="pa-4">
            @Localizer["dataManagement.noFieldsDefined"]
        </MudText>
    </NoRecordsContent>
</MudTable>

<MudButton Color="Color.Primary" Variant="Variant.Text"
           StartIcon="@Icons.Material.Filled.Add"
           OnClick="AddField"
           Class="mt-2">
    @Localizer["dataManagement.addField"]
</MudButton>

@code {
    [Parameter] public List<SchemaField> Fields { get; set; } = new();
    [Parameter] public EventCallback<List<SchemaField>> FieldsChanged { get; set; }
    [Parameter] public bool Disabled { get; set; }

    private List<SchemaField> _fields = new();

    protected override void OnParametersSet() => _fields = Fields;

    private async Task AddField()
    {
        _fields.Add(new SchemaField());
        await FieldsChanged.InvokeAsync(_fields);
    }

    private async Task RemoveField(SchemaField field)
    {
        _fields.Remove(field);
        await FieldsChanged.InvokeAsync(_fields);
    }

    public bool Validate()
    {
        var validator = new SchemaFieldValidator(Localizer);
        foreach (var field in _fields)
        {
            var result = validator.Validate(field);
            if (!result.IsValid) return false;
        }
        return true;
    }
}
```

### FieldTypeSelect

```razor
@* Modules/DataManagement/Components/FieldTypeSelect.razor *@
@inject ILocalizationService Localizer

<MudSelect @bind-Value="Value" Variant="Variant.Text" Dense="true"
           ValueChanged="OnValueChanged">
    @foreach (var ft in Enum.GetValues<FieldType>())
    {
        <MudSelectItem Value="ft">@Localizer[$"dataManagement.fieldType.{ft}"]</MudSelectItem>
    }
</MudSelect>

@code {
    [Parameter] public FieldType Value { get; set; }
    [Parameter] public EventCallback<FieldType> ValueChanged { get; set; }

    private async Task OnValueChanged(FieldType value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }
}
```

### FileUpload

Supports both S3 pre-signed URL flow and DMS direct upload. Uses `MudFileUpload` for drag-drop. Shows `MudProgressLinear` during upload. Tracks progress with `HttpClient` progress reporting.

```razor
@* Modules/DataManagement/Components/FileUpload.razor *@
@inject IDataManagementService DataService
@inject ILocalizationService Localizer
@inject ISnackbar Snackbar

<MudFileUpload T="IBrowserFile" FilesChanged="OnFileSelected"
               Accept="@Accept" MaximumFileCount="1">
    <ActivatorContent>
        <FileDropZone />
    </ActivatorContent>
</MudFileUpload>

@if (_progress is not null)
{
    <UploadProgress FileName="@_progress.FileName"
                    Progress="@_progress.Progress"
                    Status="@_progress.Status"
                    OnRetry="@(() => RetryUpload())" />
}

@code {
    [Parameter] public string? ParentDirectoryId { get; set; }
    [Parameter] public string ProjectKey { get; set; } = string.Empty;
    [Parameter] public bool UseS3 { get; set; }
    [Parameter] public string Accept { get; set; } = "*/*";
    [Parameter] public EventCallback OnUploadComplete { get; set; }

    private UploadProgress? _progress;
    private IBrowserFile? _lastFile;

    private async Task OnFileSelected(IBrowserFile file)
    {
        _lastFile = file;

        if (UseS3)
            await UploadViaS3(file);
        else
            await UploadViaDms(file);
    }

    private async Task UploadViaS3(IBrowserFile file)
    {
        _progress = new(file.Name, 0, "uploading");
        StateHasChanged();

        try
        {
            // Step 1: Get pre-signed URL
            var preSignResponse = await DataService.GetPreSignedUploadUrl(new
            {
                FileName = file.Name,
                ContentType = file.ContentType,
                ProjectKey
            });
            preSignResponse.EnsureSuccessStatusCode();
            var preSign = await preSignResponse.Content
                .ReadFromJsonAsync<UdsResponse<PreSignedUrlData>>();

            // Step 2: PUT file to S3
            _progress = _progress with { Progress = 30 };
            StateHasChanged();

            var stream = file.OpenReadStream(maxAllowedSize: 100 * 1024 * 1024);
            await DataService.UploadToS3(preSign!.Data.Url, stream, file.ContentType);

            // Step 3: Update file info in Blocks
            _progress = _progress with { Progress = 80 };
            StateHasChanged();

            var updateResponse = await DataService.UpdateFileInfo(new
            {
                FileId = preSign.Data.FileId,
                Name = file.Name,
                AccessModifier = "Public",
                ProjectKey
            });
            updateResponse.EnsureSuccessStatusCode();

            _progress = _progress with { Progress = 100, Status = "success" };
            Snackbar.Add(Localizer["common.uploadSuccess"], Severity.Success);
            await OnUploadComplete.InvokeAsync();
        }
        catch
        {
            _progress = _progress with { Status = "error" };
            Snackbar.Add(Localizer["common.uploadError"], Severity.Error);
        }
    }

    private async Task UploadViaDms(IBrowserFile file)
    {
        _progress = new(file.Name, 0, "uploading");
        StateHasChanged();

        try
        {
            var formData = new MultipartFormDataContent();
            var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
            formData.Add(new StreamContent(stream), "File", file.Name);
            formData.Add(new StringContent(ProjectKey), "ProjectKey");
            formData.Add(new StringContent("Public"), "AccessModifier");
            if (ParentDirectoryId is not null)
                formData.Add(new StringContent(ParentDirectoryId), "ParentDirectoryId");

            _progress = _progress with { Progress = 50 };
            StateHasChanged();

            var response = await DataService.UploadToDms(formData);
            response.EnsureSuccessStatusCode();

            _progress = _progress with { Progress = 100, Status = "success" };
            Snackbar.Add(Localizer["common.uploadSuccess"], Severity.Success);
            await OnUploadComplete.InvokeAsync();
        }
        catch
        {
            _progress = _progress with { Status = "error" };
            Snackbar.Add(Localizer["common.uploadError"], Severity.Error);
        }
    }

    private async Task RetryUpload()
    {
        if (_lastFile is not null)
            await OnFileSelected(_lastFile);
    }
}
```

### FileDropZone

```razor
@* Modules/DataManagement/Components/FileDropZone.razor *@
@inject ILocalizationService Localizer

<MudPaper Outlined="true"
          Class="d-flex flex-column align-center justify-center pa-8 cursor-pointer"
          Style="border-style: dashed;">
    <MudIcon Icon="@Icons.Material.Filled.CloudUpload" Size="Size.Large" Color="Color.Primary" />
    <MudText Typo="Typo.body1" Class="mt-2">@Localizer["dataManagement.dropFilesHere"]</MudText>
    <MudText Typo="Typo.caption" Color="Color.Secondary">
        @Localizer["dataManagement.dropFilesHint"]
    </MudText>
</MudPaper>
```

### UploadProgress

```razor
@* Modules/DataManagement/Components/UploadProgress.razor *@
@inject ILocalizationService Localizer

<MudPaper Class="pa-3 mt-2" Outlined="true">
    <div class="d-flex align-center gap-2">
        <MudIcon Icon="@GetStatusIcon()" Color="@GetStatusColor()" />
        <MudText Typo="Typo.body2">@FileName</MudText>
    </div>

    <MudProgressLinear Value="@Progress" Color="@GetStatusColor()"
                       Striped="@(Status == "uploading")"
                       Class="mt-2" />

    @if (Status == "error")
    {
        <MudButton Variant="Variant.Text" Color="Color.Error"
                   StartIcon="@Icons.Material.Filled.Refresh"
                   OnClick="OnRetry" Class="mt-1">
            @Localizer["common.retry"]
        </MudButton>
    }
</MudPaper>

@code {
    [Parameter] public string FileName { get; set; } = string.Empty;
    [Parameter] public int Progress { get; set; }
    [Parameter] public string Status { get; set; } = "uploading";
    [Parameter] public EventCallback OnRetry { get; set; }

    private string GetStatusIcon() => Status switch
    {
        "success" => Icons.Material.Filled.CheckCircle,
        "error" => Icons.Material.Filled.Error,
        _ => Icons.Material.Filled.CloudUpload
    };

    private Color GetStatusColor() => Status switch
    {
        "success" => Color.Success,
        "error" => Color.Error,
        _ => Color.Primary
    };
}
```

### FileBrowser

Split-pane view: left `MudTreeView` for folders, right `MudTable` for files. Loads root on mount (`ParentDirectoryId: null`), loads children on folder click. Uses `MudSkeleton` while loading.

```razor
@* Modules/DataManagement/Components/FileBrowser.razor *@
@inject IDataManagementService DataService
@inject ILocalizationService Localizer

<MudGrid>
    @* Left pane — Folder tree *@
    <MudItem xs="3">
        <MudPaper Class="pa-2" Style="min-height: 400px;" Outlined="true">
            @if (_loadingTree)
            {
                <MudSkeleton SkeletonType="SkeletonType.Text" Width="80%" />
                <MudSkeleton SkeletonType="SkeletonType.Text" Width="60%" Class="mt-2" />
                <MudSkeleton SkeletonType="SkeletonType.Text" Width="70%" Class="mt-2" />
            }
            else
            {
                <FolderTree Items="_rootFolders"
                            SelectedFolderId="@_selectedFolderId"
                            OnFolderSelected="OnFolderSelected" />
            }
        </MudPaper>
    </MudItem>

    @* Right pane — File list *@
    <MudItem xs="9">
        @if (_loadingFiles)
        {
            <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="40px" />
            <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="40px" Class="mt-1" />
            <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="40px" Class="mt-1" />
        }
        else
        {
            <FileList Files="_files"
                      OnFileSelected="OnFileSelected"
                      OnDeleteFile="OnDeleteFile" />
        }
    </MudItem>
</MudGrid>

@code {
    [Parameter] public string ProjectKey { get; set; } = string.Empty;
    [Parameter] public EventCallback<DmsFile> FileSelected { get; set; }
    [Parameter] public EventCallback OnRefresh { get; set; }

    private List<DmsFile> _rootFolders = new();
    private List<DmsFile> _files = new();
    private string? _selectedFolderId;
    private bool _loadingTree = true;
    private bool _loadingFiles = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadRootFolder();
    }

    private async Task LoadRootFolder()
    {
        _loadingTree = true;
        _loadingFiles = true;

        var response = await DataService.GetDmsFiles(new
        {
            ParentDirectoryId = (string?)null,
            ProjectKey,
            Page = 1,
            PageSize = 100
        });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<UdsResponse<List<DmsFile>>>();
            var all = result?.Data ?? new();

            _rootFolders = all.Where(f => f.IsFolder).ToList();
            _files = all.Where(f => !f.IsFolder).ToList();
        }

        _loadingTree = false;
        _loadingFiles = false;
    }

    private async Task OnFolderSelected(string folderId)
    {
        _selectedFolderId = folderId;
        _loadingFiles = true;
        StateHasChanged();

        var response = await DataService.GetDmsFiles(new
        {
            ParentDirectoryId = folderId,
            ProjectKey,
            Page = 1,
            PageSize = 100
        });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<UdsResponse<List<DmsFile>>>();
            _files = result?.Data?.Where(f => !f.IsFolder).ToList() ?? new();
        }

        _loadingFiles = false;
    }

    private async Task OnFileSelected(DmsFile file)
        => await FileSelected.InvokeAsync(file);

    private async Task OnDeleteFile(DmsFile file)
    {
        var response = await DataService.DeleteFile(new
        {
            FileId = file.Id,
            ProjectKey
        });

        if (response.IsSuccessStatusCode)
            _files.Remove(file);
    }
}
```

### FolderTree

```razor
@* Modules/DataManagement/Components/FolderTree.razor *@
@inject ILocalizationService Localizer

<MudTreeView T="DmsFile" Items="@TreeItems" Dense="true"
             SelectedValueChanged="@(item => OnFolderClicked(item))">
    <ItemTemplate>
        <MudTreeViewItem Value="@context"
                         Text="@context.Name"
                         Icon="@Icons.Material.Filled.Folder"
                         IconColor="Color.Warning" />
    </ItemTemplate>
</MudTreeView>

@code {
    [Parameter] public List<DmsFile> Items { get; set; } = new();
    [Parameter] public string? SelectedFolderId { get; set; }
    [Parameter] public EventCallback<string> OnFolderSelected { get; set; }

    private HashSet<DmsFile> TreeItems => Items.ToHashSet();

    private async Task OnFolderClicked(DmsFile folder)
    {
        if (folder is not null)
            await OnFolderSelected.InvokeAsync(folder.Id);
    }
}
```

### FileList

```razor
@* Modules/DataManagement/Components/FileList.razor *@
@inject ILocalizationService Localizer

<MudTable Items="Files" Dense="true" Hover="true" Elevation="0">
    <HeaderContent>
        <MudTh>@Localizer["dataManagement.file.name"]</MudTh>
        <MudTh>@Localizer["dataManagement.file.size"]</MudTh>
        <MudTh>@Localizer["dataManagement.file.type"]</MudTh>
        <MudTh>@Localizer["dataManagement.file.created"]</MudTh>
        <MudTh>@Localizer["common.actions"]</MudTh>
    </HeaderContent>
    <RowTemplate>
        <MudTd>
            <MudLink OnClick="@(() => OnFileSelected.InvokeAsync(context))">
                @context.Name
            </MudLink>
        </MudTd>
        <MudTd>@FormatSize(context.Size)</MudTd>
        <MudTd>@(context.ContentType ?? "-")</MudTd>
        <MudTd>@context.CreatedAt.ToString("yyyy-MM-dd")</MudTd>
        <MudTd>
            <MudIconButton Icon="@Icons.Material.Filled.Delete"
                           Color="Color.Error" Size="Size.Small"
                           OnClick="@(() => OnDeleteFile.InvokeAsync(context))" />
        </MudTd>
    </RowTemplate>
    <NoRecordsContent>
        <MudText Typo="Typo.body2" Class="pa-4">
            @Localizer["dataManagement.noFiles"]
        </MudText>
    </NoRecordsContent>
</MudTable>

@code {
    [Parameter] public List<DmsFile> Files { get; set; } = new();
    [Parameter] public EventCallback<DmsFile> OnFileSelected { get; set; }
    [Parameter] public EventCallback<DmsFile> OnDeleteFile { get; set; }

    private string FormatSize(long? bytes)
    {
        if (bytes is null) return "-";
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            _ => $"{bytes / (1024.0 * 1024.0):F1} MB"
        };
    }
}
```

---

## Page Patterns

### SchemasPage

```razor
@* Modules/DataManagement/Pages/SchemasPage.razor *@
@page "/data-management/schemas"
@attribute [Authorize]
@inject IDataManagementService DataService
@inject ILocalizationService Localizer
@inject NavigationManager Navigation
@inject IDialogService DialogService

<MudText Typo="Typo.h5" Class="mb-4">@Localizer["dataManagement.schemas.title"]</MudText>

<MudButton Color="Color.Primary" Variant="Variant.Filled"
           StartIcon="@Icons.Material.Filled.Add"
           OnClick="OpenNewSchemaDialog" Class="mb-4">
    @Localizer["dataManagement.schemas.newSchema"]
</MudButton>

@if (_loading)
{
    <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="300px" />
}
else
{
    <MudTable Items="_schemas" Hover="true" Dense="true" Elevation="1"
              OnRowClick="@((TableRowClickEventArgs<Schema> args) => GoToDetail(args.Item))">
        <HeaderContent>
            <MudTh>@Localizer["dataManagement.schemas.name"]</MudTh>
            <MudTh>@Localizer["dataManagement.schemas.collection"]</MudTh>
            <MudTh>@Localizer["dataManagement.schemas.type"]</MudTh>
            <MudTh>@Localizer["dataManagement.schemas.created"]</MudTh>
        </HeaderContent>
        <RowTemplate>
            <MudTd>@context.SchemaName</MudTd>
            <MudTd><MudChip T="string" Size="Size.Small">@context.CollectionName</MudChip></MudTd>
            <MudTd>@context.SchemaType</MudTd>
            <MudTd>@context.CreatedAt.ToString("yyyy-MM-dd")</MudTd>
        </RowTemplate>
        <NoRecordsContent>
            <MudText Typo="Typo.body2" Class="pa-4">
                @Localizer["dataManagement.schemas.noSchemas"]
            </MudText>
        </NoRecordsContent>
    </MudTable>
}

@code {
    private List<Schema> _schemas = new();
    private bool _loading = true;

    [CascadingParameter] private AppSettings Settings { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var response = await DataService.GetSchemas(new
        {
            projectKey = Settings.ProjectSlug,
            page = 1,
            pageSize = 50
        });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<UdsResponse<List<Schema>>>();
            _schemas = result?.Data ?? new();
        }

        _loading = false;
    }

    private void GoToDetail(Schema schema)
        => Navigation.NavigateTo($"/data-management/schemas/{schema.Id}");

    private async Task OpenNewSchemaDialog()
    {
        var dialog = await DialogService.ShowAsync<NewSchemaDialog>(
            Localizer["dataManagement.schemas.newSchema"]);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            // Reload schemas after creation
            _loading = true;
            StateHasChanged();
            await OnInitializedAsync();
        }
    }
}
```

### SchemaDetailPage

```razor
@* Modules/DataManagement/Pages/SchemaDetailPage.razor *@
@page "/data-management/schemas/{Id}"
@attribute [Authorize]
@inject IDataManagementService DataService
@inject ILocalizationService Localizer
@inject ISnackbar Snackbar
@inject NavigationManager Navigation

@if (_loading)
{
    <MudSkeleton SkeletonType="SkeletonType.Rectangle" Height="400px" />
}
else if (_schema is null)
{
    <MudAlert Severity="Severity.Warning">@Localizer["common.notFound"]</MudAlert>
}
else
{
    <MudText Typo="Typo.h5" Class="mb-2">@_schema.SchemaName</MudText>
    <MudText Typo="Typo.caption" Color="Color.Secondary" Class="mb-4">
        @_schema.CollectionName
    </MudText>

    <MudTabs Elevation="1" Rounded="true" ApplyEffectsToContainer="true" Class="mt-4">
        @* Tab 1: Fields *@
        <MudTabPanel Text="@Localizer["dataManagement.detail.fields"]"
                     Icon="@Icons.Material.Filled.TableChart">
            <div class="pa-4">
                <SchemaBuilder @bind-Fields="_schema.Fields"
                               Disabled="_reloading" />

                <MudButton Color="Color.Primary" Variant="Variant.Filled"
                           OnClick="SaveFields" Disabled="_saving || _reloading"
                           Class="mt-4">
                    @if (_saving)
                    {
                        <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
                    }
                    @Localizer["common.save"]
                </MudButton>
            </div>
        </MudTabPanel>

        @* Tab 2: Validations *@
        <MudTabPanel Text="@Localizer["dataManagement.detail.validations"]"
                     Icon="@Icons.Material.Filled.Rule">
            <div class="pa-4">
                @if (_schema.Fields is not null)
                {
                    @foreach (var field in _schema.Fields)
                    {
                        <MudExpansionPanels Class="mb-2">
                            <MudExpansionPanel Text="@field.Name">
                                @* Validation rules for this field *@
                                @{
                                    var fieldRules = _validations
                                        .Where(v => v.FieldName == field.Name).ToList();
                                }
                                @foreach (var rule in fieldRules)
                                {
                                    <MudChip T="string" Color="Color.Info" Size="Size.Small"
                                             Class="mr-1">
                                        @rule.Type @(rule.Value is not null ? $": {rule.Value}" : "")
                                    </MudChip>
                                }
                                <MudButton Variant="Variant.Text" Color="Color.Primary"
                                           Size="Size.Small"
                                           StartIcon="@Icons.Material.Filled.Add">
                                    @Localizer["dataManagement.addValidation"]
                                </MudButton>
                            </MudExpansionPanel>
                        </MudExpansionPanels>
                    }
                }
            </div>
        </MudTabPanel>

        @* Tab 3: Access Control *@
        <MudTabPanel Text="@Localizer["dataManagement.detail.accessControl"]"
                     Icon="@Icons.Material.Filled.Security">
            <div class="pa-4">
                <MudSelect @bind-Value="_securityType" Label="@Localizer["dataManagement.securityType"]"
                           Variant="Variant.Outlined">
                    @foreach (var st in Enum.GetValues<SecurityType>())
                    {
                        <MudSelectItem Value="st">@st</MudSelectItem>
                    }
                </MudSelect>

                <MudButton Color="Color.Primary" Variant="Variant.Outlined"
                           OnClick="ChangeSecurity" Class="mt-2">
                    @Localizer["dataManagement.updateSecurity"]
                </MudButton>

                @if (_securityType == SecurityType.RoleBased)
                {
                    <MudDivider Class="my-4" />
                    <MudText Typo="Typo.h6">@Localizer["dataManagement.accessPolicies"]</MudText>

                    <MudTable Items="_policies" Dense="true" Hover="true" Class="mt-2">
                        <HeaderContent>
                            <MudTh>@Localizer["dataManagement.policy.name"]</MudTh>
                            <MudTh>@Localizer["dataManagement.policy.roles"]</MudTh>
                            <MudTh>@Localizer["dataManagement.policy.operations"]</MudTh>
                            <MudTh>@Localizer["common.actions"]</MudTh>
                        </HeaderContent>
                        <RowTemplate>
                            <MudTd>@context.PolicyName</MudTd>
                            <MudTd>
                                @foreach (var role in context.AllowedRoles)
                                {
                                    <MudChip T="string" Size="Size.Small">@role</MudChip>
                                }
                            </MudTd>
                            <MudTd>
                                @foreach (var op in context.Operations)
                                {
                                    <MudChip T="string" Size="Size.Small"
                                             Color="Color.Info">@op</MudChip>
                                }
                            </MudTd>
                            <MudTd>
                                <MudIconButton Icon="@Icons.Material.Filled.Edit"
                                               Size="Size.Small" />
                                <MudIconButton Icon="@Icons.Material.Filled.Delete"
                                               Color="Color.Error" Size="Size.Small" />
                            </MudTd>
                        </RowTemplate>
                    </MudTable>
                }
            </div>
        </MudTabPanel>
    </MudTabs>

    @* Reload indicator *@
    @if (_reloading)
    {
        <MudOverlay Visible="true" DarkBackground="true" Absolute="true">
            <MudProgressCircular Indeterminate="true" Color="Color.Primary" />
            <MudText Class="mt-2" Color="Color.Surface">
                @Localizer["dataManagement.reloadingConfiguration"]
            </MudText>
        </MudOverlay>
    }
}

@code {
    [Parameter] public string Id { get; set; } = string.Empty;
    [CascadingParameter] private AppSettings Settings { get; set; } = default!;

    private Schema? _schema;
    private List<ValidationRule> _validations = new();
    private List<AccessPolicy> _policies = new();
    private SecurityType _securityType = SecurityType.Public;
    private bool _loading = true;
    private bool _saving;
    private bool _reloading;

    protected override async Task OnInitializedAsync()
    {
        var response = await DataService.GetSchemaById(new
        {
            id = Id,
            projectKey = Settings.ProjectSlug
        });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<UdsResponse<Schema>>();
            _schema = result?.Data;
        }

        _loading = false;
    }

    private async Task SaveFields()
    {
        if (_schema?.Fields is null) return;

        _saving = true;
        var response = await DataService.SaveSchemaInfo(new SaveSchemaInfoPayload
        {
            SchemaId = _schema.Id,
            Fields = _schema.Fields,
            ProjectKey = Settings.ProjectSlug
        });

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add(Localizer["common.saved"], Severity.Success);
            await ReloadConfig();
        }
        else
        {
            Snackbar.Add(Localizer["common.error"], Severity.Error);
        }

        _saving = false;
    }

    private async Task ChangeSecurity()
    {
        await DataService.ChangeSecurity(new
        {
            SchemaName = _schema!.SchemaName,
            SecurityType = _securityType.ToString(),
            ProjectKey = Settings.ProjectSlug
        });
        await ReloadConfig();
    }

    private async Task ReloadConfig()
    {
        _reloading = true;
        StateHasChanged();

        await DataService.ReloadConfiguration();

        _reloading = false;
        StateHasChanged();
    }
}
```

### FilesPage

```razor
@* Modules/DataManagement/Pages/FilesPage.razor *@
@page "/data-management/files"
@attribute [Authorize]
@inject IDataManagementService DataService
@inject ILocalizationService Localizer
@inject IDialogService DialogService
@inject ISnackbar Snackbar

<MudToolBar Dense="true" Class="mb-4">
    <MudText Typo="Typo.h5">@Localizer["dataManagement.files.title"]</MudText>
    <MudSpacer />
    <MudButton Color="Color.Primary" Variant="Variant.Filled"
               StartIcon="@Icons.Material.Filled.Upload"
               OnClick="ToggleUpload">
        @Localizer["dataManagement.files.upload"]
    </MudButton>
    <MudButton Color="Color.Default" Variant="Variant.Outlined"
               StartIcon="@Icons.Material.Filled.CreateNewFolder"
               OnClick="CreateNewFolder" Class="ml-2">
        @Localizer["dataManagement.files.newFolder"]
    </MudButton>
    <MudButton Color="Color.Error" Variant="Variant.Text"
               StartIcon="@Icons.Material.Filled.Delete"
               OnClick="DeleteSelected" Class="ml-2"
               Disabled="@(_selectedFile is null)">
        @Localizer["common.delete"]
    </MudButton>
</MudToolBar>

@if (_showUpload)
{
    <MudPaper Class="pa-4 mb-4" Outlined="true">
        <FileUpload ProjectKey="@Settings.ProjectSlug"
                    ParentDirectoryId="@_selectedFolderId"
                    OnUploadComplete="OnUploadComplete" />
    </MudPaper>
}

<FileBrowser ProjectKey="@Settings.ProjectSlug"
             @ref="_browser"
             FileSelected="OnFileSelected" />

@code {
    [CascadingParameter] private AppSettings Settings { get; set; } = default!;

    private FileBrowser? _browser;
    private DmsFile? _selectedFile;
    private string? _selectedFolderId;
    private bool _showUpload;

    private void ToggleUpload() => _showUpload = !_showUpload;

    private void OnFileSelected(DmsFile file) => _selectedFile = file;

    private async Task OnUploadComplete()
    {
        _showUpload = false;
        if (_browser is not null)
            await _browser.OnInitializedAsync();
    }

    private async Task CreateNewFolder()
    {
        var parameters = new DialogParameters
        {
            ["ProjectKey"] = Settings.ProjectSlug,
            ["ParentDirectoryId"] = _selectedFolderId
        };

        var dialog = await DialogService.ShowAsync<NewFolderDialog>(
            Localizer["dataManagement.files.newFolder"], parameters);
        var result = await dialog.Result;

        if (!result.Canceled && _browser is not null)
            await _browser.OnInitializedAsync();
    }

    private async Task DeleteSelected()
    {
        if (_selectedFile is null) return;

        var confirmed = await DialogService.ShowMessageBox(
            Localizer["common.confirm"],
            Localizer["dataManagement.files.deleteConfirm"],
            yesText: Localizer["common.delete"],
            cancelText: Localizer["common.cancel"]);

        if (confirmed == true)
        {
            HttpResponseMessage response;
            if (_selectedFile.IsFolder)
            {
                response = await DataService.DeleteFolder(new
                {
                    folderId = _selectedFile.Id,
                    projectKey = Settings.ProjectSlug
                });
            }
            else
            {
                response = await DataService.DeleteFile(new
                {
                    FileId = _selectedFile.Id,
                    ProjectKey = Settings.ProjectSlug
                });
            }

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add(Localizer["common.deleted"], Severity.Success);
                _selectedFile = null;
                if (_browser is not null)
                    await _browser.OnInitializedAsync();
            }
            else
            {
                Snackbar.Add(Localizer["common.error"], Severity.Error);
            }
        }
    }
}
```

---

## File Upload Flow

### S3 Pre-Signed URL Flow

1. Call `GetPreSignedUploadUrl` with file name, content type, and project key
2. Receive `{ url, fileId }` in the response
3. PUT the raw file binary directly to the S3 `url` (no auth headers -- direct S3 upload)
4. Call `UpdateFileInfo` with the `fileId` to register the file in Blocks

```csharp
// S3 upload sequence
var preSign = await DataService.GetPreSignedUploadUrl(new { FileName, ContentType, ProjectKey });
var data = await preSign.Content.ReadFromJsonAsync<UdsResponse<PreSignedUrlData>>();

await DataService.UploadToS3(data.Data.Url, fileStream, contentType);

await DataService.UpdateFileInfo(new { FileId = data.Data.FileId, Name = fileName, ProjectKey });
```

### DMS Direct Upload Flow

1. Build `MultipartFormDataContent` with the file binary and metadata fields
2. POST to `UploadFile` endpoint

```csharp
// DMS upload sequence
var formData = new MultipartFormDataContent();
formData.Add(new StreamContent(file.OpenReadStream(50 * 1024 * 1024)), "File", file.Name);
formData.Add(new StringContent(projectKey), "ProjectKey");
formData.Add(new StringContent("Public"), "AccessModifier");
if (parentDirectoryId is not null)
    formData.Add(new StringContent(parentDirectoryId), "ParentDirectoryId");

await DataService.UploadToDms(formData);
```

---

## After Schema Changes

Always call `ReloadConfiguration()` after any of these operations:

- `DefineSchema`
- `UpdateSchema`
- `SaveSchemaInfo`
- `UpdateSchemaInfo`
- `SaveSchemaFields`
- `DeleteSchema`

Implementation pattern:

```csharp
private async Task ReloadConfig()
{
    _reloading = true;
    StateHasChanged();

    await DataService.ReloadConfiguration();

    _reloading = false;
    StateHasChanged();
}
```

- Show a loading overlay (`MudOverlay` + `MudProgressCircular`) while reload is in progress
- Block all schema edit controls by binding `Disabled="_reloading"` on buttons and inputs
- Do not allow further schema modifications until `ReloadConfiguration` completes

---

## Route Definitions

```razor
@page "/data-management/schemas"        → SchemasPage.razor
@page "/data-management/schemas/{Id}"   → SchemaDetailPage.razor
@page "/data-management/files"          → FilesPage.razor
```

All data management routes require `@attribute [Authorize]`.

---

## Error Handling

| HTTP Status | Meaning | UI Pattern |
|-------------|---------|-----------|
| 400 | Validation / malformed request | Parse `errors` dictionary from response, show field-level errors using `MudTextField` `ErrorText` binding |
| 401 | Token expired | Handled automatically by `TokenDelegatingHandler` -- triggers token refresh |
| 403 | Insufficient permissions | Show `<MudAlert Severity="Severity.Warning">` with permission error message |
| 404 | Resource not found | Show empty state with "Not found" message |
| 500 | Server error | Show `<MudAlert Severity="Severity.Error">` with a retry button |
| Upload failure | Network or size error | Show error status in `UploadProgress` component with retry button |

Error handling helper pattern:

```csharp
private async Task HandleResponse(HttpResponseMessage response, Func<Task>? onSuccess = null)
{
    if (response.IsSuccessStatusCode)
    {
        Snackbar.Add(Localizer["common.success"], Severity.Success);
        if (onSuccess is not null) await onSuccess();
        return;
    }

    var status = (int)response.StatusCode;
    switch (status)
    {
        case 400:
            var body = await response.Content.ReadFromJsonAsync<UdsResponse<object>>();
            if (body?.Errors is not null)
            {
                foreach (var error in body.Errors)
                    Snackbar.Add($"{error.Key}: {error.Value}", Severity.Warning);
            }
            break;
        case 403:
            Snackbar.Add(Localizer["common.permissionDenied"], Severity.Warning);
            break;
        case 404:
            Snackbar.Add(Localizer["common.notFound"], Severity.Info);
            break;
        default:
            Snackbar.Add(Localizer["common.serverError"], Severity.Error);
            break;
    }
}
```

---

## Localization

Every user-visible string must use `ILocalizationService`. Never hard-code display text.

```razor
@inject ILocalizationService Localizer

<MudText>@Localizer["dataManagement.schemas.title"]</MudText>
```

Key naming convention: `{domain}.{section}.{key}`

Examples:
- `dataManagement.schemas.title`
- `dataManagement.field.name`
- `dataManagement.files.upload`
- `dataManagement.dropFilesHere`
- `common.save`
- `common.delete`
- `common.retry`

Always look up existing keys before creating new ones. Reuse `common.*` keys for shared labels (Save, Cancel, Delete, Retry, etc.).
