# App Scaffold — Blazor WebAssembly + Tailwind CSS

Covers everything needed to initialise a new .NET 10 Blazor WASM project using Tailwind CSS for styling — no component library.

Run this **once** when starting a new project — before building any feature. If a project already has `Program.cs` and `App.razor`, skip to the specific section that is missing.

---

## When to Run

Trigger phrases:
> "set up a new project"
> "initialise the app"
> "create the app scaffold"
> "start a new blocks project"
> "what do I need to install"

---

## Step 1 — Create Project and Install Packages

```bash
dotnet new blazorwasm -n MyApp --framework net10.0
cd MyApp

# Local Storage (for auth token persistence)
dotnet add package Blazored.LocalStorage

# Validation
dotnet add package FluentValidation

# Query string parsing (used by OIDC callback)
dotnet add package Microsoft.AspNetCore.WebUtilities

# GraphQL (for data-management schema queries)
dotnet add package GraphQL.Client
dotnet add package GraphQL.Client.Serializer.SystemTextJson
```

### Tailwind CSS Setup

Tailwind requires Node.js for the build step:

```bash
npm init -y
npm install -D tailwindcss @tailwindcss/forms
npx tailwindcss init
```

Create `Styles/app.css`:

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@import url('https://fonts.googleapis.com/css2?family=Nunito+Sans:wght@400;500;600;700&display=swap');

@layer base {
    :root {
        --color-primary: 21 153 155;       /* #15969B */
        --color-secondary: 81 148 184;     /* #5194B8 */
    }

    html, body {
        font-family: 'Nunito Sans', sans-serif;
    }

    #app {
        min-height: 100vh;
    }
}

@layer components {
    .btn {
        @apply inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:pointer-events-none;
    }
    .btn-primary {
        @apply btn bg-primary text-white hover:bg-primary/90 focus:ring-primary;
    }
    .btn-secondary {
        @apply btn bg-secondary text-white hover:bg-secondary/90 focus:ring-secondary;
    }
    .btn-outline {
        @apply btn border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 focus:ring-primary;
    }
    .btn-danger {
        @apply btn bg-red-600 text-white hover:bg-red-700 focus:ring-red-500;
    }
    .btn-ghost {
        @apply btn bg-transparent text-gray-700 hover:bg-gray-100;
    }
    .btn-sm {
        @apply px-3 py-1.5 text-xs;
    }
    .btn-lg {
        @apply px-6 py-3 text-base;
    }
    .input {
        @apply block w-full rounded-md border border-gray-300 px-3 py-2 text-sm shadow-sm placeholder:text-gray-400 focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary disabled:bg-gray-50 disabled:text-gray-500;
    }
    .input-error {
        @apply border-red-500 focus:border-red-500 focus:ring-red-500;
    }
    .label {
        @apply block text-sm font-medium text-gray-700 mb-1;
    }
    .card {
        @apply bg-white rounded-lg border border-gray-200 shadow-sm;
    }
    .badge {
        @apply inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium;
    }
    .badge-primary {
        @apply badge bg-primary/10 text-primary;
    }
    .badge-success {
        @apply badge bg-green-100 text-green-800;
    }
    .badge-warning {
        @apply badge bg-yellow-100 text-yellow-800;
    }
    .badge-danger {
        @apply badge bg-red-100 text-red-800;
    }
    .badge-gray {
        @apply badge bg-gray-100 text-gray-800;
    }
}
```

Update `tailwind.config.js`:

```js
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./**/*.{razor,html,cshtml}"],
  theme: {
    extend: {
      colors: {
        primary: "rgb(var(--color-primary) / <alpha-value>)",
        secondary: "rgb(var(--color-secondary) / <alpha-value>)",
      },
    },
  },
  plugins: [require("@tailwindcss/forms")],
};
```

Add a build script to `package.json`:

```json
{
  "scripts": {
    "css:build": "npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.css --minify",
    "css:watch": "npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.css --watch"
  }
}
```

Reference in `wwwroot/index.html`:

```html
<link href="css/app.css" rel="stylesheet" />
```

> **Dev workflow:** Run `npm run css:watch` in a separate terminal alongside `dotnet watch`.

---

## Step 2 — App Settings (`wwwroot/appsettings.json`)

```json
{
  "ApiBaseUrl": "https://api.seliseblocks.com",
  "BlocksKey": "",
  "ProjectSlug": "",
  "OidcClientId": "",
  "OidcRedirectUri": "http://localhost:5173/auth/callback",
  "PrimaryColor": "#15969B",
  "SecondaryColor": "#5194B8",
  "CaptchaSiteKey": "",
  "CaptchaType": ""
}
```

Create a strongly-typed settings class:

```csharp
// Models/AppSettings.cs
public class AppSettings
{
    public string ApiBaseUrl { get; set; } = "https://api.seliseblocks.com";
    public string BlocksKey { get; set; } = string.Empty;
    public string ProjectSlug { get; set; } = string.Empty;
    public string OidcClientId { get; set; } = string.Empty;
    public string OidcRedirectUri { get; set; } = "http://localhost:5173/auth/callback";
    public string PrimaryColor { get; set; } = "#15969B";
    public string SecondaryColor { get; set; } = "#5194B8";
    public string? CaptchaSiteKey { get; set; }
    public string? CaptchaType { get; set; }
}
```

---

## Step 3 — Auth State (`State/AuthState.cs`)

```csharp
// State/AuthState.cs
public class AuthState
{
    private readonly ILocalStorageService _localStorage;

