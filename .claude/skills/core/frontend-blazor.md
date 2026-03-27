# Frontend Skill — Blazor WebAssembly

## Stack

| Layer | Technology |
|-------|-----------|
| Framework | .NET 10 Blazor WebAssembly (standalone) |
| Component library | MudBlazor |
| State management | Scoped services + event pattern |
| Auth | Custom `AuthenticationStateProvider` + `Blazored.LocalStorage` |
| HTTP | `HttpClient` + `DelegatingHandler` |
| GraphQL | `GraphQL.Client` |
| Validation | DataAnnotations + FluentValidation |
| i18n | `IStringLocalizer` with remote JSON resources |
| Icons | MudBlazor built-in icons + Material Design Icons |
| Font | Nunito Sans |

---

## Project Structure

```
ProjectName/
├── wwwroot/
│   ├── css/
│   │   └── app.css                  ← global styles + theme overrides
│   ├── appsettings.json             ← client-side config (API base URL, blocks key, etc.)
│   └── index.html                   ← host page
├── Components/
│   ├── Shared/                      ← base reusable components
│   │   ├── LoadingOverlay.razor
│   │   ├── ErrorAlert.razor
│   │   ├── ConfirmationModal.razor
│   │   └── ProtectedView.razor
│   └── Core/                        ← feature-specific composite components
│       ├── AppSidebar.razor
│       ├── ProfileMenu.razor
│       └── LanguageSwitcher.razor
├── Layout/
│   ├── MainLayout.razor             ← authenticated shell
│   └── EmptyLayout.razor            ← public pages (login, signup)
├── Models/                          ← C# record/class types
├── Modules/                         ← feature modules (one folder per domain)
│   └── FeatureName/
│       ├── Components/              ← feature-specific Razor components
│       ├── Pages/                   ← routed @page components
│       └── Services/                ← feature-specific service classes
├── Services/                        ← shared services
│   ├── AuthService.cs
│   ├── AuthStateProvider.cs
│   ├── TokenDelegatingHandler.cs
│   └── LocalizationService.cs
├── State/                           ← state container services
│   └── AuthState.cs
├── Extensions/                      ← service registration helpers
│   └── ServiceCollectionExtensions.cs
├── _Imports.razor                   ← global usings
├── App.razor                        ← root component + router
├── Routes.razor                     ← route definitions
└── Program.cs                       ← DI + service registration
```

---

## Component Layers

### Shared/ — Base Components
Thin wrappers around MudBlazor components or custom utility components. Never contain business logic.

Examples: `LoadingOverlay`, `ErrorAlert`, `ConfirmationModal`, `ProtectedView`

### Core/ — Feature Components
Composite components built from MudBlazor + Shared components. May contain domain logic.

Examples: `AppSidebar`, `ProfileMenu`, `LanguageSwitcher`, `DataTable`, `OtpInput`

**Rule:** Always use MudBlazor components first. Never use raw HTML elements where a MudBlazor component exists.

---

## Theming

MudBlazor themes are configured in `Program.cs` via `MudTheme`:

```csharp
var theme = new MudTheme
{
    PaletteLight = new PaletteLight
    {
        Primary = appSettings.PrimaryColor ?? "#15969B",
        Secondary = appSettings.SecondaryColor ?? "#5194B8",
        AppbarBackground = "#FFFFFF",
        Background = "#FFFFFF",
        Surface = "#FFFFFF",
    },
    PaletteDark = new PaletteDark
    {
        Primary = appSettings.PrimaryColor ?? "#15969B",
        Secondary = appSettings.SecondaryColor ?? "#5194B8",
    },
    Typography = new Typography
    {
        Default = new DefaultTypography
        {
            FontFamily = new[] { "Nunito Sans", "sans-serif" }
        }
    }
};
```

Apply the theme in `App.razor` (not `MainLayout.razor` — this ensures it covers all routes including public pages):

```razor
<MudThemeProvider Theme="_theme" @bind-IsDarkMode="_isDarkMode" />
```

Colors come from `wwwroot/appsettings.json`:

```json
{
  "PrimaryColor": "#15969B",
  "SecondaryColor": "#5194B8"
}
```

Use MudBlazor semantic tokens in all components — never hardcode hex values:

```razor
@* correct *@
<MudButton Color="Color.Primary">Submit</MudButton>

@* wrong *@
<MudButton Style="background-color: #15969B">Submit</MudButton>
```

Supports light / dark / system modes via `MudThemeProvider`.

---

## Forms

All forms use MudBlazor form components with FluentValidation or DataAnnotations:

```csharp
public class LoginModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;
}
```

```razor
<MudForm @ref="_form" Model="_model" Validation="@(_validator.ValidateValue)">
    <MudTextField @bind-Value="_model.Email"
                  Label="@Localizer["auth.login.emailLabel"]"
                  Validation="@(new Func<string, IEnumerable<string>>(val => _validator.ValidateField(_model, nameof(_model.Email))))"
                  For="@(() => _model.Email)" />
    <MudTextField @bind-Value="_model.Password"
                  Label="@Localizer["auth.login.passwordLabel"]"
                  InputType="InputType.Password"
                  Validation="@(new Func<string, IEnumerable<string>>(val => _validator.ValidateField(_model, nameof(_model.Password))))"
                  For="@(() => _model.Password)" />
    <MudButton Color="Color.Primary" OnClick="HandleSubmit">
        @Localizer["common.submit"]
    </MudButton>
</MudForm>

@code {
    private MudForm _form = default!;
    private LoginModel _model = new();
    private LoginModelValidator _validator = new();
}
```

