# App Layout — Blazor WebAssembly + Tailwind CSS

Covers the authenticated app shell — the layout that wraps every protected page — and shared core components used throughout the app.

Build this after `app-scaffold-blazor-tailwind.md` and before generating any feature pages.

---

## When to Run

Trigger phrases:
> "set up the app layout"
> "create the sidebar"
> "build the app shell"
> "add a navigation menu"
> "set up the main layout"

Also generate this automatically when building the first feature that results in a protected page.

---

## File Map

| File | Purpose |
|------|---------|
| `Layout/MainLayout.razor` | Authenticated shell: sidebar + header + content area |
| `Layout/EmptyLayout.razor` | Public pages (login, signup) — no sidebar/header |
| `Components/Core/AppSidebar.razor` | Left navigation with links to feature modules |
| `Components/Core/AppHeader.razor` | Top bar with LanguageSwitcher and ProfileMenu |
| `Components/Core/ProfileMenu.razor` | Avatar dropdown: user info + logout |
| `Components/Core/LanguageSwitcher.razor` | Language select dropdown |
| `Components/Shared/LoadingOverlay.razor` | Full-screen loading spinner |
| `Components/Shared/ErrorAlert.razor` | Inline error message box |
| `Components/Shared/ConfirmationModal.razor` | Reusable delete/confirm dialog |
| `Components/Shared/Modal.razor` | Base modal wrapper |
| `Components/Shared/ProtectedView.razor` | Permission-based conditional rendering |
| `Components/Shared/ToastContainer.razor` | Toast notification display |
| `Components/Shared/Pagination.razor` | Page navigation for tables |

---

## MainLayout (`Layout/MainLayout.razor`)

```razor
@inherits LayoutComponentBase
@inject ILocalizationService Localizer
@inject AuthState AuthState

<div class="flex h-screen overflow-hidden bg-gray-50">
    <AppSidebar />
    <div class="flex flex-1 flex-col overflow-hidden">
        <AppHeader />
        <main class="flex-1 overflow-y-auto p-6">
            @Body
        </main>
    </div>
</div>
```

---

## EmptyLayout (`Layout/EmptyLayout.razor`)

For public pages (login, signup, password reset):

```razor
@inherits LayoutComponentBase

<div class="min-h-screen bg-gray-50">
    @Body
</div>
```

---

## AppSidebar (`Components/Core/AppSidebar.razor`)

Update the `_navItems` list as new feature modules are added.

