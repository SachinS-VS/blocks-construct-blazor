# App Scaffold — Blazor WebAssembly

Covers everything needed to initialise a new .NET 10 Blazor WASM project that uses SELISE Blocks services.

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

# UI Components
dotnet add package MudBlazor

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
        return kvp?.Select(k => new Claim(k.Key, k.Value.ToString() ?? string.Empty)) ?? [];
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

## Step 7 — Program.cs

```csharp
// Program.cs
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
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

// MudBlazor
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.SnackbarVariant = MudBlazor.Variant.Filled;
});

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

## Step 8 — App.razor

```razor
@* App.razor *@
<MudThemeProvider Theme="_theme" @bind-IsDarkMode="_isDarkMode" />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />

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
                <MudText Typo="Typo.h4" Color="Color.Error">Page not found</MudText>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>

@code {
    private bool _isDarkMode;
    private MudTheme _theme = default!;

    [Inject] private AppSettings AppSettings { get; set; } = default!;

    protected override void OnInitialized()
    {
        _theme = new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = AppSettings.PrimaryColor,
                Secondary = AppSettings.SecondaryColor,
                AppbarBackground = "#FFFFFF",
                Background = "#FAFAFA",
                Surface = "#FFFFFF",
            },
            PaletteDark = new PaletteDark
            {
                Primary = AppSettings.PrimaryColor,
                Secondary = AppSettings.SecondaryColor,
            },
            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = new[] { "Nunito Sans", "sans-serif" }
                }
            }
        };
    }
}
```

---

## Step 9 — Global Imports (`_Imports.razor`)

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
@using MudBlazor
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

## Step 10 — RedirectToLogin Component

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

## Step 11 — Global CSS (`wwwroot/css/app.css`)

```css
@import url('https://fonts.googleapis.com/css2?family=Nunito+Sans:wght@400;500;600;700&display=swap');

html, body {
    font-family: 'Nunito Sans', sans-serif;
}

#app {
    min-height: 100vh;
}

/* MudBlazor overrides for consistent styling */
.mud-appbar {
    border-bottom: 1px solid var(--mud-palette-lines-default);
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
| `Program.cs` | DI + service registration |
| `App.razor` | Root component with MudBlazor providers + router |
| `_Imports.razor` | Global using directives |
| `wwwroot/css/app.css` | Global styles |

---

## Rules

- Run this skill once at project start. Do not re-run it if the files already exist — edit them instead.
- When adding a new domain's services, register them in `Program.cs`.
- Never duplicate the auth state or HTTP client — all modules inject `HttpClient` and `AuthState` from DI.
- `GraphQLService` can be omitted if the project does not use data-management schemas.
- `MudThemeProvider` must be in `App.razor`, not `MainLayout.razor`, to cover all routes.