For complex validation, use FluentValidation:

```csharp
public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
```

---

## Feature Module Structure

Each domain feature follows this structure inside `Modules/`:

```
Modules/
└── FeatureName/
    ├── Components/     ← feature-specific Razor components
    ├── Pages/          ← @page routed components
    ├── Services/       ← feature-specific service classes
    └── Models/         ← feature-specific models (if not in root Models/)
```

---

## Localization — Mandatory for All Frontend Code

**Every user-visible string must use the localizer. No hardcoded strings anywhere.**

This applies to labels, placeholders, button text, error messages, tooltips, headings, empty state messages, and snackbar notifications.

### Localization Service

All components use `IStringLocalizer<App>` or a custom `ILocalizationService`:

```csharp
// Services/LocalizationService.cs
public interface ILocalizationService
{
    string this[string key] { get; }
    string this[string key, params object[] arguments] { get; }
    string CurrentLanguage { get; }
    Task SetLanguageAsync(string languageCode);
    Task LoadTranslationsAsync();
    event Action? OnLanguageChanged;
}

public class BlocksLocalizationService : ILocalizationService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private Dictionary<string, string> _translations = new();

    public string this[string key] =>
        _translations.TryGetValue(key, out var value) ? value : key;

    public string this[string key, params object[] arguments] =>
        string.Format(this[key], arguments);

    public string CurrentLanguage { get; private set; } = "en";

    public event Action? OnLanguageChanged;

    public async Task SetLanguageAsync(string languageCode)
    {
        CurrentLanguage = languageCode;
        await _localStorage.SetItemAsStringAsync("language", languageCode);
        await LoadTranslationsAsync();
        OnLanguageChanged?.Invoke();
    }

    public async Task LoadTranslationsAsync()
    {
        var projectKey = _configuration["ProjectSlug"];
        var response = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(
            $"/uilm/v1/UilmFile/Get?projectKey={projectKey}&languageCode={CurrentLanguage}");
        _translations = response ?? new();
    }
}
```

### Usage in Components

```razor
@inject ILocalizationService Localizer

@* correct *@
<MudButton Color="Color.Primary">@Localizer["common.submit"]</MudButton>
<MudText Typo="Typo.h6">@Localizer["auth.login.title"]</MudText>

@* wrong — hardcoded string *@
<MudButton Color="Color.Primary">Submit</MudButton>
```

### Key Naming Convention

```
{module}.{context}.{element}

auth.login.title
auth.login.emailLabel
auth.login.passwordLabel
common.submit
common.cancel
users.table.emptyState
```

### Key Lookup Before Creation — Required Workflow

Before writing any component, Claude must:

1. **List all user-visible strings** in the planned component
2. **Call `get-keys-by-names`** with the candidate key names to check which already exist
3. **Reuse existing keys** — do not create duplicates
4. **Call `save-keys`** (batch) to create only the missing keys
5. **Then generate the component** using the confirmed key names

### Validation Error Messages

Validation messages must also use the localizer:

```csharp
// wrong
RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");

// correct
RuleFor(x => x.Email).NotEmpty().WithMessage(localizer["validation.email.required"]);
```

---

## Language Switcher — Required in Every App

The language switcher must be added to the app header from the very first feature.

```razor
@* Components/Core/LanguageSwitcher.razor *@
@inject ILocalizationService Localizer
@inject HttpClient Http
@inject AppSettings AppSettings

<MudSelect T="string" Value="Localizer.CurrentLanguage" ValueChanged="OnLanguageChanged"
           Dense="true" Margin="Margin.Dense" Class="mr-2">
    @foreach (var lang in _languages)
    {
        <MudSelectItem Value="@lang.Code">
            <MudIcon Icon="@Icons.Material.Filled.Language" Class="mr-1" Size="Size.Small" />
            @lang.Name
        </MudSelectItem>
    }
</MudSelect>

@code {
    private List<LanguageItem> _languages = new();

    protected override async Task OnInitializedAsync()
    {
        var result = await Http.GetFromJsonAsync<List<LanguageItem>>(
            $"/uilm/v1/Language/GetProjectLanguages?projectKey={AppSettings.ProjectSlug}");
        _languages = result ?? new();
    }

    private async Task OnLanguageChanged(string languageCode)
    {
        await Localizer.SetLanguageAsync(languageCode);
        StateHasChanged();
    }

    private record LanguageItem(string Code, string Name);
}
```

---

## Rules

* Use C# for all components — no JavaScript interop unless absolutely required
* Use MudBlazor components for all UI — never raw HTML where MudBlazor has an equivalent
* Use FluentValidation or DataAnnotations for all form validation
* Use MudBlazor theme tokens — never hardcode colours
* Handle loading state with `<MudSkeleton />` or `<LoadingOverlay />`
* Handle error state with `<ErrorAlert />` or `<MudAlert Severity="Severity.Error">`
* All pages must handle loading, error, and empty states
* **Every user-visible string must use `Localizer["key.name"]` — no hardcoded strings, ever**
* **Look up existing keys with `get-keys-by-names` before creating new ones**
* **Language switcher must be in the app layout from the first feature**