```razor
@inject ILocalizationService Localizer
@inject NavigationManager Navigation

<aside class="flex w-64 flex-col border-r border-gray-200 bg-white">
    <div class="flex h-14 items-center border-b border-gray-200 px-4">
        <span class="text-lg font-bold text-primary">Blocks App</span>
    </div>
    <nav class="flex-1 space-y-1 px-2 py-4">
        @foreach (var item in _navItems)
        {
            var isActive = Navigation.Uri.Contains(item.Path);
            <a href="@item.Path"
               class="@NavClass(isActive)">
                <svg class="mr-3 h-5 w-5 shrink-0" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="@item.IconPath" />
                </svg>
                @Localizer[item.LabelKey]
            </a>
        }
    </nav>
</aside>

@code {
    private record NavItem(string LabelKey, string Path, string IconPath);

    private readonly List<NavItem> _navItems = new()
    {
        new("nav.users",          "/users",             "M15 19.128a9.38 9.38 0 0 0 2.625.372 9.337 9.337 0 0 0 4.121-.952 4.125 4.125 0 0 0-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 0 1 8.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0 1 11.964-3.07M12 6.375a3.375 3.375 0 1 1-6.75 0 3.375 3.375 0 0 1 6.75 0Zm8.25 2.25a2.625 2.625 0 1 1-5.25 0 2.625 2.625 0 0 1 5.25 0Z"),
        new("nav.dataManagement", "/data-management",   "M20.25 6.375c0 2.278-3.694 4.125-8.25 4.125S3.75 8.653 3.75 6.375m16.5 0c0-2.278-3.694-4.125-8.25-4.125S3.75 4.097 3.75 6.375m16.5 0v11.25c0 2.278-3.694 4.125-8.25 4.125s-8.25-1.847-8.25-4.125V6.375"),
        new("nav.localization",   "/localization/keys", "M10.5 21l5.25-11.25L21 21m-9-3h7.5M3 5.621a48.474 48.474 0 0 1 6-.371m0 0c1.12 0 2.233.038 3.334.114M9 5.25V3m3.334 2.364C11.176 10.658 7.69 15.08 3 17.502m9.334-12.138c.896.061 1.785.147 2.666.257m-4.589 8.495a18.023 18.023 0 0 1-3.827-5.802"),
        new("nav.aiServices",     "/ai",                "M9.813 15.904L9 18.75l-.813-2.846a4.5 4.5 0 0 0-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 0 0 3.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 0 0 3.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 0 0-3.09 3.09ZM18.259 8.715L18 9.75l-.259-1.035a3.375 3.375 0 0 0-2.455-2.456L14.25 6l1.036-.259a3.375 3.375 0 0 0 2.455-2.456L18 2.25l.259 1.035a3.375 3.375 0 0 0 2.455 2.456L21.75 6l-1.036.259a3.375 3.375 0 0 0-2.455 2.456ZM16.894 20.567L16.5 21.75l-.394-1.183a2.25 2.25 0 0 0-1.423-1.423L13.5 18.75l1.183-.394a2.25 2.25 0 0 0 1.423-1.423l.394-1.183.394 1.183a2.25 2.25 0 0 0 1.423 1.423l1.183.394-1.183.394a2.25 2.25 0 0 0-1.423 1.423Z"),
        new("nav.logs",           "/lmt/logs",          "M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m0 12.75h7.5m-7.5 3H12M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z"),
        new("nav.settings",       "/settings",          "M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.325.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 0 1 1.37.49l1.296 2.247a1.125 1.125 0 0 1-.26 1.431l-1.003.827c-.293.241-.438.613-.43.992a7.723 7.723 0 0 1 0 .255c-.008.378.137.75.43.991l1.004.827c.424.35.534.955.26 1.43l-1.298 2.247a1.125 1.125 0 0 1-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.47 6.47 0 0 1-.22.128c-.331.183-.581.495-.644.869l-.213 1.281c-.09.543-.56.94-1.11.94h-2.594c-.55 0-1.019-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 0 1-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 0 1-1.369-.49l-1.297-2.247a1.125 1.125 0 0 1 .26-1.431l1.004-.827c.292-.24.437-.613.43-.991a6.932 6.932 0 0 1 0-.255c.007-.38-.138-.751-.43-.992l-1.004-.827a1.125 1.125 0 0 1-.26-1.43l1.297-2.247a1.125 1.125 0 0 1 1.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.086.22-.128.332-.183.582-.495.644-.869l.214-1.28Z"),
    };

    private static string NavClass(bool isActive) => isActive
        ? "flex items-center rounded-md px-3 py-2 text-sm font-medium bg-primary/10 text-primary"
        : "flex items-center rounded-md px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 hover:text-gray-900";
}
```

> When a new domain is added, append its entry to `_navItems`. Key names follow `nav.{module}` convention.

---

## AppHeader (`Components/Core/AppHeader.razor`)

```razor
@inject ILocalizationService Localizer

<header class="flex h-14 items-center justify-end border-b border-gray-200 bg-white px-6 gap-3">
    <LanguageSwitcher />
    <ProfileMenu />
</header>
```

---

## ProfileMenu (`Components/Core/ProfileMenu.razor`)

