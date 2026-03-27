# Data Management — Frontend Guide (Blazor + Tailwind)

This file extends `core/frontend-blazor-tailwind.md` with data-management-specific patterns for the SELISE Blocks data management domain.
Always read `core/frontend-blazor-tailwind.md` first, then apply the overrides and additions here.

**Stack:** .NET 10 Blazor WebAssembly, plain HTML + Tailwind CSS for UI, scoped services for state, HttpClient + DelegatingHandler for HTTP, FluentValidation for validation, ILocalizationService for i18n, GraphQL.Client for GraphQL queries.

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

## Models

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

## Component Patterns

### SchemaBuilder

Dynamic list of field rows rendered in an HTML `<table>` with Tailwind. Each row contains: field name (`<input>`), field type (`<select>`), IsArray (`<input type="checkbox">`), IsRequired (`<input type="checkbox">`), Description (`<input>`). Validates with FluentValidation on save.

```razor
@* Modules/DataManagement/Components/SchemaBuilder.razor *@
@inject ILocalizationService Localizer

<div class="card overflow-hidden">
    <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
            <tr>
                <th class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["dataManagement.field.name"]
                </th>
                <th class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["dataManagement.field.type"]
                </th>
                <th class="px-4 py-3 text-center text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["dataManagement.field.isArray"]
                </th>
                <th class="px-4 py-3 text-center text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["dataManagement.field.isRequired"]
                </th>
                <th class="px-4 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["dataManagement.field.description"]
                </th>
                <th class="relative px-4 py-3"><span class="sr-only">@Localizer["common.actions"]</span></th>
            </tr>
        </thead>
        <tbody class="divide-y divide-gray-200 bg-white">
            @if (!_fields.Any())
            {
                <tr>
                    <td colspan="6" class="px-6 py-8 text-center text-sm text-gray-500">
                        @Localizer["dataManagement.noFieldsDefined"]
                    </td>
                </tr>
            }
            else
            {
                @foreach (var field in _fields)
                {
                    <tr class="hover:bg-gray-50">
                        <td class="whitespace-nowrap px-4 py-2">
                            <input type="text" @bind="field.Name" @bind:event="oninput"
                                   placeholder="@Localizer["dataManagement.field.namePlaceholder"]"
                                   class="input" disabled="@Disabled" />
                        </td>
                        <td class="whitespace-nowrap px-4 py-2">
                            <FieldTypeSelect @bind-Value="field.Type" />
                        </td>
                        <td class="whitespace-nowrap px-4 py-2 text-center">
                            <input type="checkbox" @bind="field.IsArray"
                                   class="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
                                   disabled="@Disabled" />
                        </td>
                        <td class="whitespace-nowrap px-4 py-2 text-center">
                            <input type="checkbox" @bind="field.IsRequired"
                                   class="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
                                   disabled="@Disabled" />
                        </td>
                        <td class="whitespace-nowrap px-4 py-2">
                            <input type="text" @bind="field.Description"
                                   placeholder="@Localizer["dataManagement.field.descriptionPlaceholder"]"
                                   class="input" disabled="@Disabled" />
                        </td>
                        <td class="whitespace-nowrap px-4 py-2 text-right">
                            <button class="text-red-600 hover:text-red-800 disabled:opacity-50"
                                    @onclick="() => RemoveField(field)" disabled="@Disabled">
                                @* Heroicon: outline/trash *@
                                <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                                </svg>
                            </button>
                        </td>
                    </tr>
                }
            }
        </tbody>
    </table>
</div>

<button class="btn-ghost mt-2 inline-flex items-center gap-1" @onclick="AddField" disabled="@Disabled">
    @* Heroicon: outline/plus *@
    <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
    </svg>
    @Localizer["dataManagement.addField"]
</button>

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

<select @bind="Value" @bind:after="OnValueChanged" class="input">
    @foreach (var ft in Enum.GetValues<FieldType>())
    {
        <option value="@ft">@Localizer[$"dataManagement.fieldType.{ft}"]</option>
    }
</select>

@code {
    [Parameter] public FieldType Value { get; set; }
    [Parameter] public EventCallback<FieldType> ValueChanged { get; set; }

    private async Task OnValueChanged()
    {
        await ValueChanged.InvokeAsync(Value);
    }
}
```

### FileUpload

Supports both S3 pre-signed URL flow and DMS direct upload. Uses `<InputFile>` with a drag-drop zone. Shows a progress bar during upload. Tracks progress with status updates.

