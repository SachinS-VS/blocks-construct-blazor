# Frontend Skill — Blazor WebAssembly + Tailwind CSS

## Stack

| Layer | Technology |
|-------|-----------:|
| Framework | .NET 10 Blazor WebAssembly (standalone) |
| Styling | Tailwind CSS + `@tailwindcss/forms` |
| Component library | None — plain HTML + Tailwind utility classes |
| State management | Scoped services + event pattern |
| Auth | Custom `AuthenticationStateProvider` + `Blazored.LocalStorage` |
| HTTP | `HttpClient` + `DelegatingHandler` |
| GraphQL | `GraphQL.Client` |
| Validation | `EditForm` + DataAnnotations + FluentValidation |
| i18n | `ILocalizationService` with remote JSON resources |
| Icons | Heroicons (inline SVG) |
| Font | Nunito Sans |
| Notifications | Custom `ToastService` |

---

## Project Structure

```
ProjectName/
├── wwwroot/
│   ├── css/
│   │   └── app.css                  ← compiled Tailwind output
│   ├── js/
│   │   └── theme.js                 ← runtime theme color injection
│   ├── appsettings.json             ← client-side config
│   └── index.html                   ← host page
├── Styles/
│   └── app.css                      ← Tailwind source (base/components/utilities)
├── Components/
│   ├── Shared/                      ← base reusable components
│   │   ├── LoadingOverlay.razor
│   │   ├── ErrorAlert.razor
│   │   ├── Modal.razor
│   │   ├── ConfirmationModal.razor
│   │   ├── ProtectedView.razor
│   │   ├── ToastContainer.razor
│   │   ├── Pagination.razor
│   │   └── RedirectToLogin.razor
│   └── Core/                        ← app-wide composite components
│       ├── AppSidebar.razor
│       ├── AppHeader.razor
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
│   ├── LocalizationService.cs
│   └── ToastService.cs
├── State/                           ← state container services
│   └── AuthState.cs
├── _Imports.razor                   ← global usings
├── App.razor                        ← root component + router
├── Program.cs                       ← DI + service registration
├── Styles/app.css                   ← Tailwind source
├── tailwind.config.js               ← Tailwind config with theme
└── package.json                     ← Tailwind build scripts
```

---

## Component Layers

### Shared/ — Base Components
Thin utility components with no business logic. Built with plain HTML + Tailwind.

Examples: `LoadingOverlay`, `ErrorAlert`, `Modal`, `ConfirmationModal`, `ProtectedView`, `ToastContainer`, `Pagination`

### Core/ — App Components
Composite components that make up the app shell. May reference `AuthState` or `ILocalizationService`.

Examples: `AppSidebar`, `AppHeader`, `ProfileMenu`, `LanguageSwitcher`

**Rule:** Always use plain HTML elements with Tailwind utility classes. Use the component classes from `Styles/app.css` (`.btn-primary`, `.input`, `.card`, `.badge-*`) for consistent styling.

---

## Theming

Colors are defined as CSS custom properties in `Styles/app.css` and consumed via `tailwind.config.js`:

```css
:root {
    --color-primary: 21 153 155;       /* RGB values for Tailwind alpha support */
    --color-secondary: 81 148 184;
}
```

```js
// tailwind.config.js
colors: {
    primary: "rgb(var(--color-primary) / <alpha-value>)",
    secondary: "rgb(var(--color-secondary) / <alpha-value>)",
}
```

Runtime override from `appsettings.json` via JS interop in `App.razor`.

Use Tailwind color classes everywhere — never hardcode hex values:

```razor
@* correct *@
<button class="btn-primary">Submit</button>
<span class="text-primary">Active</span>
<div class="bg-primary/10 border border-primary/20">Highlight</div>

@* wrong *@
<button style="background-color: #15969B">Submit</button>
```

---

## Forms

All forms use Blazor's built-in `EditForm` with `DataAnnotationsValidator` or FluentValidation:

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
<EditForm Model="_model" OnValidSubmit="HandleSubmit" class="space-y-4">
    <DataAnnotationsValidator />

    <div>
        <label class="label">@Localizer["auth.login.emailLabel"]</label>
        <InputText @bind-Value="_model.Email" class="input" />
        <ValidationMessage For="@(() => _model.Email)" class="text-red-500 text-xs mt-1" />
    </div>

    <div>
        <label class="label">@Localizer["auth.login.passwordLabel"]</label>
        <InputText @bind-Value="_model.Password" type="password" class="input" />
        <ValidationMessage For="@(() => _model.Password)" class="text-red-500 text-xs mt-1" />
    </div>

    <button type="submit" class="btn-primary w-full">
        @Localizer["common.submit"]
    </button>
</EditForm>