```razor
@inject AuthState AuthState
@inject NavigationManager Navigation
@inject ILocalizationService Localizer
@inject ToastService Toast

<div class="relative" @onfocusout="CloseMenu">
    <button @onclick="ToggleMenu"
            class="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-sm font-medium text-white focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2">
        @GetInitials()
    </button>

    @if (_open)
    {
        <div class="absolute right-0 z-50 mt-2 w-56 origin-top-right rounded-md bg-white shadow-lg ring-1 ring-black/5">
            <div class="px-4 py-3 border-b border-gray-100">
                <p class="text-xs text-gray-500">@AuthState.User?.Email</p>
            </div>
            <div class="py-1">
                <a href="/profile" class="flex items-center px-4 py-2 text-sm text-gray-700 hover:bg-gray-100">
                    @Localizer["nav.profile"]
                </a>
            </div>
            <div class="border-t border-gray-100 py-1">
                <button @onclick="HandleLogout"
                        class="flex w-full items-center px-4 py-2 text-sm text-red-600 hover:bg-red-50">
                    @Localizer["auth.logout"]
                </button>
            </div>
        </div>
    }
</div>

@code {
    private bool _open;

    private string GetInitials()
    {
        var first = AuthState.User?.FirstName?[..1] ?? "";
        var last = AuthState.User?.LastName?[..1] ?? "";
        var initials = $"{first}{last}".ToUpper();
        return string.IsNullOrWhiteSpace(initials) ? "?" : initials;
    }

    private void ToggleMenu() => _open = !_open;
    private void CloseMenu() => _open = false;

    private async Task HandleLogout()
    {
        await AuthState.LogoutAsync();
        Navigation.NavigateTo("/login", forceLoad: true);
    }
}
```

---

## LanguageSwitcher (`Components/Core/LanguageSwitcher.razor`)

```razor
@inject ILocalizationService Localizer
@inject HttpClient Http
@inject AppSettings AppSettings

<select value="@Localizer.CurrentLanguage"
        @onchange="OnLanguageChanged"
        class="rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary">
    @foreach (var lang in _languages)
    {
        <option value="@lang.Code">@lang.Name</option>
    }
</select>

@code {
    private List<LanguageItem> _languages = new();

    protected override async Task OnInitializedAsync()
    {
        var result = await Http.GetFromJsonAsync<List<LanguageItem>>(
            $"/uilm/v1/Language/GetProjectLanguages?projectKey={AppSettings.ProjectSlug}");
        _languages = result ?? new();
    }

    private async Task OnLanguageChanged(ChangeEventArgs e)
    {
        var code = e.Value?.ToString();
        if (!string.IsNullOrEmpty(code))
        {
            await Localizer.SetLanguageAsync(code);
        }
    }

    private record LanguageItem(string Code, string Name);
}
```

---

## LoadingOverlay (`Components/Shared/LoadingOverlay.razor`)

```razor
@if (Visible)
{
    <div class="fixed inset-0 z-[9999] flex items-center justify-center bg-white/80">
        <div class="h-10 w-10 animate-spin rounded-full border-4 border-gray-200 border-t-primary"></div>
    </div>
}

@code {
    [Parameter] public bool Visible { get; set; } = true;
}
```

---

## ErrorAlert (`Components/Shared/ErrorAlert.razor`)

```razor
<div class="rounded-md bg-red-50 border border-red-200 p-4 @Class" role="alert">
    <div class="flex">
        <svg class="h-5 w-5 text-red-400 shrink-0" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z" clip-rule="evenodd" />
        </svg>
        <p class="ml-3 text-sm text-red-700">@Message</p>
    </div>
</div>

@code {
    [Parameter, EditorRequired] public string Message { get; set; } = string.Empty;
    [Parameter] public string? Class { get; set; }
}
```

---

## Modal (`Components/Shared/Modal.razor`)

Base modal wrapper — used by `ConfirmationModal` and any custom dialog.

```razor
@if (IsOpen)
{
    <div class="fixed inset-0 z-50 overflow-y-auto" aria-modal="true" role="dialog">
        <div class="flex min-h-full items-center justify-center p-4">
            <div class="fixed inset-0 bg-black/25 transition-opacity" @onclick="OnClose"></div>
            <div class="relative w-full max-w-md rounded-lg bg-white shadow-xl">
                @if (!string.IsNullOrEmpty(Title))
                {
                    <div class="border-b border-gray-200 px-6 py-4">
                        <h3 class="text-lg font-semibold text-gray-900">@Title</h3>
                    </div>
                }
                <div class="px-6 py-4">
                    @ChildContent
                </div>
            </div>
        </div>
    </div>
}

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
}
```

---

## ConfirmationModal (`Components/Shared/ConfirmationModal.razor`)