    public bool IsAuthenticated { get; private set; }
    public string AccessToken { get; private set; } = string.Empty;
    public string RefreshToken { get; private set; } = string.Empty;
    public UserInfo? User { get; private set; }

    public event Action? OnAuthStateChanged;

    public AuthState(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        AccessToken = await _localStorage.GetItemAsStringAsync("access_token") ?? string.Empty;
        RefreshToken = await _localStorage.GetItemAsStringAsync("refresh_token") ?? string.Empty;
        IsAuthenticated = !string.IsNullOrEmpty(AccessToken);
    }

    public async Task LoginAsync(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        IsAuthenticated = true;
        await _localStorage.SetItemAsStringAsync("access_token", accessToken);
        await _localStorage.SetItemAsStringAsync("refresh_token", refreshToken);
        OnAuthStateChanged?.Invoke();
    }

    public async Task SetAccessTokenAsync(string accessToken)
    {
        AccessToken = accessToken;
        await _localStorage.SetItemAsStringAsync("access_token", accessToken);
    }

    public void SetUser(UserInfo user)
    {
        User = user;
        OnAuthStateChanged?.Invoke();
    }

    public async Task LogoutAsync()
    {
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
        IsAuthenticated = false;
        User = null;
        await _localStorage.RemoveItemAsync("access_token");
        await _localStorage.RemoveItemAsync("refresh_token");
        OnAuthStateChanged?.Invoke();
    }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}
```

---

## Step 4 — Authentication State Provider

```csharp
// Services/AuthStateProvider.cs
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

public class BlocksAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthState _authState;

    public BlocksAuthStateProvider(AuthState authState)
    {
        _authState = authState;
        _authState.OnAuthStateChanged += () =>
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_authState.IsAuthenticated || string.IsNullOrEmpty(_authState.AccessToken))
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        var claims = ParseClaimsFromJwt(_authState.AccessToken);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var kvp = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        return kvp?.Select(k => new Claim(k.Key, k.Value.ToString() ?? string.Empty)) ?? new List<Claim>();
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
```

---

## Step 5 — Token Delegating Handler

```csharp
// Services/TokenDelegatingHandler.cs
using Microsoft.AspNetCore.Components;

public class TokenDelegatingHandler : DelegatingHandler
{
    private readonly AuthState _authState;
    private readonly AppSettings _appSettings;
    private readonly IServiceProvider _serviceProvider;
    private bool _isRefreshing;