@code {
    private LoginModel _model = new();

    private async Task HandleSubmit() { /* ... */ }
}
```

For complex validation, use FluentValidation with a custom validator component:

```csharp
public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator(ILocalizationService localizer)
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage(localizer["validation.email.required"]).EmailAddress();
        RuleFor(x => x.Password).NotEmpty().WithMessage(localizer["validation.password.required"]).MinimumLength(8);
    }
}
```

---

## Tables

Use plain HTML `<table>` with Tailwind. Combine with `Pagination` component for paged data.

```razor
<div class="card overflow-hidden">
    <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
            <tr>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["users.table.name"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["users.table.email"]
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                    @Localizer["users.table.status"]
                </th>
                <th class="relative px-6 py-3"><span class="sr-only">Actions</span></th>
            </tr>
        </thead>
        <tbody class="divide-y divide-gray-200 bg-white">
            @if (_loading)
            {
                <tr><td colspan="4" class="px-6 py-12 text-center text-sm text-gray-500">
                    <span class="inline-block h-5 w-5 animate-spin rounded-full border-2 border-gray-300 border-t-primary"></span>
                </td></tr>
            }
            else if (!_items.Any())
            {
                <tr><td colspan="4" class="px-6 py-12 text-center text-sm text-gray-500">
                    @Localizer["common.emptyState"]
                </td></tr>
            }
            else
            {
                @foreach (var item in _items)
                {
                    <tr class="hover:bg-gray-50">
                        <td class="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">@item.Name</td>
                        <td class="whitespace-nowrap px-6 py-4 text-sm text-gray-500">@item.Email</td>
                        <td class="whitespace-nowrap px-6 py-4">
                            <span class="@(item.IsActive ? "badge-success" : "badge-gray")">
                                @(item.IsActive ? Localizer["common.active"] : Localizer["common.inactive"])
                            </span>
                        </td>
                        <td class="whitespace-nowrap px-6 py-4 text-right text-sm">
                            <button class="btn-ghost btn-sm" @onclick="() => Edit(item)">
                                @Localizer["common.edit"]
                            </button>
                        </td>
                    </tr>
                }
            }
        </tbody>
    </table>
    <Pagination CurrentPage="_page" PageSize="_pageSize" TotalCount="_total"
                OnPageChanged="LoadPage" />
</div>
```

Every table must handle three states: **loading**, **empty**, and **data**.

---

## Select / Dropdown

Use native `<select>` with Tailwind `@tailwindcss/forms` styling:

```razor
<div>
    <label class="label">@Localizer["users.form.roleLabel"]</label>
    <select @bind="_selectedRole" class="input">
        <option value="">@Localizer["common.selectPlaceholder"]</option>
        @foreach (var role in _roles)
        {
            <option value="@role.Id">@role.Name</option>
        }
    </select>
</div>
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

This applies to labels, placeholders, button text, error messages, tooltips, headings, empty state messages, and toast notifications.

### Localization Service

All components use `ILocalizationService`:

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
    private readonly AppSettings _appSettings;
    private Dictionary<string, string> _translations = new();

    public string this[string key] =>
        _translations.TryGetValue(key, out var value) ? value : key;

    public string this[string key, params object[] arguments] =>
        string.Format(this[key], arguments);

    public string CurrentLanguage { get; private set; } = "en";

    public event Action? OnLanguageChanged;

    public BlocksLocalizationService(HttpClient httpClient, ILocalStorageService localStorage, AppSettings appSettings)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _appSettings = appSettings;
    }

    public async Task SetLanguageAsync(string languageCode)
    {
        CurrentLanguage = languageCode;
        await _localStorage.SetItemAsStringAsync("language", languageCode);
        await LoadTranslationsAsync();
        OnLanguageChanged?.Invoke();
    }

    public async Task LoadTranslationsAsync()
    {
        var stored = await _localStorage.GetItemAsStringAsync("language");
        if (!string.IsNullOrEmpty(stored)) CurrentLanguage = stored;

        var response = await _httpClient.GetFromJsonAsync<Dictionary<string, string>>(
            $"/uilm/v1/UilmFile/Get?projectKey={_appSettings.ProjectSlug}&languageCode={CurrentLanguage}");
        _translations = response ?? new();
    }
}
```

### Usage in Components

```razor
@inject ILocalizationService Localizer

@* correct *@
<button class="btn-primary">@Localizer["common.submit"]</button>
<h1 class="text-2xl font-bold">@Localizer["auth.login.title"]</h1>

@* wrong — hardcoded string *@
<button class="btn-primary">Submit</button>
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

### Language Change Subscription

Components that display localized text should subscribe to language changes to re-render when the language is switched:

```razor
@implements IDisposable
@inject ILocalizationService Localizer

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

The language switcher must be added to the app header from the very first feature. See `core/app-layout-blazor-tailwind.md` for the component.

---

## Icons

Use Heroicons (inline SVG) for all icons. Do not add an icon library — paste the SVG directly:

```razor
@* Heroicon: outline/users *@
<svg class="h-5 w-5 text-gray-500" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
    <path stroke-linecap="round" stroke-linejoin="round" d="M15 19.128a9.38 9.38 0 0 0 2.625.372..." />
</svg>
```

For repeated icons, extract a `{IconName}Icon.razor` component if used in 3+ places.

---

## Rules

* Use plain HTML + Tailwind for all UI — never add a component library
* Use `EditForm` + `DataAnnotationsValidator` or FluentValidation for all form validation
* Use CSS custom properties via Tailwind config for theming — never hardcode colours
* Handle loading state with `<LoadingOverlay />` or inline spinners
* Handle error state with `<ErrorAlert />` or inline error text
* All pages must handle loading, error, and empty states
* **Every user-visible string must use `Localizer["key.name"]` — no hardcoded strings, ever**
* **Look up existing keys with `get-keys-by-names` before creating new ones**
* **Language switcher must be in the app layout from the first feature**
* Use `ToastService` for success/error/info notifications — not browser alerts
* Use Heroicons (inline SVG) for all icons
* Run `npm run css:watch` during development