```razor
@inject ILocalizationService Localizer

<Modal IsOpen="IsOpen" Title="@Title" OnClose="Cancel">
    <p class="text-sm text-gray-600 mb-6">@Description</p>
    <div class="flex justify-end gap-3">
        <button class="btn-outline" @onclick="Cancel" disabled="@Loading">
            @Localizer["common.cancel"]
        </button>
        <button class="@ButtonClass" @onclick="Confirm" disabled="@Loading">
            @if (Loading)
            {
                <span class="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white/30 border-t-white"></span>
            }
            @Localizer["common.confirm"]
        </button>
    </div>
</Modal>

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string Description { get; set; } = string.Empty;
    [Parameter] public bool Loading { get; set; }
    [Parameter] public bool IsDanger { get; set; } = true;
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnConfirm { get; set; }

    private string ButtonClass => IsDanger ? "btn-danger" : "btn-primary";

    private async Task Cancel() => await OnCancel.InvokeAsync();
    private async Task Confirm() => await OnConfirm.InvokeAsync();
}
```

---

## ProtectedView (`Components/Shared/ProtectedView.razor`)

Permission-based conditional rendering:

```razor
@inject AuthState AuthState

@if (HasPermission())
{
    @ChildContent
}

@code {
    [Parameter, EditorRequired] public string Permission { get; set; } = string.Empty;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool HasPermission() =>
        AuthState.User?.Permissions.Contains(Permission) ?? false;
}
```

Usage:

```razor
<ProtectedView Permission="users.delete">
    <button class="btn-danger btn-sm" @onclick="HandleDelete">
        @Localizer["common.delete"]
    </button>
</ProtectedView>
```

---

## ToastContainer (`Components/Shared/ToastContainer.razor`)

```razor
@implements IDisposable
@inject ToastService ToastService

<div class="fixed bottom-4 right-4 z-[9999] flex flex-col gap-2" aria-live="polite">
    @foreach (var toast in _toasts)
    {
        <div class="@ToastClass(toast.Level) flex items-center gap-3 rounded-lg px-4 py-3 text-sm shadow-lg min-w-[300px] max-w-md animate-slide-in">
            <p class="flex-1">@toast.Message</p>
            <button @onclick="() => Dismiss(toast.Id)" class="shrink-0 opacity-70 hover:opacity-100">
                &times;
            </button>
        </div>
    }
</div>

@code {
    private readonly List<ToastMessage> _toasts = new();

    protected override void OnInitialized()
    {
        ToastService.OnShow += HandleShow;
        ToastService.OnDismiss += Dismiss;
    }

    private async void HandleShow(ToastMessage toast)
    {
        _toasts.Add(toast);
        await InvokeAsync(StateHasChanged);

        _ = Task.Delay(toast.DurationMs).ContinueWith(_ =>
        {
            Dismiss(toast.Id);
        });
    }

    private async void Dismiss(Guid id)
    {
        _toasts.RemoveAll(t => t.Id == id);
        await InvokeAsync(StateHasChanged);
    }

    private static string ToastClass(ToastLevel level) => level switch
    {
        ToastLevel.Success => "bg-green-600 text-white",
        ToastLevel.Error   => "bg-red-600 text-white",
        ToastLevel.Warning => "bg-yellow-500 text-white",
        _                  => "bg-gray-800 text-white",
    };

    public void Dispose()
    {
        ToastService.OnShow -= HandleShow;
        ToastService.OnDismiss -= Dismiss;
    }
}
```

Add to `Styles/app.css`:

```css
@layer utilities {
    @keyframes slide-in {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    .animate-slide-in {
        animation: slide-in 0.2s ease-out;
    }
}
```

---

## Pagination (`Components/Shared/Pagination.razor`)