```razor
@* Modules/DataManagement/Components/FileUpload.razor *@
@inject IDataManagementService DataService
@inject ILocalizationService Localizer
@inject ToastService Toast

<label class="block cursor-pointer">
    <InputFile OnChange="OnFileSelected" accept="@Accept"
               class="sr-only" id="file-upload-input" />
    <FileDropZone />
</label>

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

    private async Task OnFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
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
            Toast.ShowSuccess(Localizer["common.uploadSuccess"]);
            await OnUploadComplete.InvokeAsync();
        }
        catch
        {
            _progress = _progress with { Status = "error" };
            Toast.ShowError(Localizer["common.uploadError"]);
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
            Toast.ShowSuccess(Localizer["common.uploadSuccess"]);
            await OnUploadComplete.InvokeAsync();
        }
        catch
        {
            _progress = _progress with { Status = "error" };
            Toast.ShowError(Localizer["common.uploadError"]);
        }
    }

    private async Task RetryUpload()
    {
        if (_lastFile is not null)
        {
            var file = _lastFile;
            if (UseS3)
                await UploadViaS3(file);
            else
                await UploadViaDms(file);
        }
    }
}
```

### FileDropZone

```razor
@* Modules/DataManagement/Components/FileDropZone.razor *@
@inject ILocalizationService Localizer

<div class="flex flex-col items-center justify-center rounded-lg border-2 border-dashed border-gray-300 px-6 py-10 text-center hover:border-primary/50 hover:bg-primary/5 transition-colors cursor-pointer">
    @* Heroicon: outline/cloud-arrow-up *@
    <svg class="mx-auto h-10 w-10 text-primary" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" d="M12 16.5V9.75m0 0 3 3m-3-3-3 3M6.75 19.5a4.5 4.5 0 0 1-1.41-8.775 5.25 5.25 0 0 1 10.233-2.33 3 3 0 0 1 3.758 3.848A3.752 3.752 0 0 1 18 19.5H6.75Z" />
    </svg>
    <p class="mt-2 text-sm font-medium text-gray-900">@Localizer["dataManagement.dropFilesHere"]</p>
    <p class="mt-1 text-xs text-gray-500">@Localizer["dataManagement.dropFilesHint"]</p>
</div>
```

### UploadProgress

```razor
@* Modules/DataManagement/Components/UploadProgress.razor *@
@inject ILocalizationService Localizer

<div class="mt-2 rounded-lg border border-gray-200 bg-white p-3">
    <div class="flex items-center gap-2">
        @if (Status == "success")
        {
            @* Heroicon: solid/check-circle *@
            <svg class="h-5 w-5 text-green-500" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 1 0 0-16 8 8 0 0 0 0 16Zm3.857-9.809a.75.75 0 0 0-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 1 0-1.06 1.061l2.5 2.5a.75.75 0 0 0 1.137-.089l4-5.5Z" clip-rule="evenodd" />
            </svg>
        }
        else if (Status == "error")
        {
            @* Heroicon: solid/x-circle *@
            <svg class="h-5 w-5 text-red-500" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 1 0 0-16 8 8 0 0 0 0 16ZM8.28 7.22a.75.75 0 0 0-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 1 0 1.06 1.06L10 11.06l1.72 1.72a.75.75 0 1 0 1.06-1.06L11.06 10l1.72-1.72a.75.75 0 0 0-1.06-1.06L10 8.94 8.28 7.22Z" clip-rule="evenodd" />
            </svg>
        }
        else
        {
            @* Heroicon: outline/cloud-arrow-up *@
            <svg class="h-5 w-5 text-primary" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 16.5V9.75m0 0 3 3m-3-3-3 3M6.75 19.5a4.5 4.5 0 0 1-1.41-8.775 5.25 5.25 0 0 1 10.233-2.33 3 3 0 0 1 3.758 3.848A3.752 3.752 0 0 1 18 19.5H6.75Z" />
            </svg>
        }
        <span class="text-sm text-gray-700">@FileName</span>
    </div>

    <div class="mt-2 h-2 w-full overflow-hidden rounded-full bg-gray-200">
        <div class="h-full rounded-full transition-all duration-300 @ProgressBarClass()"
             style="width: @(Progress)%"></div>
    </div>

    @if (Status == "error")
    {
        <button class="btn-ghost btn-sm mt-2 text-red-600 hover:text-red-800 inline-flex items-center gap-1"
                @onclick="OnRetry">
            @* Heroicon: outline/arrow-path *@
            <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0 3.181 3.183a8.25 8.25 0 0 0 13.803-3.7M4.031 9.865a8.25 8.25 0 0 1 13.803-3.7l3.181 3.182M2.985 19.644l3.181-3.183" />
            </svg>
            @Localizer["common.retry"]
        </button>
    }
</div>

@code {
    [Parameter] public string FileName { get; set; } = string.Empty;
    [Parameter] public int Progress { get; set; }
    [Parameter] public string Status { get; set; } = "uploading";
    [Parameter] public EventCallback OnRetry { get; set; }

    private string ProgressBarClass() => Status switch
    {
        "success" => "bg-green-500",
        "error" => "bg-red-500",
        _ => "bg-primary"
    };
}
```

