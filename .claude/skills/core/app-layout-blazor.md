# App Layout — Blazor WebAssembly

Covers the authenticated app shell — the layout that wraps every protected page — and shared core components used throughout the app.

Build this after `app-scaffold-blazor.md` and before generating any feature pages.

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
| `Components/Shared/ProtectedView.razor` | Permission-based conditional rendering |

---

## MainLayout (`Layout/MainLayout.razor`)

```razor
@inherits LayoutComponentBase
@inject ILocalizationService Localizer
@inject AuthState AuthState

<MudLayout>
    <AppSidebar />
    <MudMainContent>
        <AppHeader />
        <MudContainer MaxWidth="MaxWidth.False" Class="pa-6">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>
```

---

## EmptyLayout (`Layout/EmptyLayout.razor`)

For public pages (login, signup, password reset):

```razor
@inherits LayoutComponentBase

<MudLayout>
    <MudMainContent>
        @Body
    </MudMainContent>
</MudLayout>
```

---

## AppSidebar (`Components/Core/AppSidebar.razor`)

Update the `_navItems` list as new feature modules are added.

```razor
@inject ILocalizationService Localizer
@inject NavigationManager Navigation

<MudDrawer Open="true" Variant="DrawerVariant.Persistent" Elevation="1" Class="mud-height-full">
    <MudDrawerHeader Class="d-flex align-center">
        <MudText Typo="Typo.h6" Color="Color.Primary">Blocks App</MudText>
    </MudDrawerHeader>
    <MudNavMenu>
        @foreach (var item in _navItems)
        {
            <MudNavLink Href="@item.Path" Icon="@item.Icon"
                        Match="NavLinkMatch.Prefix">
                @Localizer[item.LabelKey]
            </MudNavLink>
        }
    </MudNavMenu>
</MudDrawer>

@code {
    private record NavItem(string LabelKey, string Path, string Icon);

    private readonly List<NavItem> _navItems = new()
    {
        new("nav.users",          "/users",               Icons.Material.Filled.People),
        new("nav.dataManagement", "/data-management",     Icons.Material.Filled.Storage),
        new("nav.localization",   "/localization/keys",   Icons.Material.Filled.Language),
        new("nav.aiServices",     "/ai",                  Icons.Material.Filled.SmartToy),
        new("nav.logs",           "/lmt/logs",            Icons.Material.Filled.Description),
        new("nav.settings",       "/settings",            Icons.Material.Filled.Settings),
    };
}
```

> When a new domain is added, append its entry to `_navItems`. Key names follow `nav.{module}` convention.

---

## AppHeader (`Components/Core/AppHeader.razor`)

```razor
@inject ILocalizationService Localizer

<MudAppBar Elevation="0" Dense="true" Class="border-b">
    <MudSpacer />
    <LanguageSwitcher />
    <ProfileMenu />
</MudAppBar>
```

---

## ProfileMenu (`Components/Core/ProfileMenu.razor`)

```razor
@inject AuthState AuthState
@inject NavigationManager Navigation
@inject ILocalizationService Localizer
@inject ISnackbar Snackbar

<MudMenu AnchorOrigin="Origin.BottomRight" TransformOrigin="Origin.TopRight">
    <ActivatorContent>
        <MudAvatar Color="Color.Primary" Size="Size.Small" Class="cursor-pointer">
            @GetInitials()
        </MudAvatar>
    </ActivatorContent>
    <ChildContent>
        <MudMenuItem Disabled="true">
            <MudText Typo="Typo.caption" Color="Color.Default">@AuthState.User?.Email</MudText>
        </MudMenuItem>
        <MudDivider />
        <MudMenuItem Icon="@Icons.Material.Filled.Person"
                     OnClick="@(() => Navigation.NavigateTo("/profile"))">
            @Localizer["nav.profile"]
        </MudMenuItem>
        <MudDivider />
        <MudMenuItem Icon="@Icons.Material.Filled.Logout"
                     OnClick="HandleLogout"
                     Class="mud-error-text">
            @Localizer["auth.logout"]
        </MudMenuItem>
    </ChildContent>
</MudMenu>

@code {
    private string GetInitials()
    {
        var first = AuthState.User?.FirstName?[..1] ?? "";
        var last = AuthState.User?.LastName?[..1] ?? "";
        var initials = $"{first}{last}".ToUpper();
        return string.IsNullOrWhiteSpace(initials) ? "?" : initials;
    }

    private async Task HandleLogout()
    {
        await AuthState.LogoutAsync();
        Navigation.NavigateTo("/login", forceLoad: true);
    }
}
```

---

## LoadingOverlay (`Components/Shared/LoadingOverlay.razor`)

```razor
<MudOverlay Visible="Visible" DarkBackground="false" ZIndex="9999">
    <MudProgressCircular Color="Color.Primary" Indeterminate="true" Size="Size.Large" />
</MudOverlay>

@code {
    [Parameter] public bool Visible { get; set; } = true;
}
```

---

## ErrorAlert (`Components/Shared/ErrorAlert.razor`)

```razor
<MudAlert Severity="Severity.Error" Variant="Variant.Filled" Class="@Class">
    @Message
</MudAlert>

@code {
    [Parameter, EditorRequired] public string Message { get; set; } = string.Empty;
    [Parameter] public string? Class { get; set; }
}
```

---

## ConfirmationModal (`Components/Shared/ConfirmationModal.razor`)

```razor
@inject ILocalizationService Localizer

<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">@Title</MudText>
    </TitleContent>
    <DialogContent>
        <MudText>@Description</MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel" Disabled="Loading">
            @Localizer["common.cancel"]
        </MudButton>
        <MudButton Color="@ButtonColor" Variant="Variant.Filled"
                   OnClick="Confirm" Disabled="Loading">
            @if (Loading)
            {
                <MudProgressCircular Size="Size.Small" Indeterminate="true" Class="mr-2" />
            }
            @Localizer["common.confirm"]
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string Description { get; set; } = string.Empty;
    [Parameter] public bool Loading { get; set; }
    [Parameter] public Color ButtonColor { get; set; } = Color.Error;

    private void Cancel() => MudDialog.Cancel();
    private void Confirm() => MudDialog.Close(DialogResult.Ok(true));
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
    <MudButton Color="Color.Error" OnClick="HandleDelete">
        @Localizer["common.delete"]
    </MudButton>
</ProtectedView>
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
    <MudContainer MaxWidth="MaxWidth.Small" Class="d-flex align-center justify-center" Style="min-height: 100vh;">
        <ErrorAlert Message="@_error" />
    </MudContainer>
}

@code {
    private bool _loading = true;
    private string? _error;

    protected override async Task OnInitializedAsync()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

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