    public TokenDelegatingHandler(AuthState authState, AppSettings appSettings, IServiceProvider serviceProvider)
    {
        _authState = authState;
        _appSettings = appSettings;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_authState.AccessToken))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authState.AccessToken);
        }

        if (!request.Headers.Contains("x-blocks-key"))
        {
            request.Headers.Add("x-blocks-key", _appSettings.BlocksKey);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !_isRefreshing)
        {
            _isRefreshing = true;
            try
            {
                var refreshed = await TryRefreshTokenAsync(cancellationToken);
                if (refreshed)
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authState.AccessToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    await _authState.LogoutAsync();
                    var nav = _serviceProvider.GetRequiredService<NavigationManager>();
                    nav.NavigateTo("/login", forceLoad: true);
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        return response;
    }

    private async Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_authState.RefreshToken)) return false;

        var httpClient = new HttpClient { BaseAddress = new Uri(_appSettings.ApiBaseUrl) };
        httpClient.DefaultRequestHeaders.Add("x-blocks-key", _appSettings.BlocksKey);
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = _authState.RefreshToken,
            ["client_id"] = _appSettings.OidcClientId,
        });

        var response = await httpClient.PostAsync("/idp/v1/Authentication/Token", content, cancellationToken);
        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
        if (result is null) return false;

        await _authState.SetAccessTokenAsync(result.AccessToken);
        return true;
    }
}

public record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);
```

---

## Step 6 — GraphQL Client (`Services/GraphQLService.cs`)

Required only if using data-management schemas for CRUD queries.

```csharp
// Services/GraphQLService.cs
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

public class GraphQLService
{
    private readonly GraphQLHttpClient _client;
    private readonly AuthState _authState;

    public GraphQLService(HttpClient httpClient, AppSettings settings, AuthState authState)
    {
        _authState = authState;
        var options = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri($"{settings.ApiBaseUrl}/uds/v1/{settings.ProjectSlug}/graphql"),
        };

        // Uses the DI-registered HttpClient which includes TokenDelegatingHandler
        // for automatic token attachment and 401 refresh
        _client = new GraphQLHttpClient(options, new SystemTextJsonSerializer(), httpClient);
    }

    public async Task<T> QueryAsync<T>(string query, object? variables = null)
    {
        var request = new GraphQLRequest { Query = query, Variables = variables };
        var response = await _client.SendQueryAsync<T>(request);
        return response.Data;
    }

    public async Task<T> MutateAsync<T>(string mutation, object? variables = null)
    {
        var request = new GraphQLRequest { Query = mutation, Variables = variables };
        var response = await _client.SendMutationAsync<T>(request);
        return response.Data;
    }
}
```

---

## Step 7 — Toast Service (`Services/ToastService.cs`)

Replaces MudBlazor's `ISnackbar`. Used for success/error/info notifications.

```csharp
// Services/ToastService.cs
public class ToastService
{
    public event Action<ToastMessage>? OnShow;
    public event Action<Guid>? OnDismiss;

    public void Show(string message, ToastLevel level = ToastLevel.Info, int durationMs = 4000)
    {
        OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), message, level, durationMs));
    }

    public void Success(string message) => Show(message, ToastLevel.Success);
    public void Error(string message) => Show(message, ToastLevel.Error);
    public void Warning(string message) => Show(message, ToastLevel.Warning);
    public void Info(string message) => Show(message, ToastLevel.Info);

    public void Dismiss(Guid id) => OnDismiss?.Invoke(id);
}

public record ToastMessage(Guid Id, string Message, ToastLevel Level, int DurationMs);

public enum ToastLevel { Info, Success, Warning, Error }
```

---

## Step 8 — Program.cs

```csharp
// Program.cs
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Load settings
var appSettings = new AppSettings();
builder.Configuration.Bind(appSettings);
builder.Services.AddSingleton(appSettings);

// Auth
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSingleton<AuthState>();
builder.Services.AddScoped<AuthenticationStateProvider, BlocksAuthStateProvider>();
builder.Services.AddAuthorizationCore();

// HTTP Client with token handler
builder.Services.AddScoped<TokenDelegatingHandler>();
builder.Services.AddHttpClient("BlocksApi", client =>
{
    client.BaseAddress = new Uri(appSettings.ApiBaseUrl);
    client.DefaultRequestHeaders.Add("x-blocks-key", appSettings.BlocksKey);
})
.AddHttpMessageHandler<TokenDelegatingHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("BlocksApi"));

// Toast notifications
builder.Services.AddScoped<ToastService>();

// Localization
builder.Services.AddScoped<ILocalizationService, BlocksLocalizationService>();

// GraphQL (optional — only if using data-management schemas)
builder.Services.AddScoped<GraphQLService>();