### FileBrowser

Split-pane view: left nested `<ul>` for folders, right HTML `<table>` for files. Loads root on mount (`ParentDirectoryId: null`), loads children on folder click. Uses skeleton loading states.

```razor
@* Modules/DataManagement/Components/FileBrowser.razor *@
@inject IDataManagementService DataService
@inject ILocalizationService Localizer

<div class="flex gap-4" style="min-height: 400px;">
    @* Left pane — Folder tree *@
    <div class="w-1/4 rounded-lg border border-gray-200 bg-white p-2">
        @if (_loadingTree)
        {
            <div class="animate-pulse space-y-3 p-2">
                <div class="h-4 w-4/5 rounded bg-gray-200"></div>
                <div class="h-4 w-3/5 rounded bg-gray-200"></div>
                <div class="h-4 w-7/10 rounded bg-gray-200"></div>
            </div>
        }
        else
        {
            <FolderTree Items="_rootFolders"
                        SelectedFolderId="@_selectedFolderId"
                        OnFolderSelected="OnFolderSelected" />
        }
    </div>

    @* Right pane — File list *@
    <div class="flex-1">
        @if (_loadingFiles)
        {
            <div class="animate-pulse space-y-2">
                <div class="h-10 w-full rounded bg-gray-200"></div>
                <div class="h-10 w-full rounded bg-gray-200"></div>
                <div class="h-10 w-full rounded bg-gray-200"></div>
            </div>
        }
        else
        {
            <FileList Files="_files"
                      OnFileSelected="OnFileSelected"
                      OnDeleteFile="OnDeleteFile" />
        }
    </div>
</div>

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

Recursive folder hierarchy using nested `<ul>` with Tailwind indentation.

```razor
@* Modules/DataManagement/Components/FolderTree.razor *@
@inject ILocalizationService Localizer

<ul class="space-y-1">
    @foreach (var folder in Items)
    {
        var isSelected = folder.Id == SelectedFolderId;
        <li>
            <button class="@FolderClass(isSelected) flex w-full items-center gap-2 rounded-md px-2 py-1.5 text-sm"
                    @onclick="() => OnFolderClicked(folder)">
                @* Heroicon: solid/folder *@
                <svg class="h-4 w-4 shrink-0 text-yellow-500" viewBox="0 0 20 20" fill="currentColor">
                    <path d="M3.75 3A1.75 1.75 0 0 0 2 4.75v3.26a3.235 3.235 0 0 1 1.75-.51h12.5c.644 0 1.245.188 1.75.51V6.75A1.75 1.75 0 0 0 16.25 5h-4.836a.25.25 0 0 1-.177-.073L9.823 3.513A1.75 1.75 0 0 0 8.586 3H3.75ZM3.75 9A1.75 1.75 0 0 0 2 10.75v4.5c0 .966.784 1.75 1.75 1.75h12.5A1.75 1.75 0 0 0 18 15.25v-4.5A1.75 1.75 0 0 0 16.25 9H3.75Z" />
                </svg>
                <span class="truncate">@folder.Name</span>
            </button>
        </li>
    }
</ul>

@if (!Items.Any())
{
    <p class="px-2 py-4 text-sm text-gray-500">@Localizer["dataManagement.noFolders"]</p>
}

@code {
    [Parameter] public List<DmsFile> Items { get; set; } = new();
    [Parameter] public string? SelectedFolderId { get; set; }
    [Parameter] public EventCallback<string> OnFolderSelected { get; set; }

    private async Task OnFolderClicked(DmsFile folder)
    {
        await OnFolderSelected.InvokeAsync(folder.Id);
    }

    private static string FolderClass(bool isSelected) => isSelected
        ? "bg-primary/10 text-primary font-medium"
        : "text-gray-700 hover:bg-gray-100";
}
```

### FileList

```razor
@* Modules/DataManagement/Components/FileList.razor *@
@inject ILocalizationService Localizer