```razor
@inject ILocalizationService Localizer

@if (TotalPages > 1)
{
    <div class="flex items-center justify-between border-t border-gray-200 px-4 py-3 sm:px-6">
        <p class="text-sm text-gray-700">
            @Localizer["common.showing"] <span class="font-medium">@(((CurrentPage - 1) * PageSize) + 1)</span>
            – <span class="font-medium">@Math.Min(CurrentPage * PageSize, TotalCount)</span>
            @Localizer["common.of"] <span class="font-medium">@TotalCount</span>
        </p>
        <nav class="flex gap-1">
            <button class="btn-outline btn-sm" disabled="@(CurrentPage <= 1)"
                    @onclick="() => OnPageChanged.InvokeAsync(CurrentPage - 1)">
                &laquo;
            </button>
            @for (var i = 1; i <= TotalPages; i++)
            {
                var page = i;
                <button class="@(page == CurrentPage ? "btn-primary btn-sm" : "btn-outline btn-sm")"
                        @onclick="() => OnPageChanged.InvokeAsync(page)">
                    @page
                </button>
            }
            <button class="btn-outline btn-sm" disabled="@(CurrentPage >= TotalPages)"
                    @onclick="() => OnPageChanged.InvokeAsync(CurrentPage + 1)">
                &raquo;
            </button>
        </nav>
    </div>
}

@code {
    [Parameter] public int CurrentPage { get; set; } = 1;
    [Parameter] public int PageSize { get; set; } = 10;
    [Parameter] public int TotalCount { get; set; }
    [Parameter] public EventCallback<int> OnPageChanged { get; set; }

    private int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
```

---

## OIDC Callback Page

```razor
@* Modules/Auth/Pages/OidcCallback.razor *@
@page "/oidc"
@layout EmptyLayout
@inject NavigationManager Navigation
@inject AuthState AuthState
@inject AppSettings AppSettings
@inject HttpClient Http
@inject ILocalizationService Localizer

<LoadingOverlay Visible="_loading" />

@if (_error is not null)
{
    <div class="flex min-h-screen items-center justify-center">
        <div class="w-full max-w-sm">
            <ErrorAlert Message="@_error" />
        </div>
    </div>
}

@code {
    private bool _loading = true;
    private string? _error;

    protected override async Task OnInitializedAsync()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var queryParams = QueryHelpers.ParseQuery(uri.Query);

        queryParams.TryGetValue("code", out var codeValues);
        queryParams.TryGetValue("error", out var errorValues);
        var code = codeValues.FirstOrDefault();
        var error = errorValues.FirstOrDefault();

        if (!string.IsNullOrEmpty(error))
        {
            _error = Localizer["auth.oidc.cancelled"];
            _loading = false;
            return;
        }

        if (string.IsNullOrEmpty(code))
        {
            Navigation.NavigateTo("/login");
            return;
        }

        try
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["client_id"] = AppSettings.OidcClientId,
                ["redirect_uri"] = AppSettings.OidcRedirectUri,
            });

            var response = await Http.PostAsync("/idp/v1/Authentication/Token", content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (result is not null)
            {
                await AuthState.LoginAsync(result.AccessToken, result.RefreshToken);
                Navigation.NavigateTo("/dashboard", replace: true);
            }
        }
        catch
        {
            _error = Localizer["auth.oidc.failed"];
        }

        _loading = false;
    }
}
```

---

## Translation Keys Required

Before generating this layout, create these keys with `save-keys`:

```
nav.users
nav.dataManagement
nav.localization
nav.aiServices
nav.logs
nav.settings
nav.profile
auth.logout
auth.oidc.cancelled
auth.oidc.failed
common.cancel
common.confirm
common.delete
common.showing
common.of
```

Use `get-keys-by-names` to check which already exist before calling `save-keys`.

---

## Rules

- `MainLayout` is the single authenticated shell — all protected pages render inside it.
- `EmptyLayout` is for public pages (login, signup, reset password) — no sidebar/header.
- `LanguageSwitcher` and `ProfileMenu` are always present in the header from the first feature.
- When adding a new domain, append its nav item to `_navItems` in `AppSidebar.razor`.
- `ProtectedView` must only be used inside authenticated routes — it depends on `AuthState`.
- The OIDC callback page must use `EmptyLayout`.
- All UI uses plain HTML + Tailwind utility classes. Never import a component library.
- Use the component classes from `Styles/app.css` (`.btn-primary`, `.input`, `.card`, etc.) for consistency.
- Use `ToastService` instead of `alert()` or `console.log()` for user-facing notifications.