// Feature services — register as features are built
// builder.Services.AddScoped<IAuthService, AuthService>();
// builder.Services.AddScoped<IDataManagementService, DataManagementService>();

var host = builder.Build();

// Initialize auth state from local storage before rendering
var authState = host.Services.GetRequiredService<AuthState>();
await authState.InitializeAsync();

// Load translations
var localization = host.Services.GetRequiredService<ILocalizationService>();
await localization.LoadTranslationsAsync();

await host.RunAsync();
```

---

## Step 9 — App.razor

```razor
@* App.razor *@
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="routeData"
                                DefaultLayout="typeof(Layout.MainLayout)">
                <NotAuthorized>
                    <RedirectToLogin />
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <LayoutView Layout="typeof(Layout.EmptyLayout)">
                <div class="flex items-center justify-center min-h-screen">
                    <p class="text-2xl font-semibold text-red-600">Page not found</p>
                </div>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>

<ToastContainer />
```

> No theme provider needed — theming is handled by Tailwind CSS variables in `:root`.

---

## Step 10 — Global Imports (`_Imports.razor`)

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.AspNetCore.WebUtilities
@using Microsoft.JSInterop
@using Blazored.LocalStorage
@using MyApp
@using MyApp.Components.Shared
@using MyApp.Components.Core
@using MyApp.Layout
@using MyApp.Models
@using MyApp.Services
@using MyApp.State
```

---

## Step 11 — RedirectToLogin Component

```razor
@* Components/Shared/RedirectToLogin.razor *@
@inject NavigationManager Navigation

@code {
    protected override void OnInitialized()
    {
        Navigation.NavigateTo("/login", forceLoad: false);
    }
}
```

---

## Step 12 — Dynamic Theme Colors via JS Interop

To apply `PrimaryColor` / `SecondaryColor` from `appsettings.json` at runtime, add a small JS helper:

`wwwroot/js/theme.js`:

```js
window.setThemeColors = (primary, secondary) => {
  const toRgb = (hex) => {
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    return `${r} ${g} ${b}`;
  };
  document.documentElement.style.setProperty("--color-primary", toRgb(primary));
  document.documentElement.style.setProperty("--color-secondary", toRgb(secondary));
};
```

Reference in `wwwroot/index.html`:

```html
<script src="js/theme.js"></script>
```

Call on app init in `App.razor`:

```razor
@inject IJSRuntime JS
@inject AppSettings AppSettings

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("setThemeColors", AppSettings.PrimaryColor, AppSettings.SecondaryColor);
        }
    }
}
```

---

## Output Summary

| File | Purpose |
|------|---------|
| `wwwroot/appsettings.json` | Client-side configuration |
| `Models/AppSettings.cs` | Typed settings |
| `State/AuthState.cs` | Persisted auth state |
| `Services/AuthStateProvider.cs` | Blazor `AuthenticationStateProvider` |
| `Services/TokenDelegatingHandler.cs` | Auto-attach tokens + 401 refresh |
| `Services/GraphQLService.cs` | GraphQL client for data-management |
| `Services/LocalizationService.cs` | Remote JSON-based i18n |
| `Services/ToastService.cs` | Toast notification service |
| `Styles/app.css` | Tailwind source with component classes |
| `tailwind.config.js` | Tailwind configuration with theme colors |
| `wwwroot/js/theme.js` | Runtime theme color injection |
| `wwwroot/css/app.css` | Compiled Tailwind output |
| `Program.cs` | DI + service registration |
| `App.razor` | Root component with router |
| `_Imports.razor` | Global using directives |

---

## Rules

- Run this skill once at project start. Do not re-run it if the files already exist — edit them instead.
- When adding a new domain's services, register them in `Program.cs`.
- Never duplicate the auth state or HTTP client — all modules inject `HttpClient` and `AuthState` from DI.
- `GraphQLService` can be omitted if the project does not use data-management schemas.
- Run `npm run css:watch` during development so Tailwind recompiles on Razor file changes.
- Always use the Tailwind component classes defined in `Styles/app.css` (`.btn-primary`, `.input`, `.card`, etc.) for consistency.
- Use CSS variables (`--color-primary`, `--color-secondary`) via `tailwind.config.js` — never hardcode hex values in components.