<div class="card overflow-hidden">
    <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
            <tr>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["dataManagement.file.name"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["dataManagement.file.size"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["dataManagement.file.type"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["dataManagement.file.created"]
                </th>
                <th class="relative px-6 py-3"><span class="sr-only">@Localizer["common.actions"]</span></th>
            </tr>
        </thead>
        <tbody class="divide-y divide-gray-200 bg-white">
            @if (!Files.Any())
            {
                <tr>
                    <td colspan="5" class="px-6 py-12 text-center text-sm text-gray-500">
                        @Localizer["dataManagement.noFiles"]
                    </td>
                </tr>
            }
            else
            {
                @foreach (var file in Files)
                {
                    <tr class="hover:bg-gray-50">
                        <td class="whitespace-nowrap px-6 py-4">
                            <button class="text-sm font-medium text-primary hover:text-primary/80 hover:underline"
                                    @onclick="() => OnFileSelected.InvokeAsync(file)">
                                @file.Name
                            </button>
                        </td>
                        <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                            @FormatSize(file.Size)
                        </td>
                        <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                            @(file.ContentType ?? "-")
                        </td>
                        <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                            @file.CreatedAt.ToString("yyyy-MM-dd")
                        </td>
                        <td class="whitespace-nowrap px-6 py-4 text-right">
                            <button class="text-red-600 hover:text-red-800"
                                    @onclick="() => OnDeleteFile.InvokeAsync(file)">
                                @* Heroicon: outline/trash *@
                                <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                                </svg>
                            </button>
                        </td>
                    </tr>
                }
            }
        </tbody>
    </table>
</div>

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

<div class="mb-6 flex items-center justify-between">
    <h1 class="text-2xl font-bold text-gray-900">@Localizer["dataManagement.schemas.title"]</h1>
    <button class="btn-primary inline-flex items-center gap-2" @onclick="OpenNewSchemaModal">
        @* Heroicon: outline/plus *@
        <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
        </svg>
        @Localizer["dataManagement.schemas.newSchema"]
    </button>
</div>

@if (_loading)
{
    <div class="animate-pulse space-y-2">
        <div class="h-12 w-full rounded bg-gray-200"></div>
        <div class="h-12 w-full rounded bg-gray-200"></div>
        <div class="h-12 w-full rounded bg-gray-200"></div>
    </div>
}
else
{
    <div class="card overflow-hidden">
        <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
                <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                        @Localizer["dataManagement.schemas.name"]
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                        @Localizer["dataManagement.schemas.collection"]
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                        @Localizer["dataManagement.schemas.type"]
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                        @Localizer["dataManagement.schemas.created"]
                    </th>
                </tr>
            </thead>
            <tbody class="divide-y divide-gray-200 bg-white">
                @if (!_schemas.Any())
                {
                    <tr>
                        <td colspan="4" class="px-6 py-12 text-center text-sm text-gray-500">
                            @Localizer["dataManagement.schemas.noSchemas"]
                        </td>
                    </tr>
                }
                else
                {
                    @foreach (var schema in _schemas)
                    {
                        <tr class="cursor-pointer hover:bg-gray-50" @onclick="() => GoToDetail(schema)">
                            <td class="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                                @schema.SchemaName
                            </td>
                            <td class="whitespace-nowrap px-6 py-4">
                                <span class="badge-gray">@schema.CollectionName</span>
                            </td>
                            <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                                @schema.SchemaType
                            </td>
                            <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                                @schema.CreatedAt.ToString("yyyy-MM-dd")
                            </td>
                        </tr>
                    }
                }
            </tbody>
        </table>
    </div>
}

@* New Schema Modal *@
<Modal IsOpen="_showNewSchemaModal"
       Title="@Localizer["dataManagement.schemas.newSchema"]"
       OnClose="CloseNewSchemaModal">
    <EditForm Model="_newSchema" OnValidSubmit="CreateSchema" class="space-y-4">
        <DataAnnotationsValidator />
        <div>
            <label class="label">@Localizer["dataManagement.schemas.name"]</label>
            <InputText @bind-Value="_newSchema.SchemaName" class="input" />
            <ValidationMessage For="@(() => _newSchema.SchemaName)" class="text-red-500 text-xs mt-1" />
        </div>
        <div>
            <label class="label">@Localizer["dataManagement.schemas.collection"]</label>
            <InputText @bind-Value="_newSchema.CollectionName" class="input" />
            <ValidationMessage For="@(() => _newSchema.CollectionName)" class="text-red-500 text-xs mt-1" />
        </div>
        <div>
            <label class="label">@Localizer["dataManagement.schemas.type"]</label>
            <InputSelect @bind-Value="_newSchema.SchemaType" class="input">
                @foreach (var st in Enum.GetValues<SchemaType>())
                {
                    <option value="@st">@st</option>
                }
            </InputSelect>
        </div>
        <div>
            <label class="label">@Localizer["dataManagement.schemas.description"]</label>
            <InputTextArea @bind-Value="_newSchema.Description" class="input" rows="3" />
        </div>
        <div class="flex justify-end gap-3 pt-2">
            <button type="button" class="btn-outline" @onclick="CloseNewSchemaModal">
                @Localizer["common.cancel"]
            </button>
            <button type="submit" class="btn-primary" disabled="@_creating">
                @if (_creating)
                {
                    <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
                }
                @Localizer["common.create"]
            </button>
        </div>
    </EditForm>
</Modal>

@code {
    private List<Schema> _schemas = new();
    private bool _loading = true;
    private bool _showNewSchemaModal;
    private bool _creating;
    private DefineSchemaPayload _newSchema = new();

    [CascadingParameter] private AppSettings Settings { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadSchemas();
    }

    private async Task LoadSchemas()
    {
        _loading = true;
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

    private void OpenNewSchemaModal()
    {
        _newSchema = new DefineSchemaPayload { ProjectKey = Settings.ProjectSlug };
        _showNewSchemaModal = true;
    }

    private void CloseNewSchemaModal() => _showNewSchemaModal = false;

    private async Task CreateSchema()
    {
        _creating = true;
        _newSchema.ProjectKey = Settings.ProjectSlug;

        var response = await DataService.DefineSchema(_newSchema);
        await HandleResponse(response, async () =>
        {
            _showNewSchemaModal = false;
            await LoadSchemas();
        });

        _creating = false;
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
@inject ToastService Toast
@inject NavigationManager Navigation

@if (_loading)
{
    <div class="animate-pulse space-y-4">
        <div class="h-8 w-1/3 rounded bg-gray-200"></div>
        <div class="h-4 w-1/4 rounded bg-gray-200"></div>
        <div class="h-64 w-full rounded bg-gray-200"></div>
    </div>
}
else if (_schema is null)
{
    <ErrorAlert Message="@Localizer["common.notFound"]" />
}
else
{
    <div class="mb-6">
        <h1 class="text-2xl font-bold text-gray-900">@_schema.SchemaName</h1>
        <p class="mt-1 text-sm text-gray-500">@_schema.CollectionName</p>
    </div>

    @* Tab navigation *@
    <div class="border-b border-gray-200">
        <nav class="-mb-px flex gap-6">
            <button class="@TabClass(_activeTab == "fields")"
                    @onclick='() => _activeTab = "fields"'>
                @Localizer["dataManagement.detail.fields"]
            </button>
            <button class="@TabClass(_activeTab == "validations")"
                    @onclick='() => _activeTab = "validations"'>
                @Localizer["dataManagement.detail.validations"]
            </button>
            <button class="@TabClass(_activeTab == "access")"
                    @onclick='() => _activeTab = "access"'>
                @Localizer["dataManagement.detail.accessControl"]
            </button>
        </nav>
    </div>

    <div class="mt-6">
        @* Tab 1: Fields *@
        @if (_activeTab == "fields")
        {
            <SchemaBuilder @bind-Fields="_schema.Fields"
                           Disabled="_reloading" />

            <button class="btn-primary mt-4 inline-flex items-center gap-2"
                    @onclick="SaveFields" disabled="@(_saving || _reloading)">
                @if (_saving)
                {
                    <span class="inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
                }
                @Localizer["common.save"]
            </button>
        }

        @* Tab 2: Validations *@
        @if (_activeTab == "validations")
        {
            @if (_schema.Fields is not null)
            {
                <div class="space-y-3">
                    @foreach (var field in _schema.Fields)
                    {
                        <details class="rounded-lg border border-gray-200 bg-white">
                            <summary class="cursor-pointer px-4 py-3 text-sm font-medium text-gray-900 hover:bg-gray-50">
                                @field.Name
                            </summary>
                            <div class="border-t border-gray-200 px-4 py-3">
                                @{
                                    var fieldRules = _validations
                                        .Where(v => v.FieldName == field.Name).ToList();
                                }
                                <div class="flex flex-wrap gap-2">
                                    @foreach (var rule in fieldRules)
                                    {
                                        <span class="badge-info">
                                            @rule.Type @(rule.Value is not null ? $": {rule.Value}" : "")
                                        </span>
                                    }
                                </div>
                                <button class="btn-ghost btn-sm mt-2 inline-flex items-center gap-1 text-primary">
                                    <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                                    </svg>
                                    @Localizer["dataManagement.addValidation"]
                                </button>
                            </div>
                        </details>
                    }
                </div>
            }
        }

        @* Tab 3: Access Control *@
        @if (_activeTab == "access")
        {
            <div class="space-y-4">
                <div>
                    <label class="label">@Localizer["dataManagement.securityType"]</label>
                    <select @bind="_securityType" class="input max-w-xs">
                        @foreach (var st in Enum.GetValues<SecurityType>())
                        {
                            <option value="@st">@st</option>
                        }
                    </select>
                </div>

                <button class="btn-outline" @onclick="ChangeSecurity">
                    @Localizer["dataManagement.updateSecurity"]
                </button>

                @if (_securityType == SecurityType.RoleBased)
                {
                    <hr class="my-4 border-gray-200" />
                    <h2 class="text-lg font-semibold text-gray-900">
                        @Localizer["dataManagement.accessPolicies"]
                    </h2>

                    <div class="card mt-2 overflow-hidden">
                        <table class="min-w-full divide-y divide-gray-200">
                            <thead class="bg-gray-50">
                                <tr>
                                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                                        @Localizer["dataManagement.policy.name"]
                                    </th>
                                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                                        @Localizer["dataManagement.policy.roles"]
                                    </th>
                                    <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                                        @Localizer["dataManagement.policy.operations"]
                                    </th>
                                    <th class="relative px-6 py-3">
                                        <span class="sr-only">@Localizer["common.actions"]</span>
                                    </th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-gray-200 bg-white">
                                @foreach (var policy in _policies)
                                {
                                    <tr class="hover:bg-gray-50">
                                        <td class="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                                            @policy.PolicyName
                                        </td>
                                        <td class="whitespace-nowrap px-6 py-4">
                                            <div class="flex flex-wrap gap-1">
                                                @foreach (var role in policy.AllowedRoles)
                                                {
                                                    <span class="badge-gray">@role</span>
                                                }
                                            </div>
                                        </td>
                                        <td class="whitespace-nowrap px-6 py-4">
                                            <div class="flex flex-wrap gap-1">
                                                @foreach (var op in policy.Operations)
                                                {
                                                    <span class="badge-info">@op</span>
                                                }
                                            </div>
                                        </td>
                                        <td class="whitespace-nowrap px-6 py-4 text-right">
                                            <div class="inline-flex gap-1">
                                                <button class="btn-ghost btn-sm">
                                                    @Localizer["common.edit"]
                                                </button>
                                                <button class="btn-ghost btn-sm text-red-600 hover:text-red-800">
                                                    @Localizer["common.delete"]
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                }
                            </tbody>
                        </table>
                    </div>
                }
            </div>
        }
    </div>

    @* Reload overlay *@
    @if (_reloading)
    {
        <div class="fixed inset-0 z-50 flex flex-col items-center justify-center bg-white/80">
            <div class="h-10 w-10 animate-spin rounded-full border-4 border-gray-200 border-t-primary"></div>
            <p class="mt-3 text-sm text-gray-600">@Localizer["dataManagement.reloadingConfiguration"]</p>
        </div>
    }
}

@code {
    [Parameter] public string Id { get; set; } = string.Empty;
    [CascadingParameter] private AppSettings Settings { get; set; } = default!;

    private Schema? _schema;
    private List<ValidationRule> _validations = new();
    private List<AccessPolicy> _policies = new();
    private SecurityType _securityType = SecurityType.Public;
    private string _activeTab = "fields";
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

        await HandleResponse(response, async () =>
        {
            await ReloadConfig();
        });

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

    private static string TabClass(bool isActive) => isActive
        ? "border-b-2 border-primary px-1 pb-3 text-sm font-medium text-primary"
        : "border-b-2 border-transparent px-1 pb-3 text-sm font-medium text-gray-500 hover:border-gray-300 hover:text-gray-700";
}
```

### FilesPage

```razor
@* Modules/DataManagement/Pages/FilesPage.razor *@
@page "/data-management/files"
@attribute [Authorize]
@inject IDataManagementService DataService
@inject ILocalizationService Localizer
@inject ToastService Toast

<div class="mb-6 flex flex-wrap items-center justify-between gap-3">
    <h1 class="text-2xl font-bold text-gray-900">@Localizer["dataManagement.files.title"]</h1>
    <div class="flex gap-2">
        <button class="btn-primary inline-flex items-center gap-2" @onclick="ToggleUpload">
            @* Heroicon: outline/arrow-up-tray *@
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M3 16.5v2.25A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75V16.5m-13.5-9L12 3m0 0 4.5 4.5M12 3v13.5" />
            </svg>
            @Localizer["dataManagement.files.upload"]
        </button>
        <button class="btn-outline inline-flex items-center gap-2" @onclick="CreateNewFolder">
            @* Heroicon: outline/folder-plus *@
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 10.5v6m3-3H9m4.06-7.19-2.12-2.12a1.5 1.5 0 0 0-1.061-.44H4.5A2.25 2.25 0 0 0 2.25 6v12a2.25 2.25 0 0 0 2.25 2.25h15A2.25 2.25 0 0 0 21.75 18V9a2.25 2.25 0 0 0-2.25-2.25h-5.379a1.5 1.5 0 0 1-1.06-.44Z" />
            </svg>
            @Localizer["dataManagement.files.newFolder"]
        </button>
        <button class="btn-ghost text-red-600 hover:text-red-800 inline-flex items-center gap-2"
                @onclick="DeleteSelected" disabled="@(_selectedFile is null)">
            @* Heroicon: outline/trash *@
            <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
            </svg>
            @Localizer["common.delete"]
        </button>
    </div>
</div>

@if (_showUpload)
{
    <div class="mb-4 rounded-lg border border-gray-200 bg-white p-4">
        <FileUpload ProjectKey="@Settings.ProjectSlug"
                    ParentDirectoryId="@_selectedFolderId"
                    OnUploadComplete="OnUploadComplete" />
    </div>
}

<FileBrowser ProjectKey="@Settings.ProjectSlug"
             @ref="_browser"
             FileSelected="OnFileSelected" />

@* New Folder Modal *@
<Modal IsOpen="_showNewFolderModal"
       Title="@Localizer["dataManagement.files.newFolder"]"
       OnClose="() => _showNewFolderModal = false">
    <div class="space-y-4">
        <div>
            <label class="label">@Localizer["dataManagement.files.folderName"]</label>
            <input type="text" @bind="_newFolderName" class="input"
                   placeholder="@Localizer["dataManagement.files.folderNamePlaceholder"]" />
        </div>
        <div class="flex justify-end gap-3">
            <button class="btn-outline" @onclick="() => _showNewFolderModal = false">
                @Localizer["common.cancel"]
            </button>
            <button class="btn-primary" @onclick="ConfirmCreateFolder" disabled="@_creatingFolder">
                @if (_creatingFolder)
                {
                    <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
                }
                @Localizer["common.create"]
            </button>
        </div>
    </div>
</Modal>

@* Delete Confirmation *@
<ConfirmationModal IsOpen="_showDeleteConfirm"
                   Title="@Localizer["common.confirm"]"
                   Description="@Localizer["dataManagement.files.deleteConfirm"]"
                   Loading="_deleting"
                   OnCancel="() => _showDeleteConfirm = false"
                   OnConfirm="ConfirmDelete" />

@code {
    [CascadingParameter] private AppSettings Settings { get; set; } = default!;

    private FileBrowser? _browser;
    private DmsFile? _selectedFile;
    private string? _selectedFolderId;
    private bool _showUpload;
    private bool _showNewFolderModal;
    private bool _showDeleteConfirm;
    private bool _creatingFolder;
    private bool _deleting;
    private string _newFolderName = string.Empty;

    private void ToggleUpload() => _showUpload = !_showUpload;

    private void OnFileSelected(DmsFile file) => _selectedFile = file;

    private async Task OnUploadComplete()
    {
        _showUpload = false;
        if (_browser is not null)
            await _browser.OnInitializedAsync();
    }

    private void CreateNewFolder()
    {
        _newFolderName = string.Empty;
        _showNewFolderModal = true;
    }

    private async Task ConfirmCreateFolder()
    {
        if (string.IsNullOrWhiteSpace(_newFolderName)) return;

        _creatingFolder = true;
        var response = await DataService.CreateFolder(new
        {
            Name = _newFolderName,
            ParentDirectoryId = _selectedFolderId,
            ProjectKey = Settings.ProjectSlug
        });

        if (response.IsSuccessStatusCode)
        {
            Toast.ShowSuccess(Localizer["common.created"]);
            _showNewFolderModal = false;
            if (_browser is not null)
                await _browser.OnInitializedAsync();
        }
        else
        {
            Toast.ShowError(Localizer["common.error"]);
        }

        _creatingFolder = false;
    }

    private void DeleteSelected()
    {
        if (_selectedFile is null) return;
        _showDeleteConfirm = true;
    }

    private async Task ConfirmDelete()
    {
        if (_selectedFile is null) return;

        _deleting = true;
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
            Toast.ShowSuccess(Localizer["common.deleted"]);
            _selectedFile = null;
            _showDeleteConfirm = false;
            if (_browser is not null)
                await _browser.OnInitializedAsync();
        }
        else
        {
            Toast.ShowError(Localizer["common.error"]);
        }

        _deleting = false;
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

- Show a loading overlay (full-screen spinner with `bg-white/80` backdrop) while reload is in progress
- Block all schema edit controls by binding `disabled="@_reloading"` on buttons and inputs
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

### HTTP Status Code Handling

| HTTP Status | Meaning | UI Pattern |
|-------------|---------|-----------|
| 400 | Validation / malformed request | Parse `errors` dictionary from response, show field-level `<ValidationMessage>` or toast each error |
| 401 | Token expired | Handled automatically by `TokenDelegatingHandler` -- triggers token refresh |
| 403 | Insufficient permissions | Show `<ErrorAlert>` with permission error message |
| 404 | Resource not found | Show inline empty state with "Not found" message |
| 500 | Server error | Show `<ErrorAlert>` with a retry button |
| Upload failure | Network or size error | Show error status in `UploadProgress` component with retry button |

### Error Handling Helper

Every page and component that calls the API should use a shared `HandleResponse` pattern:

```csharp
private async Task HandleResponse(HttpResponseMessage response, Func<Task>? onSuccess = null)
{
    if (response.IsSuccessStatusCode)
    {
        Toast.ShowSuccess(Localizer["common.success"]);
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
                    Toast.ShowWarning($"{error.Key}: {error.Value}");
            }
            else
            {
                Toast.ShowError(Localizer["common.validationError"]);
            }
            break;
        case 403:
            Toast.ShowWarning(Localizer["common.permissionDenied"]);
            break;
        case 404:
            Toast.ShowInfo(Localizer["common.notFound"]);
            break;
        default:
            Toast.ShowError(Localizer["common.serverError"]);
            break;
    }
}
```

### Component Error States

Every component and page must handle three states: **loading**, **error**, and **data**.

**Loading state** -- use skeleton placeholders:

```razor
@if (_loading)
{
    <div class="animate-pulse space-y-2">
        <div class="h-12 w-full rounded bg-gray-200"></div>
        <div class="h-12 w-full rounded bg-gray-200"></div>
    </div>
}
```

**Error state** -- use `<ErrorAlert>` for inline errors:

```razor
@if (_error is not null)
{
    <ErrorAlert Message="@_error" />
}
```

**Empty state** -- show a message inside tables or content areas:

```razor
<tr>
    <td colspan="4" class="px-6 py-12 text-center text-sm text-gray-500">
        @Localizer["dataManagement.schemas.noSchemas"]
    </td>
</tr>
```

### Toast Notifications

Use `ToastService` for all user-facing notifications. Never use browser `alert()` or `console.log()`.

```csharp
@inject ToastService Toast

Toast.ShowSuccess(Localizer["common.saved"]);
Toast.ShowError(Localizer["common.error"]);
Toast.ShowWarning(Localizer["common.permissionDenied"]);
Toast.ShowInfo(Localizer["common.notFound"]);
```

---

## Localization

Every user-visible string must use `ILocalizationService`. Never hard-code display text.

```razor
@inject ILocalizationService Localizer

<h1 class="text-2xl font-bold">@Localizer["dataManagement.schemas.title"]</h1>
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

---

## MudBlazor to Tailwind Component Mapping Reference

| MudBlazor Component | Tailwind Replacement |
|---------------------|---------------------|
| `MudTable` | HTML `<table>` with Tailwind + `<Pagination />` |
| `MudTreeView` | Nested `<ul>` with Tailwind indentation |
| `MudFileUpload` | `<InputFile>` with drag-drop Tailwind styling |
| `MudTextField` | `<input type="text" class="input">` or `<InputText class="input">` |
| `MudSelect` | `<select class="input">` or `<InputSelect class="input">` |
| `MudCheckBox` | `<input type="checkbox" class="h-4 w-4 rounded border-gray-300 text-primary">` |
| `MudChip` | `<span class="badge-*">` (badge-gray, badge-info, badge-success) |
| `MudDialog` | `<Modal>` / `<ConfirmationModal>` |
| `MudSnackbar` / `ISnackbar` | `ToastService` |
| `MudButton` | `<button class="btn-primary">` / `btn-outline` / `btn-ghost` / `btn-danger` |
| `MudIconButton` | `<button>` with inline Heroicon SVG |
| `MudProgressLinear` | `<div>` with `bg-primary` and width percentage |
| `MudProgressCircular` | Spinning `<div>` with `animate-spin rounded-full border-*` |
| `MudSkeleton` | `<div class="animate-pulse"><div class="h-* w-* rounded bg-gray-200">` |
| `MudOverlay` | `<div class="fixed inset-0 z-50 bg-white/80">` |
| `MudAlert` | `<ErrorAlert>` component |
| `MudTabs` / `MudTabPanel` | Custom tab nav with `border-b-2 border-primary` active class |
| `MudExpansionPanel` | `<details>` / `<summary>` with Tailwind styling |
| `MudDivider` | `<hr class="border-gray-200">` |
| `MudToolBar` | `<div class="flex items-center justify-between">` |
| `MudGrid` / `MudItem` | `<div class="flex gap-*">` with width fractions |
| `MudText` | `<h1>` / `<p>` / `<span>` with Tailwind text classes |
| `MudLink` | `<button class="text-primary hover:underline">` or `<a>` |
| `MudSpacer` | `flex-1` or `ml-auto` on adjacent element |
